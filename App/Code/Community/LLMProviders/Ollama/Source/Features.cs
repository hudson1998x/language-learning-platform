using System.Net.Http.Json;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.LLMProviders.Ollama.Models;

namespace LLE.LLMProviders.Ollama;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<string>>>
        {
            FeatureName = "listOllamaModels",
            FeatureGroup = "ollama",
            Route = "/api/ollama/models",
            Method = HttpMethod.Get,
            Handler = async (_, _) =>
            {
                var config = ConfigurationCatalog.GetConfiguration<OllamaConfiguration>();
                using var client = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };

                try
                {
                    var response = await client.GetFromJsonAsync<OllamaListModelsResponse>("/api/tags");
                    var models = response?.Models?.Select(m => m.Name).ToList() ?? [];
                    return new ApiResponse<List<string>> { Success = true, Data = models };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = $"Failed to fetch models from Ollama: {ex.Message}"
                    };
                }
            }
        });
    }
}
