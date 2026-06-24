namespace LLE.Scenarios.Dto;

public class StartStudySessionRequest
{
    public string ScenarioId { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1;
}

public class StartStudySessionResponse
{
    public string ScenarioId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Steps { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1;
}

public class SendMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string ScenarioTitle { get; set; } = string.Empty;
    public string ScenarioSteps { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1;
    public string Language { get; set; } = string.Empty;
    public ChatMessage[]? History { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Correct { get; set; } = true;
    public string Feedback { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
}

public class ScenarioLine
{
    public string Original { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string CulturalMeaning { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public bool Correct { get; set; } = true;
    public string Feedback { get; set; } = string.Empty;
    public bool IsUser { get; set; }
}
