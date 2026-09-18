namespace VocabularyWidget.Models;

public class WordItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Word { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public int CorrectCount { get; set; }

    public int MissCount => Math.Max(0, ReviewCount - CorrectCount);
}
