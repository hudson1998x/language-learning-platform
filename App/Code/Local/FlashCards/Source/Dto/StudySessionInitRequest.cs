namespace LLE.FlashCards.Dto;

public class StudySessionInitRequest
{
    public int CardCount { get; set; }
    public List<string>? Categories { get; set; }
}