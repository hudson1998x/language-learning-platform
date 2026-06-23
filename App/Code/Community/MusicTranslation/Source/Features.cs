using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.MusicTranslation.Dto;
using LLE.MusicTranslation.Services;

namespace LLE.MusicTranslation;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<SongTranslationRequest, ApiResponse<SongTranslationResponse>>
        {
            FeatureName = "translateSong",
            FeatureGroup = "musicTranslation",
            Route = "/api/music/translateSong",
            Method = HttpMethod.Post,
            Handler = async (request, httpContext) =>
            {
                var ctx = UserContext.FromHttpContext(httpContext);
                var service = ServiceCatalog.GetService<MusicTranslationService>();

                try
                {
                    var result = await service.TranslateAsync(request, ctx);
                    return new ApiResponse<SongTranslationResponse>
                    {
                        Success = true,
                        Data = result
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<SongTranslationResponse>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
        });
    }
}
