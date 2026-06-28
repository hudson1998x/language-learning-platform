using LLE.Kernel.Attributes;

namespace LLE.LLMFramework.Configurations;

[Configuration("AI integration", 1, "AI Settings")]
public class LLMConfiguration
{
    [ConfigComponent("@config/llm/provider-selector")]
    public string PreferredProvider { get; set; } = "Ollama";
}