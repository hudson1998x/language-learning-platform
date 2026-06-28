using LLE.FlashCards.Dto;
using LLE.FlashCards.FlashCards;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Dto;
using LLE.Kernel.Exceptions;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using Microsoft.AspNetCore.Http;

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
                var language = GetLanguageId(httpContext);

                if (language is null)
                {
                    return new ApiResponse<List<FlashCard>>()
                    {
                        Success = false,
                        Message = "Language is empty",
                        Data = null
                    };
                }

                var hasCategories = payload.Categories is { Count: > 0 };

                List<FlashCard> items;

                if (hasCategories)
                {
                    items = await flashCards.GetByCategories(
                        language.Value,
                        payload.Categories!,
                        context,
                        DataOptions.Default,
                        new SortOption()
                        {
                            Field = "IncorrectCount",
                            Ascending = false
                        }
                    );

                    if (items.Count > payload.CardCount)
                        items = items.Take(payload.CardCount).ToList();
                }
                else
                {
                    items = await flashCards.GetStudySessionFlashCards(
                        language.Value,
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

                    if (items.Count < payload.CardCount)
                    {
                        var remaining = payload.CardCount - items.Count;
                        var supplementalItems = await flashCards.GetPaginatedForLanguage(
                            language.Value,
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
                                Limit = remaining
                            });

                        items.AddRange(supplementalItems.Where(s => !items.Any(i => i.Id == s.Id)));
                    }
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
                    card.CorrectCount = Math.Max(0, card.CorrectCount - 1);
                }
                card.LastReviewedUtc = DateTime.UtcNow;
                card.ReviewCount++;
                
                card = await repo.UpdateAsync(card, userContext, DataOptions.Default);

                return new ApiResponse<FlashCard>()
                {
                    Success = true,
                    Message = "Endpoint okay",
                    Data = card
                };
            }
        });
        
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<FlashCard>>>()
        {
            FeatureName = $"listFlashCardsForLanguage",
            FeatureGroup = "flashcard",
            Route = $"/api/flashcard/for-language/{{pageNum}}/{{size}}/{{sortField}}/{{sortDir}}",
            Method = HttpMethod.Get,
            Handler = async (_, context) =>
            {
                if (!context.Request.RouteValues.TryGetValue("pageNum", out var pageNum) || pageNum is null)
                    throw new MalformedUrlException("Invalid/Missing parameter pageNum from the URL");

                if (!context.Request.RouteValues.TryGetValue("size", out var size) || size is null)
                    throw new MalformedUrlException("Invalid/Missing parameter size from the URL");

                if (!context.Request.RouteValues.TryGetValue("sortField", out var sortField) || sortField is null)
                    throw new MalformedUrlException("Invalid/Missing parameter sortField from the URL");

                if (!context.Request.RouteValues.TryGetValue("sortDir", out var sortDir) || sortDir is null)
                    throw new MalformedUrlException("Invalid/Missing parameter sortDir from the URL");

                var uc = UserContext.FromHttpContext(context);
                var language = GetLanguageId(context);

                if (language is null)
                {
                    return new ApiResponse<List<FlashCard>>()
                    {
                        Success = false,
                        Message = "Language is empty",
                        Data = null
                    };
                }
                
                var sortBy = new SortOption { Field = sortField.ToString()!, Ascending = sortDir.ToString() != "desc" };
                var repository = RepositoryCatalog.GetRepository<IFlashCardRepository>();
                var pagination = new Pagination { PageNo = int.Parse(pageNum.ToString()!), Limit = int.Parse(size.ToString()!) };
                return new ApiResponse<List<FlashCard>>
                {
                    Success = true,
                    Data = await repository.GetPaginatedForLanguage(language.Value, uc, DataOptions.Default, sortBy, pagination)
                };
            }
        });

        FeatureRegistry.Add(new Feature<object, ApiResponse<List<CategoryInfo>>>()
        {
            FeatureName = "getCategories",
            FeatureGroup = "flashcard",
            Route = "/api/flashcard/categories",
            Method = HttpMethod.Get,
            Handler = async (_, httpContext) =>
            {
                var repo = RepositoryCatalog.GetRepository<IFlashCardRepository>();
                var context = UserContext.FromHttpContext(httpContext);
                var language = GetLanguageId(httpContext);

                if (language is null)
                {
                    return new ApiResponse<List<CategoryInfo>>()
                    {
                        Success = false,
                        Message = "Language is empty",
                        Data = null
                    };
                }

                var cards = await repo.GetAllForLanguage(
                    language.Value,
                    context,
                    DataOptions.Default,
                    new SortOption { Field = "Category", Ascending = true }
                );

                var categories = cards
                    .Where(c => !string.IsNullOrWhiteSpace(c.Category))
                    .GroupBy(c => c.Category!.Trim())
                    .Select(g => new CategoryInfo { Name = g.Key, Count = g.Count() })
                    .OrderBy(c => c.Name)
                    .ToList();

                return new ApiResponse<List<CategoryInfo>>()
                {
                    Success = true,
                    Data = categories
                };
            }
        });
    }

    private static Guid? GetLanguageId(HttpContext httpContext)
    {
        var idObj = httpContext.Session.GetString("Language");

        if (idObj is null)
        {
            return null;
        }

        return Guid.Parse(idObj);
    }
}