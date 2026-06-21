using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.FlashCards.FlashCards;

[Entity]
public class FlashCard : ContentWithId
{
    public Guid UserId { get; set; }

    public Guid LanguageId { get; set; }

    public string FrontStatement { get; set; } = string.Empty;

    public string BackStatement { get; set; } = string.Empty;

    public string? Pronunciation { get; set; }

    public string? Notes { get; set; }

    public string? Category { get; set; }

    public string? Tags { get; set; }

    public int Difficulty { get; set; } = 1;

    public DateTime? LastReviewedUtc { get; set; }

    public int ReviewCount { get; set; }

    public int CorrectCount { get; set; }

    public int IncorrectCount { get; set; }

    public int Streak { get; set; }
}