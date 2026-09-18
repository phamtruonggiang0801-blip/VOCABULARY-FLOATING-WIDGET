using System.Media;
using System.Text;

namespace VocabularyWidget.Forms;

/// <summary>Tiny in-memory WAV beeps. Quiet amplitude, no extra files.</summary>
internal static class FeedbackSounds
{
    private static SoundPlayer? _player;
    private static MemoryStream? _stream;

    public static void PlayCorrect()
    {
        PlayTone(880, 70);
    }

    public static void PlayWrong()
    {
        PlayTone(196, 90);
    }

    private static void PlayTone(int hz, int milliseconds)
    {
        try
        {
            byte[] wav = MakeSineWav(hz, milliseconds, amplitude: 0.12);
            _player?.Stop();
            _player?.Dispose();
            _stream?.Dispose();
            _stream = new MemoryStream(wav);
            _player = new SoundPlayer(_stream);
            _player.Load();
            _player.Play();
        }
        catch
        {
            // never fail the quiz because of audio
        }
    }

    private static byte[] MakeSineWav(int frequencyHz, int durationMs, double amplitude)
    {
        const int sampleRate = 16000;
        int samples = Math.Max(1, sampleRate * durationMs / 1000);
        int dataSize = samples * 2;
        using var ms = new MemoryStream(44 + dataSize);
        using var w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true);

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataSize);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(sampleRate);
        w.Write(sampleRate * 2);
        w.Write((short)2);
        w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataSize);

        for (int i = 0; i < samples; i++)
        {
            double t = i / (double)sampleRate;
            double fade = 1.0;
            int fadeSamples = Math.Min(400, samples / 4);
            if (i < fadeSamples)
            {
                fade = i / (double)fadeSamples;
            }
            else if (i > samples - fadeSamples)
            {
                fade = (samples - i) / (double)fadeSamples;
            }

            short sample = (short)(Math.Sin(2 * Math.PI * frequencyHz * t) * amplitude * fade * short.MaxValue);
            w.Write(sample);
        }

        w.Flush();
        return ms.ToArray();
    }
}
