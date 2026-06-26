using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LLE.Kernel.Attributes;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.LeMessage.Conversations;
using LLE.LeMessage.Dto;
using LLE.LeMessage.Messages;
using LLE.LeMessage.Profiles;
using LLE.LLMFramework.Models;
using LLE.LLMFramework.Services;

namespace LLE.LeMessage;

[Service]
public class LeMessageService
{
    private const int MaxTokens = 4096;
    private const int HistoryLimit = 10;
    private const int TranslationMaxTokens = 2048;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex ThinkBlockRegex = new(
        @"<think>[\s\S]*?</think>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string ChatOutputSchema =
        "You MUST respond with exactly one valid JSON object. " +
        "Do NOT wrap your JSON response inside markdown blocks (do not use ```json). " +
        "Do NOT include any text, preamble, or explanations outside the JSON object.\n\n" +
        "SCHEMA:\n" +
        "{\n" +
        "  \"message\": \"<your natural conversational response>\",\n" +
        "  \"correction\": null or {\n" +
        "    \"mistake\": \"<what the learner said wrong>\",\n" +
        "    \"corrected\": \"<the correct version>\",\n" +
        "    \"explanation\": \"<brief grammar or vocabulary note in English>\"\n" +
        "  }\n" +
        "}\n\n" +
        "STRICT RULES FOR JSON STABILITY:\n" +
        "- NEVER use unescaped double quotes (\") inside text values. Use single quotes (') instead for examples or emphasized text.\n" +
        "- If the learner made no mistake, set \"correction\" to null.\n" +
        "- If the learner made a mistake, correct them politely and conversationally inline, " +
        "AND populate the correction object.\n" +
        "- The \"message\" field must contain ONLY your conversational response " +
        "(the learner should never see the JSON structure).\n" +
        "- All string values must be properly escaped for valid JSON.\n" +
        "- Never leave fields empty. If no correction, use null.";

    public async Task<ApiResponse<StartConversationResponse>> StartConversationAsync(
        StartConversationRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileId))
            return ResponseFail<StartConversationResponse>("ProfileId is required");

        var profileRepo = RepositoryCatalog.GetRepository<IProfileRepository>();
        var profile = await profileRepo.FindByIdAsync(Guid.Parse(request.ProfileId), ctx, DataOptions.Default);

        if (profile is null)
            return ResponseFail<StartConversationResponse>("Profile not found");

        var conversationRepo = RepositoryCatalog.GetRepository<IConversationRepository>();
        var conversation = await conversationRepo.CreateAsync(new Conversation
        {
            ProfileId = profile.Id,
            UserId = ctx.UserId ?? Guid.Empty,
            Title = profile.Name
        }, ctx, DataOptions.Default);

        var llmService = ServiceCatalog.GetService<LLMService>();
        var greeting = await QueryLlmAsync(llmService, profile.SystemPrompt,
            "Start the conversation with a friendly greeting in character. Keep it brief — 2-3 sentences.");

        if (string.IsNullOrWhiteSpace(greeting))
            greeting = $"Hello! I'm {profile.Name}. How can I help you today?";

