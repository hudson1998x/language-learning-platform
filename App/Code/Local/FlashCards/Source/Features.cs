using LLE.FlashCards.Dto;
using LLE.FlashCards.FlashCards;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;

namespace LLE.FlashCards;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<StudySessionInitRequest, ApiResponse<List<FlashCard>>>
        {
            FeatureName = "getStudySession",
            FeatureGroup = "flashcard",
            Route = "/api/flashcard/studysession/start",
            Method = HttpMethod.Post,
            Handler = async (payload, httpContext) =>
            {
                var flashCards = RepositoryCatalog.GetRepository<IFlashCardRepository>();
                var context = UserContext.FromHttpContext(httpContext);
                
                var items = await flashCards.GetStudySessionFlashCards(
                    context,
                    DataOptions.Default,
                    new Pagination()
                    {
                        PageNo = 1,
                        Limit = payload.CardCount
                    },
                    new SortOption()
                    {
                        Field = "IncorrectCount",
                        Ascending = false
                    }
                );

                if (items.Count == 0)
                {
                    // pull some random items. 
                    items = await flashCards.FindAllAsync(
                        context,
                        DataOptions.Default,
                        new SortOption()
                        {
                            Field = "IncorrectCount",
                            Ascending = false
                        },
                        new Pagination()
                        {
                            PageNo = 1,
                            Limit = payload.CardCount
                        });
                }
                
                return new ApiResponse<List<FlashCard>>()
                {
                    Data = items,
                    Success = true,
                    Message = "Endpoint okay"
                };
            }
        });
        
        FeatureRegistry.Add(new Feature<StudySessionCardAnswerRequest, ApiResponse<FlashCard>>
        {
            FeatureName = "updateFlashCardScore",
            FeatureGroup = "flashcard",
            Route = "/api/flashcard/studysession/answer",
            Method = HttpMethod.Post,
            Handler = async (payload, httpContext) =>
            {
                var repo = RepositoryCatalog.GetRepository<IFlashCardRepository>();

                if (string.IsNullOrEmpty(payload.CardId))
                {
                    return new ApiResponse<FlashCard>()
                    {
                        Success = false,
                        Message = "CardId is empty",
                        Data = null
                    };
                }
                var parsedId = Guid.Parse(payload.CardId);

                if (parsedId == Guid.Empty)
                {
                    return new ApiResponse<FlashCard>()
                    {
                        Success = false,
                        Message = "CardId is invalid",
                        Data = null
                    };
                }
                
                var userContext = UserContext.FromHttpContext(httpContext);
                
                var card = await repo.FindByIdAsync(parsedId, userContext, DataOptions.Default);

                if (card is null)
                {
                    return new ApiResponse<FlashCard>()
                    {
                        Success = false,
                        Message = "Card does not exist",
                        Data = null
                    };
                }

                if (payload.IsCorrect)
                {
                    card.CorrectCount++;
                }
                else
                {
                    card.CorrectCount--;
                }
                card.LastReviewedUtc = DateTime.UtcNow;
                
                card = await repo.UpdateAsync(card, userContext, DataOptions.Default);

                return new ApiResponse<FlashCard>()
                {
                    Success = true,
                    Message = "Endpoint okay",
                    Data = card
                };
            }
        });
    }
}