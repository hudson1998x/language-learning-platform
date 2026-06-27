using System.Text.Json.Serialization;

namespace LLE.LLMProviders.ChatGPT.Models;

public class ChatGPTChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
