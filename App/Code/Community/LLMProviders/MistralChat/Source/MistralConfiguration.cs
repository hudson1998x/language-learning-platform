using LLE.Kernel.Attributes;

namespace LLE.LLMProviders.MistralChat;

[Configuration]
[ConfigHelp("@help/mistral/connection-guide", "Connection Guide")]
public class MistralConfiguration
{
    public bool Enabled { get; set; } = true;

    [FromEnvironment<string>("MISTRAL_API_KEY", "")]
    public string ApiKey { get; set; } = string.Empty;

    [FromEnvironment<string>("MISTRAL_URL", "https://api.mistral.ai")]
    public string BaseUrl { get; set; } = "https://api.mistral.ai";

    [ConfigComponent("@config/mistral/model-selector")]
    public string ModelName { get; set; } = "mistral-small-latest";
}
