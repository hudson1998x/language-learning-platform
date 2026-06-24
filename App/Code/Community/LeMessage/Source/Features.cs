using LLE.Kernel.AutoEntity;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.LeMessage.Conversations;
using LLE.LeMessage.Dto;
using LLE.LeMessage.Messages;
using LLE.LeMessage.Profiles;

namespace LLE.LeMessage;

public static class Features
{
    public static void LoadFeatures()
    {
        AutoEntityFeature.AutoFeature<Profile, IProfileRepository>();
        AutoEntityFeature.AutoFeature<Conversation, IConversationRepository>();
        AutoEntityFeature.AutoFeature<ChatMessage, IChatMessageRepository>();

        FeatureRegistry.Add(new Feature<StartConversationRequest, ApiResponse<StartConversationResponse>>
        {
            FeatureName = "startConversation",
            FeatureGroup = "leMessage",
            Route = "/api/lemessage/chat/start",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var service = ServiceCatalog.GetService<LeMessageService>();
                return await service.StartConversationAsync(request, ctx);
            }
        });

        FeatureRegistry.Add(new Feature<SendMessageRequest, ApiResponse<SendMessageResponse>>
        {
            FeatureName = "sendMessage",
            FeatureGroup = "leMessage",
            Route = "/api/lemessage/chat/send",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var service = ServiceCatalog.GetService<LeMessageService>();
                return await service.SendMessageAsync(request, ctx);
            }
        });

        FeatureRegistry.Add(new Feature<object, ApiResponse<ListConversationsResponse>>
        {
            FeatureName = "listConversations",
            FeatureGroup = "leMessage",
            Route = "/api/lemessage/chat/conversations",
            Method = HttpMethod.Get,
            Handler = async (_, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var service = ServiceCatalog.GetService<LeMessageService>();
                return await service.GetUserConversationsAsync(ctx);
            }
        });

        FeatureRegistry.Add(new Feature<GetMessagesRequest, ApiResponse<GetMessagesResponse>>
        {
            FeatureName = "getMessages",
            FeatureGroup = "leMessage",
            Route = "/api/lemessage/chat/messages",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var messageRepo = RepositoryCatalog.GetRepository<IChatMessageRepository>();
                var conversationRepo = RepositoryCatalog.GetRepository<IConversationRepository>();

                var conversation = await conversationRepo.FindByIdAsync(
                    Guid.Parse(request.ConversationId), ctx, DataOptions.Default);

                if (conversation is null)
                {
                    return new ApiResponse<GetMessagesResponse>
                    {
                        Success = false,
                        Message = "Conversation not found"
                    };
                }

                if (ctx.UserId is null || conversation.UserId != ctx.UserId.Value)
                {
                    return new ApiResponse<GetMessagesResponse>
                    {
                        Success = false,
                        Message = "Access denied"
                    };
                }

                var messages = await messageRepo.FindByConversationAsync(
                    conversation.Id, ctx, DataOptions.Default,
                    new SortOption { Field = "CreateTime", Ascending = true },
                    new Pagination { PageNo = request.Page, Limit = request.Limit });

                return new ApiResponse<GetMessagesResponse>
                {
                    Success = true,
                    Data = new GetMessagesResponse
                    {
                        Messages = messages.Select(m => new ChatMessageDto
                        {
                            Id = m.Id.ToString(),
                            Role = m.Role,
                            Content = m.Content,
                            CreatedAt = m.CreateTime
                        }).ToList(),
                        TotalCount = messages.Count
                    }
                };
            }
        });
    }
}
