using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.MusicTranslation.Media.Artists;

[Entity]
public class Artist : ContentWithId
{
    [Unique]
    public string Name { get; set; } = string.Empty;
    
    public string CoverImageUrl  { get; set; } = string.Empty;
}