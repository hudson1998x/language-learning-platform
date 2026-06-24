using System.Text.Json;
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
    private const string CorrectionDelimiter = "---CORRECTION---";

    private static readonly string CorrectionInstruction =
        $"When the learner makes a mistake in the target language, correct them politely and conversationally inline. " +
        $"For example: \"You said 'Salida el avion' but it's actually 'Salió el avión' — easy mix-up! 😊\"\n" +
        $"After your conversational response, if a correction was needed, append:\n" +
        $"{CorrectionDelimiter}\n" +
        $"{{\"mistake\": \"<what they said>\", \"corrected\": \"<correct version>\", \"explanation\": \"<brief grammar note>\"}}\n" +
        $"If no correction is needed, do not include the delimiter.";

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
            $"Respond as your character would. Keep your response natural and conversational. " +
            $"Do not wrap your response in XML tags or markdown.";

        var systemPrompt = profile?.SystemPrompt ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            systemPrompt += "\n\n" + CorrectionInstruction;

        var rawResponse = await QueryLlmAsync(llmService, systemPrompt, userPrompt);

        if (string.IsNullOrWhiteSpace(rawResponse))
            rawResponse = "I'm not sure how to respond to that. Could you tell me more?";

        var (displayContent, correction) = ExtractCorrection(rawResponse);

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

    private static async Task<string> QueryLlmAsync(
        LLMService llmService, string systemPrompt, string userPrompt)
    {
        try
        {
            var response = await llmService.SendMessageAsync("Ollama", userPrompt, req =>
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

    private static (string displayContent, Correction? correction) ExtractCorrection(string raw)
    {
        var delimiterIndex = raw.LastIndexOf(CorrectionDelimiter, StringComparison.Ordinal);
        if (delimiterIndex < 0)
            return (raw.Trim(), null);

        var displayPart = raw[..delimiterIndex].Trim();

        var jsonPart = raw[(delimiterIndex + CorrectionDelimiter.Length)..].Trim();

        try
        {
            var correction = JsonSerializer.Deserialize<Correction>(jsonPart, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (correction is not null &&
                !string.IsNullOrWhiteSpace(correction.Mistake) &&
                !string.IsNullOrWhiteSpace(correction.Corrected))
            {
                return (displayPart, correction);
            }
        }
        catch { }

        return (displayPart, null);
    }

    private static ApiResponse<T> ResponseFail<T>(string message) =>
        new() { Success = false, Message = message };
}
