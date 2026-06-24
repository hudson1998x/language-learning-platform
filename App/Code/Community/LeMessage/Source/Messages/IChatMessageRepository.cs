using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.LeMessage.Messages;

[Repository(typeof(ChatMessage))]
public interface IChatMessageRepository : IEntityRepository<ChatMessage>
{
    [Query("ConversationId = :conversationId")]
    Task<List<ChatMessage>> FindByConversationAsync(Guid conversationId, UserContext context, DataOptions dataOptions, SortOption sort, Pagination pagination);
}
