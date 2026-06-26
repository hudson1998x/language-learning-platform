using LLE.Kernel.Attributes;

namespace LLE.LLMFramework.Configurations;

[Configuration]
public class LLMConfiguration
{
    [ConfigComponent("@config/llm/provider-selector")]
    public string PreferredProvider { get; set; } = "Ollama";
}