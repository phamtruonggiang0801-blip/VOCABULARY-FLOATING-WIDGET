using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class AnswerCheckerTests
{
    [Theory]
    [InlineData("resilient", "resilient", true)]
    [InlineData("Resilient", "resilient", true)]
    [InlineData("  kiên cường  ", "kiên cường", true)]
    [InlineData("kien cuong", "kiên cường", false)]
    [InlineData("ubiquitous everywhere", "ubiquitous", false)]
    [InlineData("", "word", false)]
    public void Matches_collapsed_whitespace_and_case_but_keeps_diacritics(string input, string expected, bool ok)
    {
        Assert.Equal(ok, AnswerChecker.IsCorrect(input, expected));
    }

    [Fact]
    public void Collapses_internal_spaces()
    {
        Assert.True(AnswerChecker.IsCorrect("góc   nhìn,  quan điểm", "góc nhìn, quan điểm"));
    }

    [Theory]
    [InlineData("thương", "Yêu; thương; yêu quý", true)]
    [InlineData("Yêu", "Yêu; thương; yêu quý", true)]
    [InlineData("máy", "Máy tính", true)]
    [InlineData("爸", "爸爸 / 爸", true)]
    [InlineData("电脑", "Máy tính", false)]
    [InlineData("sai", "Yêu; thương; yêu quý", false)]
    [InlineData("", "Yêu; thương", false)]
    [InlineData("a", "ba, ca", false)]
    [InlineData("yêu quý", "Yêu; thương; yêu quý", true)]
    [InlineData("Máy tính", "Máy tính", true)]
    public void Write_mode_accepts_any_one_token(string input, string expected, bool ok)
    {
        Assert.Equal(ok, AnswerChecker.MatchesAnyToken(input, expected));
    }
}
