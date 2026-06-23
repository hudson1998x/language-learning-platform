namespace LLE.LLMFramework.Models;

public class LLMRequest
{
    public string Prompt { get; set; } = string.Empty;
    public List<Message> History { get; set; } = new();
    public List<Instruction> Instructions { get; set; } = new();
    public ConversationContext Context { get; set; } = new();
    public PromptVariables Variables { get; set; } = new();
    public bool Stream { get; set; }
    public string? Model { get; set; }
    public int? MaxTokens { get; set; }
}
