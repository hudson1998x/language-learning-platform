namespace LLE.LLMFramework.Dto;

public class LlmStatusResponse
{
    public bool Available { get; set; }
    public string? DefaultProvider { get; set; }
    public Dictionary<string, bool> Providers { get; set; } = new();
}
