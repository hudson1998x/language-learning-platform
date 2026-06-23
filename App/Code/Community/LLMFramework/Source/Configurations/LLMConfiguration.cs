using LLE.Kernel.Attributes;

namespace LLE.LLMFramework.Configurations;

[Configuration]
public class LLMConfiguration
{
    public string PreferredProvider { get; set; } = "Ollama";
}