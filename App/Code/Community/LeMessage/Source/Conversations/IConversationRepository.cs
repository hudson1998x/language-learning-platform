using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.LeMessage.Conversations;

[Repository(typeof(Conversation))]
public interface IConversationRepository : IEntityRepository<Conversation>
{
    [Query("UserId = :userId")]
    Task<List<Conversation>> FindByUserAsync(Guid userId, UserContext context, DataOptions dataOptions, SortOption sort, Pagination pagination);

    [Query("ProfileId = :profileId and UserId = :userId")]
    Task<List<Conversation>> FindByProfileAndUserAsync(Guid profileId, Guid userId, UserContext context, DataOptions dataOptions);
}
