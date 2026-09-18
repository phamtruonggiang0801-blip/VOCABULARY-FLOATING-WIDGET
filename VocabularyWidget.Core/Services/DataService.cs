using System.Text.Json;
using VocabularyWidget.Models;

namespace VocabularyWidget.Services;

public class DataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;
    private readonly string _settingsPath;

    public string FilePath => _filePath;

    public DataService(string? directory = null)
    {
        string appDir = directory ?? ResolveWritableDirectory();
        Directory.CreateDirectory(appDir);
        _filePath = Path.Combine(appDir, "words.json");
        _settingsPath = Path.Combine(appDir, "settings.json");
        EnsureDataFileExists();
    }

    public static List<WordItem> CreateSeedWords()
    {
        return new List<WordItem>
        {
            new() { Id = "1", Word = "resilient", Definition = "kiên cường, có khả năng phục hồi nhanh" },
            new() { Id = "2", Word = "ubiquitous", Definition = "có mặt ở khắp mọi nơi, phổ biến" },
            new() { Word = "diligent", Definition = "chăm chỉ, siêng năng" },
            new() { Word = "innovative", Definition = "đổi mới, sáng tạo" },
            new() { Word = "perspective", Definition = "góc nhìn, quan điểm" }
        };
    }

    public List<WordItem> LoadWords()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                var seed = CreateSeedWords();
                SaveWords(seed);
                return seed;
            }

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<WordItem>>(json, JsonOptions) ?? new List<WordItem>();
        }
        catch
        {
            return new List<WordItem>();
        }
    }

    public void SaveWords(List<WordItem> words)
    {
        string json = JsonSerializer.Serialize(words, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaults = new AppSettings();
                SaveSettings(defaults);
                return defaults;
            }

            string json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            settings.SetTimerMinutes(settings.TimerMinutes);
            settings.SetMode(settings.GetMode());
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private void EnsureDataFileExists()
    {
        if (!File.Exists(_filePath))
        {
            SaveWords(CreateSeedWords());
        }
    }

    private static string ResolveWritableDirectory()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        try
        {
            string probe = Path.Combine(baseDir, ".write-test");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return baseDir;
        }
        catch
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VocabularyWidget");
            Directory.CreateDirectory(appData);
            return appData;
        }
    }
}
