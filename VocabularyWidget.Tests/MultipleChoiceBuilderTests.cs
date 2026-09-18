using VocabularyWidget.Models;
using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class MultipleChoiceBuilderTests
{
    private static List<WordItem> Sample() => new()
    {
        new() { Id = "1", Word = "爱", Definition = "Yêu; thương" },
        new() { Id = "2", Word = "八", Definition = "Số tám" },
        new() { Id = "3", Word = "白", Definition = "Trắng" },
        new() { Id = "4", Word = "北", Definition = "Phía bắc" },
        new() { Id = "5", Word = "大", Definition = "Lớn; to" }
    };

    [Fact]
    public void Includes_correct_definition_among_four_options()
    {
        var words = Sample();
        var q = MultipleChoiceBuilder.Build(words, words[0], showingWord: true, new Random(1));
        Assert.Equal("爱", q.Prompt);
        Assert.Equal("Yêu; thương", q.CorrectAnswer);
        Assert.Equal(4, q.Options.Count);
        Assert.Contains(q.CorrectAnswer, q.Options);
        Assert.True(q.IsCorrect(q.CorrectAnswer));
        Assert.False(q.IsCorrect("nope"));
        Assert.Equal(q.Options.Distinct().Count(), q.Options.Count);
    }

    [Fact]
    public void When_showing_definition_options_are_words()
    {
        var words = Sample();
        var q = MultipleChoiceBuilder.Build(words, words[1], showingWord: false, new Random(2));
        Assert.Equal("Số tám", q.Prompt);
        Assert.Equal("八", q.CorrectAnswer);
        Assert.All(q.Options, o => Assert.Contains(words, w => w.Word == o));
    }

    [Fact]
    public void Works_with_fewer_than_four_cards()
    {
        var words = Sample().Take(2).ToList();
        var q = MultipleChoiceBuilder.Build(words, words[0], showingWord: true, new Random(3));
        Assert.Equal(2, q.Options.Count);
        Assert.Contains("Yêu; thương", q.Options);
        Assert.Contains("Số tám", q.Options);
    }
}
