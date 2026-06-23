namespace LLE.MusicTranslation.Dto;

public class SongTranslationRequest
{
    public string Lyrics { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Artist { get; set; } = string.Empty;
    
    public string Album { get; set; } = string.Empty;
}