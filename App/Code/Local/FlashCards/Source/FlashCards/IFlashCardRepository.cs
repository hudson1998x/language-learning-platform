using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;

namespace LLE.FlashCards.FlashCards;

[Repository(typeof(FlashCard))]
public interface IFlashCardRepository : IEntityRepository<FlashCard>
{
    
}