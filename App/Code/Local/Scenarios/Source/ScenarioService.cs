using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    private const int MaxTokens = 2000;
    private const int MaxRetries = 2;

    private static string BuildSystemPrompt(string scenarioTitle, string scenarioSteps, int difficulty, string language)
    {
        var difficultyLevel = difficulty switch
        {
            1 => "basic (use simple vocabulary, short sentences, slow pacing)",
            2 => "intermediate (use moderate vocabulary, compound sentences, natural pacing)",
            3 => "advanced (use complex vocabulary, idiomatic expressions, full-speed pacing)",
            _ => "intermediate"
        };
        
        

        return
            "You are a professional language instructor running an interactive practice session.\n\n" +
            $"SCENARIO: {scenarioTitle}\n" +
            $"STEPS/INSTRUCTIONS:\n{scenarioSteps}\n\n" +
            $"DIFFICULTY: {difficultyLevel}\n\n" +
            $"LANGUAGE: {language}\n\n" +
            "Rules:\n" +
            "  - Respond ONLY in the target learning language for the 'message' field. !!Never use English in 'message'!!.\n" +
            "  - Stay in character for the scenario. Act out the role the scenario describes.\n" +
            "  - Keep responses concise and appropriate for the difficulty level.\n" +
            "  - You MUST always respond with a raw JSON object and nothing else. No markdown fences, no prose.\n" +
            "  - The 'message' field MUST always be non-empty. It is the most important field.\n\n" +
            "Required JSON schema (all fields required, no field may be omitted or null):\n" +
            "{\n" +
            "  \"message\": \"<your in-character response in the target language — NEVER empty>\",\n" +
            "  \"translation\": \"<English translation of your response>\",\n" +
            "  \"pronunciation\": \"<phonetic romanisation — MANDATORY, never omit>\",\n" +
            "  \"culturalMeaning\": \"<1-2 sentences on cultural context if relevant, or empty string>\",\n" +
            "  \"hint\": \"<a helpful tip or correction for the learner, or empty string if not needed>\"\n" +
            "}\n\n" +
            "Pronunciation rules:\n" +
            "  - Break pronunciation into small, speakable chunks separated by spaces.\n" +
            "  - Use simple Latin letters only (a–z), plus optional diacritics for tone where required.\n" +
            "  - NEVER use IPA symbols or phonetic alphabets.\n" +
            "  - Chinese -> Pinyin (e.g. \"wo ai ni\")\n" +
            "  - Japanese -> Hepburn romaji (e.g. \"watashi wa anata ga suki desu\")\n" +
            "  - Korean -> Revised Romanization (e.g. \"saranghaeyo\")\n" +
            "  - Other -> best-effort phonetic spelling\n\n" +
            "IMPORTANT: Never respond in message in english, respond in the passed language\n\n" +
            "IMPORTANT: Output ONLY the JSON object. Do not include any text before or after it.";
    }

    /// <summary>
    /// Builds a synthetic opening exchange to prime the LLM when the user sends their very first message.
    /// Without this, models (especially smaller local ones) often fail to produce well-formed JSON
    /// because they have no prior example of the expected output format in the conversation window.
    /// </summary>
    private static string BuildPrimingContext(string scenarioTitle)
    {
        return
            $"[This is the start of a new scenario: \"{scenarioTitle}\". " +
            "The learner is about to speak first. " +
            "Remember: respond ONLY with the JSON object as specified. " +
            "The 'message' field must be non-empty and in the target language.]";
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
        var systemPrompt = BuildSystemPrompt(request.ScenarioTitle, request.ScenarioSteps, request.Difficulty, request.Language);

        // Build conversation context from history
        string userPrompt;
        var hasHistory = request.History is { Length: > 0 };

        if (!hasHistory)
        {
            // First message: include a priming hint so the model knows what's expected
            var priming = BuildPrimingContext(request.ScenarioTitle);
            userPrompt = $"{priming}\n\nThe learner says: {request.Message}";
        }
        else
        {
            var recentHistory = request.History!.TakeLast(6);
            var conversationContext = string.Join("\n", recentHistory.Select(h =>
                (h.Role == "user" ? "Learner" : "Instructor") + ": " + h.Content));
            userPrompt = $"Conversation so far:\n{conversationContext}\n\nThe learner now says: {request.Message}";
        }

        // Retry loop — small local models occasionally produce malformed output
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

            // On retry, append a gentle correction to the prompt
            if (attempt < MaxRetries - 1)
            {
                userPrompt += "\n\n[SYSTEM: Your previous response was not valid JSON or was missing the 'message' field. " +
                              "Please respond ONLY with the required JSON object. The 'message' field must not be empty.]";
            }
        }

        return new ApiResponse<ScenarioLine>
        {
            Success = false,
            Message = lastError ?? "Failed to get a valid response from the LLM"
        };
    }

    private static ApiResponse<ScenarioLine> TryParseScenarioLine(string raw, string originalUserMessage)
    {
        try
        {
            var json = StripMarkdownFences(raw);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new ApiResponse<ScenarioLine>
                {
                    Success = false,
                    Message = "Response was empty after stripping markdown fences"
                };
            }

            // Find the outermost JSON object in case the model prepended/appended stray text
            var jsonStart = json.IndexOf('{');
            var jsonEnd = json.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return new ApiResponse<ScenarioLine>
                {
                    Success = false,
                    Message = $"No JSON object found in LLM response. Raw: {Truncate(raw, 200)}"
                };
            }

            json = json[jsonStart..(jsonEnd + 1)];

            var node = JsonNode.Parse(json)
                       ?? throw new JsonException("Parsed JSON was null");

            var message = node["message"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var translation = node["translation"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var pronunciation = node["pronunciation"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var culturalMeaning = node["culturalMeaning"]?.GetValue<string>()?.Trim() ?? string.Empty;
            var hint = node["hint"]?.GetValue<string>()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                return new ApiResponse<ScenarioLine>
                {
                    Success = false,
                    Message = $"LLM returned empty 'message' field. Full JSON: {Truncate(json, 300)}"
                };
            }

            // Pronunciation is mandatory — fall back gracefully rather than failing
            if (string.IsNullOrWhiteSpace(pronunciation))
                pronunciation = "[pronunciation unavailable]";

            return new ApiResponse<ScenarioLine>
            {
                Success = true,
                Data = new ScenarioLine
                {
                    Original = originalUserMessage,
                    Message = message,
                    Translation = translation,
                    Pronunciation = pronunciation,
                    CulturalMeaning = culturalMeaning,
                    Hint = hint,
                    IsUser = false
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ScenarioLine>
            {
                Success = false,
                Message = $"Failed to parse LLM response: {ex.Message}. Raw: {Truncate(raw, 200)}"
            };
        }
    }

    /// <summary>
    /// Strips markdown code fences robustly, handling:
    ///   ```json\n...\n```
    ///   ```\n...\n```
    ///   Trailing whitespace/newlines after closing fence
    ///   Windows-style \r\n line endings
    /// </summary>
    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;

        // Find end of the opening fence line (may be ```json or just ```)
        var firstNewline = trimmed.IndexOfAny(['\n', '\r']);
        if (firstNewline < 0) return trimmed; // malformed, return as-is

        // Find the last ``` closing fence
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
            // Log properly in production; returning empty triggers the retry loop
            Console.Error.WriteLine($"[ScenarioService] LLM call failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static string Truncate(string s, int maxLength) =>
        s.Length <= maxLength ? s : s[..maxLength] + "…";
}