        var messageRepo = RepositoryCatalog.GetRepository<IChatMessageRepository>();
        var assistantMsg = await messageRepo.CreateAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = greeting
        }, ctx, DataOptions.Default);

        return new ApiResponse<StartConversationResponse>
        {
            Success = true,
            Data = new StartConversationResponse
            {
                ConversationId = conversation.Id.ToString(),
                ProfileId = profile.Id.ToString(),
                ProfileName = profile.Name,
                ProfileAvatarUrl = profile.AvatarUrl,
                Greeting = new ChatMessageDto
                {
                    Id = assistantMsg.Id.ToString(),
                    Role = assistantMsg.Role,
                    Content = assistantMsg.Content,
                    CreatedAt = assistantMsg.CreateTime
                }
            }
        };
    }

    public async Task<ApiResponse<ListConversationsResponse>> GetUserConversationsAsync(UserContext ctx)
    {
        if (ctx.UserId is null)
            return ResponseFail<ListConversationsResponse>("User not authenticated");

        var conversationRepo = RepositoryCatalog.GetRepository<IConversationRepository>();
        var messageRepo = RepositoryCatalog.GetRepository<IChatMessageRepository>();
        var profileRepo = RepositoryCatalog.GetRepository<IProfileRepository>();

        var conversations = await conversationRepo.FindByUserAsync(
            ctx.UserId.Value, ctx, DataOptions.Default,
            new SortOption { Field = "CreateTime", Ascending = false },
            new Pagination { PageNo = 1, Limit = 100 });

        var summaries = new List<ConversationSummary>();

        foreach (var conv in conversations)
        {
            var profile = await profileRepo.FindByIdAsync(conv.ProfileId, ctx, DataOptions.Default);
            var lastMessages = await messageRepo.FindByConversationAsync(
                conv.Id, ctx, DataOptions.Default,
                new SortOption { Field = "CreateTime", Ascending = false },
                new Pagination { PageNo = 1, Limit = 1 });

            summaries.Add(new ConversationSummary
            {
                Id = conv.Id.ToString(),
                ProfileId = conv.ProfileId.ToString(),
                ProfileName = profile?.Name ?? "Unknown",
                ProfileAvatarUrl = profile?.AvatarUrl ?? string.Empty,
                LastMessage = lastMessages.Count > 0 ? lastMessages[0].Content : string.Empty,
                LastMessageTime = lastMessages.Count > 0 ? lastMessages[0].CreateTime : conv.CreateTime,
                CreateTime = conv.CreateTime
            });
        }

        return new ApiResponse<ListConversationsResponse>
        {
            Success = true,
            Data = new ListConversationsResponse { Conversations = summaries }
        };
    }

    public async Task<ApiResponse<SendMessageResponse>> SendMessageAsync(
        SendMessageRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId))
            return ResponseFail<SendMessageResponse>("ConversationId is required");

        if (string.IsNullOrWhiteSpace(request.Message))
            return ResponseFail<SendMessageResponse>("Message is required");

        var conversationRepo = RepositoryCatalog.GetRepository<IConversationRepository>();
        var conversation = await conversationRepo.FindByIdAsync(
            Guid.Parse(request.ConversationId), ctx, DataOptions.Default);

        if (conversation is null)
            return ResponseFail<SendMessageResponse>("Conversation not found");

        if (ctx.UserId is null || conversation.UserId != ctx.UserId.Value)
            return ResponseFail<SendMessageResponse>("Access denied");

        var messageRepo = RepositoryCatalog.GetRepository<IChatMessageRepository>();
        var userMsg = await messageRepo.CreateAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Message
        }, ctx, DataOptions.Default);

        var profileRepo = RepositoryCatalog.GetRepository<IProfileRepository>();
        var profile = await profileRepo.FindByIdAsync(conversation.ProfileId, ctx, DataOptions.Default);

        var recentMessages = await messageRepo.FindByConversationAsync(
            conversation.Id, ctx, DataOptions.Default,
            new SortOption { Field = "CreateTime", Ascending = false },
            new Pagination { PageNo = 1, Limit = HistoryLimit });

        recentMessages.Reverse();

        var llmService = ServiceCatalog.GetService<LLMService>();
        var conversationHistory = string.Join("\n", recentMessages.Select(m =>
            $"<{m.Role}>\n{m.Content}\n</{m.Role}>"));

        var userPrompt =
            $"<conversation_history>\n{conversationHistory}\n</conversation_history>\n\n" +
            $"<current_input>\n{request.Message}\n</current_input>\n\n" +
            $"Respond as your character would. Keep your response natural and conversational.";

        var systemPrompt = profile?.SystemPrompt ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            systemPrompt += "\n\n" + ChatOutputSchema;

        var rawResponse = await QueryLlmAsync(llmService, systemPrompt, userPrompt);

        string displayContent;
        Correction? correction = null;

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            displayContent = "I'm not sure how to respond to that. Could you tell me more?";
        }
        else
        {
            var parseResult = TryParseResponse(rawResponse);
            displayContent = parseResult.message;
            correction = parseResult.correction;

            if (string.IsNullOrWhiteSpace(displayContent))
                displayContent = rawResponse;
        }

        var assistantMsg = await messageRepo.CreateAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = displayContent
        }, ctx, DataOptions.Default);

        return new ApiResponse<SendMessageResponse>
        {
            Success = true,
            Data = new SendMessageResponse
            {
                UserMessage = new ChatMessageDto
                {
                    Id = userMsg.Id.ToString(),
                    Role = userMsg.Role,
                    Content = userMsg.Content,
                    CreatedAt = userMsg.CreateTime
                },
                AssistantMessage = new ChatMessageDto
                {
                    Id = assistantMsg.Id.ToString(),
                    Role = assistantMsg.Role,
                    Content = assistantMsg.Content,
                    CreatedAt = assistantMsg.CreateTime
                },
                Correction = correction
            }
        };
    }

    public async Task<ApiResponse<TranslateResponse>> TranslateMessageAsync(
        TranslateRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return new ApiResponse<TranslateResponse>
            {
                Success = false,
                Message = "Text is required"
            };

        var llmService = ServiceCatalog.GetService<LLMService>();

        var systemPrompt =
            "You are a translation assistant. Split the given text into individual sentences. " +
            "Translate each sentence to English. Return ONLY a valid JSON array of objects. " +
            "Do NOT wrap in markdown blocks.\n\n" +
            "SCHEMA:\n" +
            "[\n" +
            "  {\n" +
            "    \"original\": \"<original sentence>\",\n" +
            "    \"translated\": \"<English translation>\"\n" +
            "  }\n" +
            "]\n\n" +
            "If the text has only one sentence, return an array with one object. " +
            "Preserve the exact original wording in the \"original\" field.";

        var userPrompt = $"Translate this text to English, sentence by sentence:\n\n{request.Text}";

        var rawResponse = await QueryLlmAsync(llmService, systemPrompt, userPrompt);

        if (string.IsNullOrWhiteSpace(rawResponse))
            return new ApiResponse<TranslateResponse>
            {
                Success = false,
                Message = "LLM returned an empty response"
            };

        var pairs = TryParseSentencePairs(rawResponse);

        if (pairs.Count == 0)
        {
            pairs.Add(new SentencePair
            {
                Original = request.Text,
                Translated = rawResponse
            });
        }

        return new ApiResponse<TranslateResponse>
        {
            Success = true,
            Data = new TranslateResponse { Pairs = pairs }
        };
    }

    private static (string message, Correction? correction) TryParseResponse(string raw)
    {
        try
        {
            var withoutThink = ThinkBlockRegex.Replace(raw, string.Empty).Trim();
            var json = StripMarkdownFences(withoutThink);

            if (string.IsNullOrWhiteSpace(json))
                return (raw.Trim(), null);

            var jsonStart = json.IndexOf('{');
            var jsonEnd = json.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return (raw.Trim(), null);

            json = json[jsonStart..(jsonEnd + 1)];

            // Try standard JSON parsing first
            try
            {
                var node = JsonNode.Parse(json, null, new JsonDocumentOptions { AllowTrailingCommas = true });
                if (node is not null)
                {
                    var message = node["message"]?.GetValue<string>()?.Trim() ?? string.Empty;

                    Correction? correction = null;
                    var correctionNode = node["correction"];
                    if (correctionNode is not null && correctionNode is JsonObject)
                    {
                        var mistake = correctionNode["mistake"]?.GetValue<string>()?.Trim() ?? string.Empty;
                        var corrected = correctionNode["corrected"]?.GetValue<string>()?.Trim() ?? string.Empty;
                        var explanation = correctionNode["explanation"]?.GetValue<string>()?.Trim() ?? string.Empty;

                        if (!string.IsNullOrWhiteSpace(mistake) && !string.IsNullOrWhiteSpace(corrected))
                        {
                            correction = new Correction
                            {
                                Mistake = mistake,
                                Corrected = corrected,
                                Explanation = explanation
                            };
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                        return (message, correction);
                }
            }
            catch
            {
                // Fall through to manual regex extraction if unescaped inner quotes broke the JSON parser
            }

            // Fallback: Manually extract message content between known string boundaries
            var messageMatch = Regex.Match(json, @"""message""\s*:\s*""([\s\S]*?)""\s*,\s*""correction""", RegexOptions.IgnoreCase);
            if (messageMatch.Success)
            {
                var extractedMessage = messageMatch.Groups[1].Value.Trim();
                if (extractedMessage.EndsWith(",")) 
                    extractedMessage = extractedMessage.TrimEnd(',').TrimEnd('"');

                return (extractedMessage, null);
            }

            return (raw.Trim(), null);
        }
        catch
        {
            return (raw.Trim(), null);
        }
    }

    private static List<SentencePair> TryParseSentencePairs(string raw)
    {
        try
        {
            var withoutThink = ThinkBlockRegex.Replace(raw, string.Empty).Trim();
            var json = StripMarkdownFences(withoutThink);

            if (string.IsNullOrWhiteSpace(json))
                return [];

            var jsonStart = json.IndexOf('[');
            var jsonEnd = json.LastIndexOf(']');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return [];

            json = json[jsonStart..(jsonEnd + 1)];

            var array = JsonNode.Parse(json, null, new JsonDocumentOptions { AllowTrailingCommas = true })?.AsArray();
            if (array is null || array.Count == 0) return [];

            var pairs = new List<SentencePair>();
            foreach (var item in array)
            {
                if (item is null) continue;
                var original = item["original"]?.GetValue<string>()?.Trim() ?? string.Empty;
                var translated = item["translated"]?.GetValue<string>()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(original) && !string.IsNullOrWhiteSpace(translated))
                {
                    pairs.Add(new SentencePair { Original = original, Translated = translated });
                }
            }

            return pairs;
        }
        catch
        {
            return [];
        }
    }

    private static async Task<string> QueryLlmAsync(
        LLMService llmService, string systemPrompt, string userPrompt)
    {
        try
        {
            var response = await llmService.SendMessageAsync(userPrompt, req =>
            {
                req.MaxTokens = MaxTokens;
                if (!string.IsNullOrWhiteSpace(systemPrompt))
                    req.Instructions.Add(new Instruction { Content = systemPrompt });
            });

            return response?.Content?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LeMessageService] LLM call failed: {ex.Message}");
            return string.Empty;
        }
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

    private static ApiResponse<T> ResponseFail<T>(string message) =>
        new() { Success = false, Message = message };
}