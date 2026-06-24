using System.Text.Json;
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
    private const int MaxTokens = 4096;

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

    private static string BuildSystemPrompt(string language)
    {
        return
            $"# ROLE\n" +
            $"You are a helpful language assistant for a language learning app.\n\n" +

            $"# TARGET LANGUAGE\n" +
            $"{language}\n\n" +

            $"# INSTRUCTIONS\n" +
            $"1. Respond naturally in {language} — answer the user's question or continue the conversation.\n" +
            $"2. After your {language} response, provide an English translation.\n" +
            $"3. Keep responses conversational and helpful.\n" +
            $"4. If the user writes in English, still respond in {language} first.\n\n" +

            $"# OUTPUT FORMAT\n" +
            $"Respond with exactly one valid JSON object with these keys:\n" +
            $"- \"reply\": (string) Your response entirely in {language}\n" +
            $"- \"translation\": (string) English translation of your reply\n" +
            $"Do NOT wrap in markdown code blocks. Output ONLY the raw JSON.";
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
            var jsonStart = raw.IndexOf('{');
            var jsonEnd = raw.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return new ApiResponse<HomeChatResponse>
                {
                    Success = false,
                    Message = $"No JSON object found in LLM response: {Truncate(raw, 200)}"
                };
            }

            var json = raw[jsonStart..(jsonEnd + 1)];
            var node = JsonDocument.Parse(json);
            var root = node.RootElement;

            var reply = root.TryGetProperty("reply", out var r) ? r.GetString()?.Trim() ?? "" : "";
            var translation = root.TryGetProperty("translation", out var t) ? t.GetString()?.Trim() ?? "" : "";

            if (string.IsNullOrWhiteSpace(reply))
            {
                return new ApiResponse<HomeChatResponse>
                {
                    Success = false,
                    Message = "LLM returned empty 'reply' field"
                };
            }

            return new ApiResponse<HomeChatResponse>
            {
                Success = true,
                Data = new HomeChatResponse
                {
                    Reply = reply,
                    Translation = translation
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

    private static string Truncate(string s, int maxLength) =>
        s.Length <= maxLength ? s : s[..maxLength] + "…";
}
