using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class DataServiceTests
{
    [Fact]
    public void Seeds_and_round_trips_words()
    {
        string dir = Path.Combine(Path.GetTempPath(), "vw-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var service = new DataService(dir);
            var loaded = service.LoadWords();
            Assert.True(loaded.Count >= 5);
            Assert.Contains(loaded, w => w.Word == "resilient");

            loaded[0].ReviewCount = 3;
            loaded[0].CorrectCount = 1;
            service.SaveWords(loaded);

            var again = new DataService(dir).LoadWords();
            var resilient = again.First(w => w.Word == "resilient");
            Assert.Equal(3, resilient.ReviewCount);
            Assert.Equal(1, resilient.CorrectCount);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Persists_timer_minutes_audio_flags_and_mode()
    {
        string dir = Path.Combine(Path.GetTempPath(), "vw-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var service = new DataService(dir);
            var settings = service.LoadSettings();
            Assert.Equal(5, settings.TimerMinutes);
            Assert.False(settings.AutoSpeakHanzi);
            Assert.False(settings.FeedbackSounds);
            settings.SetTimerMinutes(3);
            settings.AutoSpeakHanzi = true;
            settings.FeedbackSounds = true;
            settings.SetMode(WidgetMode.Listen);
            service.SaveSettings(settings);
            var again = new DataService(dir).LoadSettings();
            Assert.Equal(3, again.TimerMinutes);
            Assert.True(again.AutoSpeakHanzi);
            Assert.True(again.FeedbackSounds);
            Assert.Equal(WidgetMode.Listen, again.GetMode());
            settings.SetTimerMinutes(99);
            Assert.Equal(5, settings.TimerMinutes);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
