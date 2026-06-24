using System.Text.Json;
using System.Text.RegularExpressions;
using LLE.HomeChat.Dto;
using LLE.Kernel.Attributes;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Models;
using LLE.LLMFramework.Services;

namespace LLE.HomeChat;

[Service]
public class HomeChatService
{
    private const int MaxTokens = 8192;
    private const int PronounceTokens = 512;

    public async Task<ApiResponse<HomeChatResponse>> SendAsync(HomeChatRequest request, string language)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new ApiResponse<HomeChatResponse>
            {
                Success = false,
                Message = "Message is required"
            };
        }

        var llmService = ServiceCatalog.GetService<LLMService>();
        var systemPrompt = BuildSystemPrompt(language);
        var userPrompt = BuildUserPrompt(request);

        var raw = await QueryLlmAsync(llmService, systemPrompt, userPrompt);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<HomeChatResponse>
            {
                Success = false,
                Message = "LLM returned an empty response"
            };
        }

        return TryParseResponse(raw);
    }

    public async Task<ApiResponse<PronounceResponse>> GeneratePronunciationAsync(PronounceRequest request, string language)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return new ApiResponse<PronounceResponse>
            {
                Success = false,
                Message = "Text is required"
            };
        }

        var llmService = ServiceCatalog.GetService<LLMService>();
        var prompt =
            $"You are a pronunciation assistant for language learners.\n\n" +
            $"CRITICAL: Your entire response MUST be a single raw JSON object. " +
            $"No markdown. No code fences. No explanation. No text before or after the JSON. " +
            $"If you output anything other than a JSON object, the app will break.\n\n" +
            $"The target language is {language}.\n\n" +
            $"Give me the pronunciation of this {language} text:\n" +
            $"{request.Text}\n\n" +
            $"Required JSON schema:\n" +
            $"{{\"pronunciation\": \"<phonetic guide using only lowercase a-z and spaces, no IPA>\"}}\n\n" +
            $"Start your output with {{ and end with }}. Nothing else.";

        var raw = await QueryLlmAsync(llmService, string.Empty, prompt);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<PronounceResponse>
            {
                Success = false,
                Message = "LLM returned an empty response"
            };
        }

        try
        {
            var jsonStart = raw.IndexOf('{');
            var jsonEnd = raw.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return new ApiResponse<PronounceResponse>
                {
                    Success = false,
                    Message = $"No JSON object found: {Truncate(raw, 200)}"
                };
            }

            var json = raw[jsonStart..(jsonEnd + 1)];
            var node = JsonDocument.Parse(json);
            var root = node.RootElement;

            var pronunciation = root.TryGetProperty("pronunciation", out var p)
                ? p.GetString()?.Trim() ?? ""
                : "";

            if (string.IsNullOrWhiteSpace(pronunciation))
            {
                return new ApiResponse<PronounceResponse>
                {
                    Success = false,
                    Message = "LLM returned empty pronunciation"
                };
            }

            return new ApiResponse<PronounceResponse>
            {
                Success = true,
                Data = new PronounceResponse { Pronunciation = pronunciation }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PronounceResponse>
            {
                Success = false,
                Message = $"Failed to parse: {ex.Message}. Raw: {Truncate(raw, 200)}"
            };
        }
    }

    private static string BuildSystemPrompt(string language)
    {
        return
            $"/no_think\n\n" +

            $"You are a language assistant for a {language} learning app.\n\n" +

            $"CRITICAL: Your entire response MUST be a single raw JSON object. " +
            $"No markdown. No code fences. No preamble. No text before or after the JSON. " +
            $"Do not think out loud. Do not explain. Output ONLY the JSON.\n\n" +

            $"Required JSON schema (all keys mandatory):\n" +
            $"{{\n" +
            $"  \"reply\": \"<your response in {language}, 50 words max>\",\n" +
            $"  \"translation\": \"<English translation of reply, 50 words max>\",\n" +
            $"  \"pronunciation\": \"<phonetic guide, lowercase a-z and spaces only, no IPA>\"\n" +
            $"}}\n\n" +

            $"Rules:\n" +
            $"- reply MUST be in {language} only, even if the user writes in English.\n" +
            $"- reply MUST be 50 words or fewer. Never list more than 5 items. Summarise instead.\n" +
            $"- pronunciation uses only lowercase a-z letters and spaces.\n" +
            $"- Begin your output with {{ and end with }}. Nothing else.";
    }

    private static string BuildUserPrompt(HomeChatRequest request)
    {
        var hasHistory = request.History is { Length: > 0 };
        var parts = new List<string>();

        if (hasHistory)
        {
            var recentHistory = request.History!.TakeLast(10);
            var context = string.Join("\n", recentHistory.Select(h =>
                $"<{h.Role}_turn>\n{h.Content}\n</{h.Role}_turn>"
            ));
            parts.Add($"<conversation_history>\n{context}\n</conversation_history>\n");
        }

        parts.Add($"<user_message>\n{request.Message}\n</user_message>");
        parts.Add("<instruction>Respond in the target language first, then provide the English translation. Output valid JSON.</instruction>");

        return string.Join("\n\n", parts);
    }

    private static async Task<string> QueryLlmAsync(LLMService llmService, string systemPrompt, string userPrompt)
    {
        try
        {
            var response = await llmService.SendMessageAsync("Ollama", userPrompt, req =>
            {
                req.MaxTokens = MaxTokens;
                req.Instructions.Add(new Instruction { Content = systemPrompt });
            });

            return response?.Content?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[HomeChatService] LLM call failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static ApiResponse<HomeChatResponse> TryParseResponse(string raw)
    {
        try
        {
            // Qwen3 may emit <think>...</think> blocks before the JSON — strip them
            var cleaned = Regex.Replace(raw, @"<think>.*?</think>", "", RegexOptions.Singleline).Trim();

            var jsonStart = cleaned.IndexOf('{');
            if (jsonStart < 0)
            {
                return new ApiResponse<HomeChatResponse>
                {
                    Success = false,
                    Message = $"No JSON object found in LLM response: {Truncate(raw, 200)}"
                };
            }

            var jsonEnd = cleaned.LastIndexOf('}');
            var isTruncated = jsonEnd <= jsonStart;

            string reply = "", translation = "", pronunciation = "";

            if (!isTruncated)
            {
                // Happy path — well-formed JSON
                try
                {
                    var root = JsonDocument.Parse(cleaned[jsonStart..(jsonEnd + 1)]).RootElement;
                    reply        = root.TryGetProperty("reply",         out var r) ? r.GetString()?.Trim() ?? "" : "";
                    translation  = root.TryGetProperty("translation",   out var t) ? t.GetString()?.Trim() ?? "" : "";
                    pronunciation= root.TryGetProperty("pronunciation", out var p) ? p.GetString()?.Trim() ?? "" : "";
                }
                catch (JsonException)
                {
                    isTruncated = true; // treat malformed JSON the same as truncated
                }
            }

            if (isTruncated)
            {
                // Regex fallback: extract whatever string values we can from the raw text
                Console.Error.WriteLine("[HomeChatService] Response truncated or malformed — attempting regex field extraction");
                reply        = ExtractJsonStringField(cleaned, "reply");
                translation  = ExtractJsonStringField(cleaned, "translation");
                pronunciation= ExtractJsonStringField(cleaned, "pronunciation");
            }

            if (string.IsNullOrWhiteSpace(reply))
            {
                return new ApiResponse<HomeChatResponse>
                {
                    Success = false,
                    Message = $"Could not extract 'reply' from LLM response: {Truncate(raw, 200)}"
                };
            }

            return new ApiResponse<HomeChatResponse>
            {
                Success = true,
                Data = new HomeChatResponse
                {
                    Reply = reply,
                    Translation = translation,
                    Pronunciation = pronunciation
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<HomeChatResponse>
            {
                Success = false,
                Message = $"Failed to parse LLM response: {ex.Message}. Raw: {Truncate(raw, 200)}"
            };
        }
    }

    /// <summary>
    /// Extracts a JSON string field value even from truncated/malformed JSON.
    /// Handles both complete values ("field": "value") and truncated ones ("field": "val...EOF).
    /// </summary>
    private static string ExtractJsonStringField(string text, string fieldName)
    {
        // Match "fieldName": "value" — greedy up to an unescaped closing quote or end of string
        var match = Regex.Match(text, $"\"{Regex.Escape(fieldName)}\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"?");
        if (!match.Success) return "";

        var value = match.Groups[1].Value;
        // Unescape basic JSON escape sequences
        return value
            .Replace("\\\"", "\"")
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\\", "\\")
            .Trim();
    }

    private static string Truncate(string s, int maxLength) =>
        s.Length <= maxLength ? s : s[..maxLength] + "…";
}