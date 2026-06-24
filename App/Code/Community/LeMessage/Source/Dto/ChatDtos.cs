namespace LLE.LeMessage.Dto;

public class StartConversationRequest
{
    public string ProfileId { get; set; } = string.Empty;
}

public class StartConversationResponse
{
    public string ConversationId { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string ProfileAvatarUrl { get; set; } = string.Empty;
    public ChatMessageDto Greeting { get; set; } = new();
}

public class SendMessageRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendMessageResponse
{
    public ChatMessageDto UserMessage { get; set; } = new();
    public ChatMessageDto AssistantMessage { get; set; } = new();
    public Correction? Correction { get; set; }
}

public class Correction
{
    public string Mistake { get; set; } = string.Empty;
    public string Corrected { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetMessagesRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}

public class GetMessagesResponse
{
    public List<ChatMessageDto> Messages { get; set; } = new();
    public int TotalCount { get; set; }
}

public class ConversationSummary
{
    public string Id { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string ProfileAvatarUrl { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public DateTime CreateTime { get; set; }
}

public class ListConversationsResponse
{
    public List<ConversationSummary> Conversations { get; set; } = new();
}
