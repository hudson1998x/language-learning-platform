using LLE.MusicTranslation.Media.Albums;
using LLE.MusicTranslation.Media.Artists;
using LLE.MusicTranslation.Media.Tracks;

namespace LLE.MusicTranslation.Dto;

public class SongTranslationResponse
{
    public Track Song { get; set; }
    
    public Artist Artist { get; set; }
    
    public Album Album { get; set; }
}