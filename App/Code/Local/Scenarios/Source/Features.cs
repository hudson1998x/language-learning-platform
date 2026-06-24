using LLE.Kernel.AutoEntity;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.Languages;
using LLE.Scenarios.Dto;
using Microsoft.AspNetCore.Http;

namespace LLE.Scenarios;

public static class Features
{
    public static void LoadFeatures()
    {
        AutoEntityFeature.AutoFeature<Scenario, IScenarioRepository>();

        FeatureRegistry.Add(new Feature<StartStudySessionRequest, ApiResponse<StartStudySessionResponse>>
        {
            FeatureName = "startStudySession",
            FeatureGroup = "scenario",
            Route = "/api/scenario/studysession/start",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var service = ServiceCatalog.GetService<ScenarioService>();
                return await service.StartStudySessionAsync(request, ctx);
            }
        });

        FeatureRegistry.Add(new Feature<SendMessageRequest, ApiResponse<ScenarioLine>>
        {
            FeatureName = "sendMessage",
            FeatureGroup = "scenario",
            Route = "/api/scenario/studysession/send",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);

                var languageId = GetLanguageId(httpContext);
                if (languageId is null)
                {
                    return new ApiResponse<ScenarioLine>
                    {
                        Success = false,
                        Message = "No language selected"
                    };
                }

                var langRepo = RepositoryCatalog.GetRepository<ILanguageRepository>();
                var lang = await langRepo.FindByIdAsync(languageId.Value, ctx, DataOptions.Bypass);
                if (lang is null)
                {
                    return new ApiResponse<ScenarioLine>
                    {
                        Success = false,
                        Message = "Language not found"
                    };
                }

                request.Language = lang.Name;

                var service = ServiceCatalog.GetService<ScenarioService>();
                return await service.SendMessageAsync(request, ctx);
            }
        });
    }

    private static Guid? GetLanguageId(HttpContext httpContext)
    {
        var idObj = httpContext.Session.GetString("Language");
        if (idObj is null) return null;
        return Guid.Parse(idObj);
    }
}
