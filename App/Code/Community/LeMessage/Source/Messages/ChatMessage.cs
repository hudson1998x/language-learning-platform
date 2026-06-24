using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.LeMessage.Messages;

[Entity]
public class ChatMessage : ContentWithId
{
    public Guid ConversationId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
