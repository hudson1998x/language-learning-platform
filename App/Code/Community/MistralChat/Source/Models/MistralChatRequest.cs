using System.Text.Json.Serialization;

namespace LLE.LLMProviders.MistralChat.Models;

public class MistralChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<MistralChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
}
