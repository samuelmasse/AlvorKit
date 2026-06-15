using AlvorKit.MiniAudio;

namespace AlvorKit.Demo;

/// <summary>Plays an endless Ode to Joy on a sine waveform through miniaudio.</summary>
public sealed unsafe class MelodyPlayer : IDisposable
{
    /// <summary>The melody frequencies cycled by the background thread.</summary>
    private static readonly double[] Notes = [330, 330, 349, 392, 392, 349, 330, 294, 262, 262, 294, 330, 330, 294, 294];

    /// <summary>The output sample rate requested from miniaudio.</summary>
    private const int SampleRate = 48000;

    /// <summary>The stereo channel count requested from miniaudio.</summary>
    private const int Channels = 2;

    /// <summary>The sine wave volume used for the demo melody.</summary>
    private const double Volume = 0.2;

    /// <summary>The sleep slice used so disposal can stop playback promptly.</summary>
    private const int StepSleepMs = 10;

    /// <summary>The duration of each regular melody note.</summary>
    private const int ShortNoteMs = 350;

    /// <summary>The held duration of the final note in the melody phrase.</summary>
    private const int LongNoteMs = 700;

    /// <summary>The miniaudio API used for engine, waveform, and sound operations.</summary>
    private readonly Ma ma;

    /// <summary>The native miniaudio engine object memory owned by this player.</summary>
    private readonly MaEngine* engine;

    /// <summary>The native waveform data-source object memory owned by this player.</summary>
    private readonly MaWaveform* waveform;

    /// <summary>The native miniaudio sound object memory owned by this player.</summary>
    private readonly MaSound* sound;

    /// <summary>The background thread that advances the melody frequency.</summary>
    private readonly Thread thread;

    /// <summary>Signals the playback thread to continue cycling notes.</summary>
    private volatile bool running;

    /// <summary>Starts the miniaudio engine, waveform, sound, and playback thread.</summary>
    /// <param name="ma">The miniaudio API used for playback.</param>
    public MelodyPlayer(Ma ma)
    {
        this.ma = ma;

        // Miniaudio's C objects are transparent structs, but the demo treats stateful
        // objects as stable native storage and never reads their backend-specific fields.
        engine = AllocateNativeObject<MaEngine>();
        waveform = AllocateNativeObject<MaWaveform>();
        sound = AllocateNativeObject<MaSound>();

        ma.EngineInit(null, engine);
        var config = ma.WaveformConfigInit(MaFormat.FormatF32, Channels, SampleRate, MaWaveformType.WaveformTypeSine, Volume, Notes[0]);
        ma.WaveformInit(in config, waveform);
        ma.SoundInitFromDataSource(engine, (nint)waveform, 0, null, sound);
        ma.SoundStart(sound);

        running = true;
        thread = new Thread(Play) { IsBackground = true };
        thread.Start();
    }

    /// <summary>Stops playback and releases miniaudio/native-memory resources owned by the player.</summary>
    public void Dispose()
    {
        running = false;
        thread.Join();
        ma.SoundStop(sound);
        ma.SoundUninit(sound);
        ma.WaveformUninit(waveform);
        ma.EngineUninit(engine);

        NativeMemory.Free(sound);
        NativeMemory.Free(waveform);
        NativeMemory.Free(engine);
    }

    /// <summary>Cycles melody frequencies until disposal asks the thread to stop.</summary>
    private void Play()
    {
        for (var i = 0; running; i = (i + 1) % Notes.Length)
        {
            ma.WaveformSetFrequency(waveform, Notes[i]);
            var duration = i == Notes.Length - 1 ? LongNoteMs : ShortNoteMs;

            for (var slept = 0; running && slept < duration; slept += StepSleepMs)
                Thread.Sleep(StepSleepMs);
        }
    }

    /// <summary>Allocates a zero-filled native object using the generated struct size.</summary>
    private static T* AllocateNativeObject<T>()
        where T : unmanaged =>
        (T*)NativeMemory.AllocZeroed((nuint)sizeof(T));
}
