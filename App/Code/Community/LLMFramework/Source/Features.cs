using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Dto;
using LLE.LLMFramework.Services;

namespace LLE.LLMFramework;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<object, ApiResponse<LlmStatusResponse>>
        {
            FeatureName = "llmStatus",
            FeatureGroup = "llm",
            Route = "/api/llm/status",
            Method = HttpMethod.Get,
            Handler = async (_, _) =>
            {
                var llmService = ServiceCatalog.GetService<LLMService>();

                var status = new LlmStatusResponse
                {
                    Available = llmService.IsLlmAvailable(),
                    DefaultProvider = llmService.GetDefaultProvider()?.GetType().Name,
                    Providers = llmService.GetProviderStatus()
                };

                return new ApiResponse<LlmStatusResponse>
                {
                    Success = true,
                    Data = status
                };
            }
        });
    }
}
