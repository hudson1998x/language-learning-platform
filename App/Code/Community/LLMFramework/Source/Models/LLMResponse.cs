namespace LLE.LLMFramework.Models;

public class LLMResponse
{
    public string Content { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
