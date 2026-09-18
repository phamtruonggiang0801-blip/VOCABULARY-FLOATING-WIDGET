using VocabularyWidget.Services;
using Xunit;

namespace VocabularyWidget.Tests;

public class WordImportServiceTests
{
    [Fact]
    public void Parses_csv_with_header_and_quotes()
    {
        const string csv = """
            word,definition
            resilient,"kiên cường, có khả năng phục hồi nhanh"
            ubiquitous,có mặt ở khắp mọi nơi
            """;
        var items = WordImportService.Parse(csv, "words.csv");
        Assert.Equal(2, items.Count);
        Assert.Equal("resilient", items[0].Word);
        Assert.Contains("kiên cường", items[0].Definition);
    }

    [Fact]
    public void Parses_pipe_and_dash_txt()
    {
        const string txt = """
            diligent | chăm chỉ, siêng năng
            innovative - đổi mới, sáng tạo
            perspective	góc nhìn, quan điểm
            """;
        var items = WordImportService.Parse(txt, "words.txt");
        Assert.Equal(3, items.Count);
        Assert.Equal("diligent", items[0].Word);
        Assert.Equal("innovative", items[1].Word);
        Assert.Equal("perspective", items[2].Word);
    }

    [Fact]
    public void Skips_incomplete_lines()
    {
        var items = WordImportService.Parse("onlyword\n , missingword\nok | nghĩa");
        Assert.Single(items);
        Assert.Equal("ok", items[0].Word);
    }
}
