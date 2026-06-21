using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.FlashCards.FlashCards;

[Repository(typeof(FlashCard))]
public interface IFlashCardRepository : IEntityRepository<FlashCard>
{
    [Query("CorrectCount <= IncorrectCount")]
    public Task<List<FlashCard>> GetStudySessionFlashCards(UserContext context, DataOptions dataOptions, Pagination pagination, SortOption sort);
}