using System.Net.Http.Headers;
using System.Net.Http.Json;
using LLE.Kernel.Dto;
using LLE.Kernel.Registry;
using LLE.LLMProviders.ChatGPT.Models;

namespace LLE.LLMProviders.ChatGPT;

public static class Features
{
    public static void LoadFeatures()
    {
        FeatureRegistry.Add(new Feature<object, ApiResponse<List<string>>>
        {
            FeatureName = "listChatGptModels",
            FeatureGroup = "chatgpt",
            Route = "/api/chatgpt/models",
            Method = HttpMethod.Get,
            Handler = async (_, _) =>
            {
                var config = ConfigurationCatalog.GetConfiguration<ChatGPTConfiguration>();
                using var client = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", config.ApiKey);

                try
                {
                    var response = await client.GetFromJsonAsync<ChatGPTListModelsResponse>("/v1/models");
                    var models = response?.Data?.Select(m => m.Id).ToList() ?? [];
                    return new ApiResponse<List<string>> { Success = true, Data = models };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = $"Failed to fetch models from ChatGPT: {ex.Message}"
                    };
                }
            }
        });
    }
}
