namespace LLE.LLMProviders.Ollama.Models;

public class OllamaChatResponse
{
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OllamaChatMessage? Message { get; set; }
    public bool Done { get; set; }
}
