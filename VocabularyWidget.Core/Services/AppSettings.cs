namespace VocabularyWidget.Services;

public class AppSettings
{
    public int TimerMinutes { get; set; } = 5;

    public int TimerMilliseconds => Math.Max(1, TimerMinutes) * 60 * 1000;

    public static readonly int[] AllowedMinutes = { 1, 3, 5, 10 };

    public void SetTimerMinutes(int minutes)
    {
        TimerMinutes = AllowedMinutes.Contains(minutes) ? minutes : 5;
    }
}
