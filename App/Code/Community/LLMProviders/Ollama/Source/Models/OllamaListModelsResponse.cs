namespace LLE.LLMProviders.Ollama.Models;

public class OllamaListModelsResponse
{
    public List<OllamaModelInfo> Models { get; set; } = new();
}

public class OllamaModelInfo
{
    public string Name { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public long Size { get; set; }
}
