using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.LeMessage.Conversations;

[Entity]
public class Conversation : ContentWithId
{
    public Guid ProfileId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;
}
