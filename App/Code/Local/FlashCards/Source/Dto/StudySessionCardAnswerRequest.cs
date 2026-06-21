namespace LLE.FlashCards.Dto;

public class StudySessionCardAnswerRequest
{
    public string CardId { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
}