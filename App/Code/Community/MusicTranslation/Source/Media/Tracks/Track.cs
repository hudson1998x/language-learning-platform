using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.MusicTranslation.Media.Tracks;

[Entity]
public class Track : ContentWithId
{
    public string Title { get; set; } = string.Empty;
    
    public Guid AlbumId { get; set; }
    
    public Guid ArtistId { get; set; }
    
    public string SongContents { get; set; } = "{}";
}