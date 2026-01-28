using AVARace.Services.Interfaces;
using NetCoreAudio;

namespace AVARace.Services;

public class SoundService : ISoundService, IDisposable
{
    private readonly string _soundDir;
    private Player? _shootPlayer;
    private Player? _explosionPlayer;
    private Player? _thrusterPlayer;
    private bool _isThrusterPlaying;
    private bool _isDisposed;

    public SoundService()
    {
        _soundDir = Path.Combine(Path.GetTempPath(), "AVARace_Sounds");
    }

    public void Initialize()
    {
        Directory.CreateDirectory(_soundDir);

        GenerateShootSound();
        GenerateExplosionSound();
        GenerateThrusterSound();

        _shootPlayer = new Player();
        _explosionPlayer = new Player();
        _thrusterPlayer = new Player();
    }

    public void PlayShoot()
    {
        if (_isDisposed) return;
        Task.Run(async () =>
        {
            try
            {
                var player = new Player();
                await player.Play(Path.Combine(_soundDir, "shoot.wav"));
            }
            catch { }
        });
    }

    public void PlayExplosion()
    {
        if (_isDisposed) return;
        Task.Run(async () =>
        {
            try
            {
                var player = new Player();
                await player.Play(Path.Combine(_soundDir, "explosion.wav"));
            }
            catch { }
        });
    }

    public void PlayThruster(bool isThrusting)
    {
        if (_isDisposed) return;

        if (isThrusting && !_isThrusterPlaying)
        {
            _isThrusterPlaying = true;
            Task.Run(async () =>
            {
                try
                {
                    if (_thrusterPlayer != null)
                    {
                        await _thrusterPlayer.Play(Path.Combine(_soundDir, "thruster.wav"));
                    }
                }
                catch { }
            });
        }
        else if (!isThrusting && _isThrusterPlaying)
        {
            _isThrusterPlaying = false;
            Task.Run(async () =>
            {
                try
                {
                    if (_thrusterPlayer != null)
                    {
                        await _thrusterPlayer.Stop();
                    }
                }
                catch { }
            });
        }
    }

    private void GenerateShootSound()
    {
        var sampleRate = 22050;
        var duration = 0.1;
        var samples = (int)(sampleRate * duration);
        var data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            var t = (double)i / sampleRate;
            var frequency = 880 - (t / duration) * 600;
            var amplitude = 1.0 - (t / duration);
            data[i] = (short)(Math.Sin(2 * Math.PI * frequency * t) * amplitude * 20000);
        }

        SaveWav(Path.Combine(_soundDir, "shoot.wav"), data, sampleRate);
    }

    private void GenerateExplosionSound()
    {
        var sampleRate = 22050;
        var duration = 0.7;
        var samples = (int)(sampleRate * duration);
        var data = new short[samples];
        var random = new Random(42);

        for (int i = 0; i < samples; i++)
        {
            var t = (double)i / sampleRate;

            // Slower decay for longer rumble
            var amplitude = Math.Pow(1.0 - (t / duration), 1.5);

            // Filtered noise (less high frequency)
            var noise = (random.NextDouble() * 2 - 1);

            // Multiple low frequency rumble layers
            var bassRumble1 = Math.Sin(2 * Math.PI * 35 * t) * 0.5;
            var bassRumble2 = Math.Sin(2 * Math.PI * 50 * t) * 0.4;
            var bassRumble3 = Math.Sin(2 * Math.PI * 70 * t) * 0.3;

            // Descending pitch component for impact feel
            var impactFreq = 150 - (t / duration) * 100;
            var impact = Math.Sin(2 * Math.PI * impactFreq * t) * 0.3 * Math.Exp(-t * 8);

            // Combine: heavy bass + filtered noise + impact
            var sample = (bassRumble1 + bassRumble2 + bassRumble3) * 0.6
                       + noise * 0.25
                       + impact;

            data[i] = (short)(sample * amplitude * 30000);
        }

        SaveWav(Path.Combine(_soundDir, "explosion.wav"), data, sampleRate);
    }

    private void GenerateThrusterSound()
    {
        var sampleRate = 22050;
        var duration = 0.3;
        var samples = (int)(sampleRate * duration);
        var data = new short[samples];
        var random = new Random(123);

        for (int i = 0; i < samples; i++)
        {
            var t = (double)i / sampleRate;

            var noise = random.NextDouble() * 2 - 1;
            var lowRumble = Math.Sin(2 * Math.PI * 80 * t) * 0.5;
            var midRumble = Math.Sin(2 * Math.PI * 120 * t) * 0.3;

            var fadeIn = Math.Min(1.0, t * 20);
            var fadeOut = Math.Min(1.0, (duration - t) * 20);
            var envelope = fadeIn * fadeOut;

            data[i] = (short)((noise * 0.4 + lowRumble + midRumble) * envelope * 15000);
        }

        SaveWav(Path.Combine(_soundDir, "thruster.wav"), data, sampleRate);
    }

    private static void SaveWav(string path, short[] data, int sampleRate)
    {
        using var stream = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        var dataSize = data.Length * 2;
        var fileSize = 36 + dataSize;

        writer.Write("RIFF"u8);
        writer.Write(fileSize);
        writer.Write("WAVE"u8);

        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);

        writer.Write("data"u8);
        writer.Write(dataSize);

        foreach (var sample in data)
        {
            writer.Write(sample);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        try
        {
            _thrusterPlayer?.Stop();
            if (Directory.Exists(_soundDir))
            {
                Directory.Delete(_soundDir, true);
            }
        }
        catch { }
    }
}
