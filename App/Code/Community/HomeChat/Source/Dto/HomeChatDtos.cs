namespace LLE.HomeChat.Dto;

public class HomeChatRequest
{
    public string Message { get; set; } = string.Empty;
    public HomeChatHistoryEntry[]? History { get; set; }
}

public class HomeChatHistoryEntry
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class HomeChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
}

public class PronounceRequest
{
    public string Text { get; set; } = string.Empty;
}

public class PronounceResponse
{
    public string Pronunciation { get; set; } = string.Empty;
}
