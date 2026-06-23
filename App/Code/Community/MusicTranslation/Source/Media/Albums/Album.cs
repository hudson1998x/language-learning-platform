using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.MusicTranslation.Media.Albums;

[Entity]
public class Album : ContentWithId
{
    public string Title { get; set; } = string.Empty;
    
    [Unique]
    public string AlbumKey { get; set; } = string.Empty;
    
    public string AlbumCoverImageUrl { get; set; } = string.Empty;
}