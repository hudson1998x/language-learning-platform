using System.Text.Json.Serialization;

namespace LLE.LLMProviders.MistralChat.Models;

public class MistralChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
