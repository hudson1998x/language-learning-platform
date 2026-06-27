using System.Text.Json.Serialization;

namespace LLE.LLMProviders.ChatGPT.Models;

public class ChatGPTChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatGPTChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
}
