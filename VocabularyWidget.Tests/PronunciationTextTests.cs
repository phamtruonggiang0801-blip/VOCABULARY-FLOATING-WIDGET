using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class PronunciationTextTests
{
    [Theory]
    [InlineData("爱", "爱")]
    [InlineData("爸爸 / 爸", "爸爸")]
    [InlineData("有时候 | 有时", "有时候")]
    [InlineData("要（动）", "要")]
    [InlineData("们（朋友们）", "们")]
    [InlineData("  电脑  ", "电脑")]
    [InlineData("", "")]
    public void Strips_variants_and_notes(string input, string expected)
    {
        Assert.Equal(expected, PronunciationText.ForSpeech(input));
    }
}
