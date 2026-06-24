using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.LeMessage.Profiles;

[Entity]
public class Profile : ContentWithId
{
    [Unique]
    public string Name { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SystemPrompt { get; set; } = string.Empty;

    public string LanguageName { get; set; } = string.Empty;
}
