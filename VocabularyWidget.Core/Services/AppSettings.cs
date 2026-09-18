namespace VocabularyWidget.Services;

public class AppSettings
{
    public int TimerMinutes { get; set; } = 5;

    /// <summary>When true, speak the Chinese word each time a card is shown. Default off.</summary>
    public bool AutoSpeakHanzi { get; set; }

    /// <summary>When true, play a short quiet beep on correct/wrong. Default off.</summary>
    public bool FeedbackSounds { get; set; }

    public int TimerMilliseconds => Math.Max(1, TimerMinutes) * 60 * 1000;

    public static readonly int[] AllowedMinutes = { 1, 3, 5, 10 };

    public void SetTimerMinutes(int minutes)
    {
        TimerMinutes = AllowedMinutes.Contains(minutes) ? minutes : 5;
    }
}
