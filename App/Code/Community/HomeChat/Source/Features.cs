using LLE.HomeChat.Dto;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.Languages;
using Microsoft.AspNetCore.Http;

namespace LLE.HomeChat;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<HomeChatRequest, ApiResponse<HomeChatResponse>>
        {
            FeatureName = "sendMessage",
            FeatureGroup = "homechat",
            Route = "/api/homechat/send",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var languageId = GetLanguageId(httpContext);
                if (languageId is null)
                {
                    return new ApiResponse<HomeChatResponse>
                    {
                        Success = false,
                        Message = "No language selected"
                    };
                }

                var ctx = UserContext.FromHttpContext(httpContext);
                var langRepo = RepositoryCatalog.GetRepository<ILanguageRepository>();
                var lang = await langRepo.FindByIdAsync(languageId.Value, ctx, DataOptions.Bypass);
                if (lang is null)
                {
                    return new ApiResponse<HomeChatResponse>
                    {
                        Success = false,
                        Message = "Language not found"
                    };
                }

                var service = ServiceCatalog.GetService<HomeChatService>();
                return await service.SendAsync(request, lang.Name);
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
