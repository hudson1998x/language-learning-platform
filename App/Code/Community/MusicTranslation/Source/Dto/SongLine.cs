namespace LLE.MusicTranslation.Dto;

public class SongLine
{
    public string LineContents { get; set; } = string.Empty;
    
    public string TranslationToUserLanguage { get; set; } = string.Empty;
    
    public string[] Pronunciations { get; set; } = [];
    
    public string CulturalMeaning { get; set; } = string.Empty;
}