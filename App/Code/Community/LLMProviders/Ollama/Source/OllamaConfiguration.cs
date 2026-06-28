using LLE.Kernel.Attributes;

namespace LLE.LLMProviders.Ollama;

[Configuration("AI integration", 4)]
public class OllamaConfiguration
{
    public bool Enabled { get; set; } = true;
    
    [FromEnvironment<string>("OLLAMA_URL", "http://localhost:11434/")]
    public string BaseUrl { get; set; } = "http://localhost:11434/";
    
    [ConfigComponent("@config/ollama/model-selector")]
    public string ModelName { get; set; } = "llama3";
}
