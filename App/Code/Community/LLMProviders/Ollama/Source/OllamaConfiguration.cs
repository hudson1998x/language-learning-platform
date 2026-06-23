using LLE.Kernel.Attributes;

namespace LLE.LLMProviders.Ollama;

[Configuration]
public class OllamaConfiguration
{
    public string BaseUrl { get; set; } = "http://localhost:11434/";
    public string ModelName { get; set; } = "llama3";
}
