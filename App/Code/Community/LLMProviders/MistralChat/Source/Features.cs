using System.Net.Http.Headers;
using System.Net.Http.Json;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.LLMProviders.MistralChat.Models;

namespace LLE.LLMProviders.MistralChat;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<string>>>
        {
            FeatureName = "listMistralModels",
            FeatureGroup = "mistral",
            Route = "/api/mistral/models",
            Method = HttpMethod.Get,
            Handler = async (_, _) =>
            {
                var config = ConfigurationCatalog.GetConfiguration<MistralConfiguration>();
                using var client = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", config.ApiKey);

                try
                {
                    var response = await client.GetFromJsonAsync<MistralListModelsResponse>("/v1/models");
                    var models = response?.Data?.Select(m => m.Id).ToList() ?? [];
                    return new ApiResponse<List<string>> { Success = true, Data = models };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = $"Failed to fetch models from Mistral: {ex.Message}"
                    };
                }
            }
        });
    }
}
