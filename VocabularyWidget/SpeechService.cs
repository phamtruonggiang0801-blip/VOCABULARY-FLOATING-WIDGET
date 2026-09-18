using System.Globalization;
using System.Speech.Synthesis;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms;

/// <summary>
/// Offline Mandarin TTS via Windows SAPI. No network, no extra files.
/// Needs a Chinese speech voice installed on the PC (common on zh-CN Windows).
/// </summary>
internal sealed class SpeechService : IDisposable
{
    private readonly SpeechSynthesizer? _synth;

    public bool HasChineseVoice { get; }
    public string? VoiceName { get; }

    public SpeechService()
    {
        try
        {
            _synth = new SpeechSynthesizer
            {
                Rate = -2,
                Volume = 80
            };
            HasChineseVoice = TrySelectChineseVoice(_synth, out var name);
            VoiceName = name;
        }
        catch
        {
            _synth = null;
            HasChineseVoice = false;
        }
    }

    public void SpeakHanzi(string? word)
    {
        if (_synth == null)
        {
            return;
        }

        string text = PronunciationText.ForSpeech(word ?? "");
        if (text.Length == 0)
        {
            return;
        }

        try
        {
            _synth.SpeakAsyncCancelAll();
            var prompt = new PromptBuilder(new CultureInfo("zh-CN"));
            prompt.AppendText(text);
            _synth.SpeakAsync(prompt);
        }
        catch
        {
            // ignore TTS failures so the widget never crashes
        }
    }

    public void Dispose()
    {
        try
        {
            _synth?.SpeakAsyncCancelAll();
            _synth?.Dispose();
        }
        catch
        {
            // ignore
        }
    }

    private static bool TrySelectChineseVoice(SpeechSynthesizer synth, out string? name)
    {
        name = null;
        try
        {
            var voices = synth.GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => v.VoiceInfo)
                .ToList();

            VoiceInfo? zh = voices.FirstOrDefault(v =>
                v.Culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase));
            zh ??= voices.FirstOrDefault(v =>
                v.Name.Contains("Chinese", StringComparison.OrdinalIgnoreCase)
                || v.Name.Contains("Huihui", StringComparison.OrdinalIgnoreCase)
                || v.Name.Contains("Xiaoxiao", StringComparison.OrdinalIgnoreCase)
                || v.Name.Contains("Yaoyao", StringComparison.OrdinalIgnoreCase)
                || v.Name.Contains("Kangkang", StringComparison.OrdinalIgnoreCase)
                || v.Name.Contains("中文", StringComparison.Ordinal));

            if (zh != null)
            {
                synth.SelectVoice(zh.Name);
                name = zh.Name;
                return true;
            }
        }
        catch
        {
            // keep default voice
        }

        return false;
    }
}
