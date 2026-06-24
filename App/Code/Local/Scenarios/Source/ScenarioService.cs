using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LLE.Kernel.Attributes;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.LLMFramework.Models;
using LLE.LLMFramework.Services;
using LLE.Scenarios.Dto;

namespace LLE.Scenarios;

[Service]
public class ScenarioService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // Qwen3 emits <think>...</think> blocks before its actual output.
    // This regex strips them so we can cleanly extract the JSON payload.
    private static readonly Regex ThinkBlockRegex = new(
        @"<think>[\s\S]*?</think>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "begin" (case-insensitive) is treated as a session-start trigger, not learner input.
    private const string SessionStartTrigger = "begin";

    private const int MaxTokens = 16000;
    private const int MaxRetries = 3;

    private static string BuildSystemPrompt(string scenarioTitle, string scenarioSteps, int difficulty, string language)
    {
        var difficultyLevel = difficulty switch
        {
            1 => "BASIC: Simple vocabulary, short sentences, slow pacing.",
            2 => "INTERMEDIATE: Moderate vocabulary, compound sentences, natural pacing.",
            3 => "ADVANCED: Complex vocabulary, idiomatic expressions, full-speed pacing.",
            _ => "INTERMEDIATE"
        };

        return
            $"# ROLE\n" +
            $"You are a strict backend engine for an interactive language learning system. You act exclusively as the language tutor.\n\n" +

            $"# SESSION CONTEXT\n" +
            $"* TARGET LANGUAGE: {language}\n" +
            $"* DIFFICULTY LEVEL: {difficultyLevel}\n" +
            $"* CURRENT SCENARIO: {scenarioTitle}\n" +
            $"* SCENARIO STEPS:\n{scenarioSteps}\n\n" +

            $"# STRICT INSTRUCTOR RULES\n" +
            $"1. Act ONLY as the instructor. NEVER write lines for the learner, never simulate learner behavior, and never continue the conversation on behalf of the learner.\n" +
            $"2. Advance through the SCENARIO STEPS in exact sequential order—one step per user turn.\n" +
            $"3. Your very first response in the session MUST introduce the scenario and open with the specific cue: \"The plane is landing, please stay seated.\"\n\n" +

            $"# OUTPUT FORMAT REQUIREMENTS\n" +
            $"* You MUST respond with exactly one valid JSON object.\n" +
            $"* Do NOT wrap your JSON response inside markdown blocks (do not use ```json).\n" +
            $"* Do NOT include any text, preamble, conversational pleasantries, or explanations outside the JSON object.\n" +
            $"* Ensure all keys and string values are properly escaped for valid JSON parsing.\n\n" +

            $"# SCHEMA DEFINITION\n" +
            $"Your response must contain exactly these 7 keys, and NONE of them should be empty string values:\n" +
            $"1. \"message\": (String) A direct imperative instruction to the learner, written ENTIRELY in the target language. Must never be empty.\n" +
            $"2. \"translation\": (String) The literal English translation of the user's *last* message. If this is the start of the session (no input yet), set this to \"[Session Started]\".\n" +
            $"3. \"pronunciation\": (String) A simplified phonetic guide for your target language `message` field. Use ONLY lowercase a-z letters and spaces. Do NOT use IPA symbols, brackets, or punctuation.\n" +
            $"4. \"correct\": (Boolean) true if the learner's response was accurate/acceptable, or if the session is starting. Otherwise, false.\n" +
            $"5. \"feedback\": (String) Written in English. Provide immediate guidance on how the learner should approach this scenario or step. Must never be empty.\n" +
            $"6. \"culturalMeaning\": (String) Written in English. Provide relevant cultural or contextual insight regarding this scenario step. Must never be empty.\n" +
            $"7. \"hint\": (String) A useful starter phrase or ideal target-language response to assist the learner with this step. Must never be empty.\n\n" +

            $"# BASELINE JSON TEMPLATE\n" +
            $"{{\n" +
            $"  \"message\": \"\",\n" +
            $"  \"translation\": \"\",\n" +
            $"  \"pronunciation\": \"\",\n" +
            $"  \"correct\": true,\n" +
            $"  \"feedback\": \"\",\n" +
            $"  \"culturalMeaning\": \"\",\n" +
            $"  \"hint\": \"\"\n" +
            $"\n}}";
    }

    /// <summary>
    /// Priming context for the very first turn. Explicitly tells the model that "begin"
    /// is a system trigger — not learner speech — so it never tries to translate it.
    /// </summary>
    private static string BuildSessionStartPrimingContext(string scenarioTitle)
    {
        return
            $"<system_directive>\n" +
            $"[SESSION INITIALIZATION: \"{scenarioTitle}\"]\n" +
            $"The user has triggered the session start via 'begin'.\n" +
            $"Do NOT leave fields empty. Fill them with initialization details:\n" +
            $"* Execute Step 1 of the scenario in the \"message\" field.\n" +
            $"* Set \"translation\" to \"[Session Started]\".\n" +
            $"* Set \"correct\" to true.\n" +
            $"* Set \"feedback\" to an introductory welcome message explaining the scenario goal in English.\n" +
            $"* Set \"culturalMeaning\" to a brief note on behavioral context or etiquette for this setting.\n" +
            $"* Set \"hint\" to a helpful starting word or phrase in the target language to assist their first step.\n" +
            $"* Output ONLY the raw JSON object.\n" +
            $"</system_directive>";
    }

    /// <summary>
    /// Priming context for the first real learner message (non-"begin" opening).
    /// </summary>
    private static string BuildFirstMessagePrimingContext(string scenarioTitle)
    {
        return
            $"<system_directive>\n" +
            $"[FIRST LEARNER INPUT for scenario: \"{scenarioTitle}\"]\n" +
            $"Evaluate the text inside the <input> tags. Provide the correct assessment, translate their statement into English inside \"translation\", provide coaching feedback, a cultural explanation, a phrase suggestion in \"hint\", and provide the next step.\n" +
            $"Do NOT leave any fields empty.\n" +
            $"</system_directive>";
    }

    public async Task<ApiResponse<StartStudySessionResponse>> StartStudySessionAsync(
        StartStudySessionRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.ScenarioId))
        {
            return new ApiResponse<StartStudySessionResponse>
            {
                Success = false,
                Message = "ScenarioId is required"
            };
        }

        var repo = RepositoryCatalog.GetRepository<IScenarioRepository>();
        var scenario = await repo.FindByIdAsync(Guid.Parse(request.ScenarioId), ctx, DataOptions.Default);

        if (scenario is null)
        {
            return new ApiResponse<StartStudySessionResponse>
            {
                Success = false,
                Message = "Scenario not found"
            };
        }

        return new ApiResponse<StartStudySessionResponse>
        {
            Success = true,
            Data = new StartStudySessionResponse
            {
                ScenarioId = scenario.Id.ToString(),
                Title = scenario.Title,
                Steps = scenario.Steps,
                Difficulty = request.Difficulty
            }
        };
    }

    public async Task<ApiResponse<ScenarioLine>> SendMessageAsync(SendMessageRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new ApiResponse<ScenarioLine>
            {
                Success = false,
                Message = "Message is required"
            };
        }

        var llmService = ServiceCatalog.GetService<LLMService>();
        var systemPrompt = BuildSystemPrompt(
            request.ScenarioTitle, request.ScenarioSteps, request.Difficulty, request.Language);

        var hasHistory = request.History is { Length: > 0 };

        // Detect the session-start trigger so we never ask the model to translate "begin".
        var isSessionStart = !hasHistory &&
            request.Message.Trim().Equals(SessionStartTrigger, StringComparison.OrdinalIgnoreCase);

        string userPrompt;

        if (!hasHistory)
        {
            if (isSessionStart)
            {
                var priming = BuildSessionStartPrimingContext(request.ScenarioTitle);
                userPrompt = $"{priming}\n\nStart the scenario now.";
            }
            else
            {
                var priming = BuildFirstMessagePrimingContext(request.ScenarioTitle);
                userPrompt = $"{priming}\n\n<current_input>\n{request.Message}\n</current_input>";
            }
        }
        else
        {
            var recentHistory = request.History!.TakeLast(6);
            var conversationContext = string.Join("\n", recentHistory.Select(h =>
            {
                if (h.Role == "user")
                    return $"  <learner_turn>{h.Content}</learner_turn>";

                return $"  <tutor_turn correct=\"{h.Correct.ToString().ToLower()}\">{h.Content}</tutor_turn>";
            }));

            userPrompt =
                $"<conversation_history>\n{conversationContext}\n</conversation_history>\n\n" +
                $"<current_input>\n{request.Message}\n</current_input>\n\n" +
                $"<instruction>\n" +
                $"Process the <current_input>. Evaluate correctness against the current scenario progress.\n" +
                $"All fields in the output schema are mandatory and must be completely populated with content. Do not use code blocks.\n" +
                $"</instruction>";
        }

        string? lastError = null;

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var raw = await QueryLlmAsync(llmService, systemPrompt, userPrompt, MaxTokens);

            if (string.IsNullOrWhiteSpace(raw))
            {
                lastError = "LLM returned an empty response";
                continue;
            }

            var parseResult = TryParseScenarioLine(raw, request.Message);
            if (parseResult.Success)
                return parseResult;

            lastError = parseResult.Message;

            if (attempt < MaxRetries - 1)
            {
                userPrompt +=
                    $"\n\n[SYSTEM CORRECTION — attempt {attempt + 2}/{MaxRetries}]: " +
                    $"Your previous response was rejected: {lastError}. " +
                    "You MUST output ONLY a valid JSON object. " +
                    "Every single field is mandatory and MUST be fully populated with actual analytical content—no empty strings or placeholders allowed.";
            }
        }

        return new ApiResponse<ScenarioLine>
        {
            Success = false,
            Message = lastError ?? "Failed to get a valid response from the LLM"
        };
    }

    private static ApiResponse<ScenarioLine> TryParseScenarioLine(
        string raw,
        string originalUserMessage)
    {
        try
        {
            // ── Step 1: strip Qwen3 thinking blocks ──────────────────────────────
            var withoutThink = ThinkBlockRegex.Replace(raw, string.Empty).Trim();

            // ── Step 2: strip markdown code fences ───────────────────────────────
            var json = StripMarkdownFences(withoutThink);

            if (string.IsNullOrWhiteSpace(json))
                return Fail("Response was empty after stripping think blocks and markdown fences");

            // ── Step 3: isolate the outermost JSON object ─────────────────────────
            var jsonStart = json.IndexOf('{');
            var jsonEnd   = json.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return Fail($"No JSON object found in response. Raw (truncated): {Truncate(raw, 200)}");

            json = json[jsonStart..(jsonEnd + 1)];

            // ── Step 4: parse ─────────────────────────────────────────────────────
            var node = JsonNode.Parse(json)
                       ?? throw new JsonException("Parsed JSON was null");

            var message         = node["message"]?.GetValue<string>()?.Trim()         ?? string.Empty;
            var translation     = node["translation"]?.GetValue<string>()?.Trim()     ?? string.Empty;
            var pronunciation   = node["pronunciation"]?.GetValue<string>()?.Trim()   ?? string.Empty;
            var culturalMeaning = node["culturalMeaning"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var hint            = node["hint"]?.GetValue<string>()?.Trim()            ?? string.Empty;
            var feedback        = node["feedback"]?.GetValue<string>()?.Trim()        ?? string.Empty;
            var correct         = node["correct"]?.GetValue<bool?>()                  ?? true;

            // ── Step 5: validate fields ────────────────────────────────
            if (string.IsNullOrWhiteSpace(message))
                return Fail($"LLM returned empty 'message' field. JSON: {Truncate(json, 300)}");

            if (string.IsNullOrWhiteSpace(translation))
                return Fail($"LLM returned empty 'translation' field. JSON: {Truncate(json, 300)}");

            if (string.IsNullOrWhiteSpace(feedback))
                return Fail($"LLM returned empty 'feedback' field. JSON: {Truncate(json, 300)}");

            if (string.IsNullOrWhiteSpace(hint))
                return Fail($"LLM returned empty 'hint' field. JSON: {Truncate(json, 300)}");

            // Pronunciation degrades gracefully — bad pronunciation shouldn't cause a retry.
            if (string.IsNullOrWhiteSpace(pronunciation) || IsPronunciationPlaceholder(pronunciation))
                pronunciation = BuildFallbackPronunciation(message);

            return new ApiResponse<ScenarioLine>
            {
                Success = true,
                Data = new ScenarioLine
                {
                    Original        = originalUserMessage,
                    Message         = message,
                    Translation     = translation,
                    Pronunciation   = pronunciation,
                    CulturalMeaning = culturalMeaning,
                    Hint            = hint,
                    Correct         = correct,
                    Feedback        = feedback,
                    IsUser          = false
                }
            };
        }
        catch (Exception ex)
        {
            return Fail($"Failed to parse LLM response: {ex.Message}. Raw: {Truncate(raw, 200)}");
        }
    }

    private static bool IsPronunciationPlaceholder(string pronunciation)
    {
        var lower = pronunciation.ToLowerInvariant();
        return lower is "unavailable" or "unknown" or "n/a" or "na" or "none"
            || lower.StartsWith("[") || lower.StartsWith("(");
    }

    private static string BuildFallbackPronunciation(string message)
    {
        var normalised = message.Normalize(System.Text.NormalizationForm.FormD);
        var cleaned = new string(normalised
            .Where(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or ' ')
            .ToArray())
            .ToLowerInvariant()
            .Trim();

        return string.IsNullOrWhiteSpace(cleaned) ? "see message above" : cleaned;
    }

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;

        var firstNewline = trimmed.IndexOfAny(['\n', '\r']);
        if (firstNewline < 0) return trimmed;

        var lastFence = trimmed.LastIndexOf("```", trimmed.Length - 1, StringComparison.Ordinal);
        if (lastFence <= firstNewline) return trimmed;

        return trimmed[(firstNewline + 1)..lastFence].Trim();
    }

    private static async Task<string> QueryLlmAsync(
        LLMService llmService,
        string systemPrompt,
        string userPrompt,
        int maxTokens)
    {
        try
        {
            var response = await llmService.SendMessageAsync("Ollama", userPrompt, req =>
            {
                req.MaxTokens = maxTokens;
                req.Instructions.Add(new Instruction { Content = systemPrompt });
            });

            return response?.Content?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ScenarioService] LLM call failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static ApiResponse<ScenarioLine> Fail(string message) =>
        new() { Success = false, Message = message };

    private static string Truncate(string s, int maxLength) =>
        s.Length <= maxLength ? s : s[..maxLength] + "…";
}