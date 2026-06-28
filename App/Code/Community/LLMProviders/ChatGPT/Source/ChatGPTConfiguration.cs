using LLE.Kernel.Attributes;

namespace LLE.LLMProviders.ChatGPT;

[Configuration("AI integration", 2)]
[ConfigHelp("@help/chatgpt/connection-guide", "Connection Guide")]
public class ChatGPTConfiguration
{
    public bool Enabled { get; set; } = true;

    [FromEnvironment<string>("OPENAI_API_KEY", "")]
    public string ApiKey { get; set; } = string.Empty;

    [FromEnvironment<string>("OPENAI_URL", "https://api.openai.com")]
    public string BaseUrl { get; set; } = "https://api.openai.com";

    [ConfigComponent("@config/chatgpt/model-selector")]
    public string ModelName { get; set; } = "gpt-4o-mini";
}
