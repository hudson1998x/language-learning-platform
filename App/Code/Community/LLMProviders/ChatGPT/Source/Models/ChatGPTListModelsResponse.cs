using System.Text.Json.Serialization;

namespace LLE.LLMProviders.ChatGPT.Models;

public class ChatGPTListModelsResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<ChatGPTModelInfo> Data { get; set; } = new();
}

public class ChatGPTModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;
}
