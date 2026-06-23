namespace LLE.LLMProviders.Ollama.Models;

public class OllamaChatRequest
{
    public string Model { get; set; } = string.Empty;
    public List<OllamaChatMessage> Messages { get; set; } = new();
    public bool Stream { get; set; }
    public OllamaOptions? Options { get; set; }
}

public class OllamaOptions
{
    public int? NumPredict { get; set; }
}
