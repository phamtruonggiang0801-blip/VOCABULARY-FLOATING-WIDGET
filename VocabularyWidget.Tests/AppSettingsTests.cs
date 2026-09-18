using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Default_mode_is_quiz()
    {
        Assert.Equal(WidgetMode.Quiz, new AppSettings().GetMode());
    }

    [Theory]
    [InlineData("listen", WidgetMode.Listen)]
    [InlineData("Work", WidgetMode.Work)]
    [InlineData("WRITE", WidgetMode.Write)]
    [InlineData("nope", WidgetMode.Quiz)]
    [InlineData("", WidgetMode.Quiz)]
    public void Parses_mode_or_falls_back_to_quiz(string raw, WidgetMode expected)
    {
        var settings = new AppSettings { Mode = raw };
        Assert.Equal(expected, settings.GetMode());
    }

    [Fact]
    public void SetMode_writes_enum_name()
    {
        var settings = new AppSettings();
        settings.SetMode(WidgetMode.Work);
        Assert.Equal("Work", settings.Mode);
        Assert.Equal(WidgetMode.Work, settings.GetMode());
    }
}
