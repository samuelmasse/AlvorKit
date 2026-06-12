using AlvorKit.MiniAudio;
using System.Runtime.InteropServices;

namespace AlvorKit.Demo;

/// <summary>Plays an endless Ode to Joy on a sine waveform through miniaudio.</summary>
public sealed class MelodyPlayer : IDisposable
{
    private static readonly double[] Notes = [330, 330, 349, 392, 392, 349, 330, 294, 262, 262, 294, 330, 330, 294, 294];
    private const int SampleRate = 48000;
    private const int Channels = 2;
    private const double Volume = 0.2;
    private const int StepSleepMs = 10;
    private const int ShortNoteMs = 350;
    private const int LongNoteMs = 700;

    private readonly Ma ma;
    private readonly nint engine;
    private readonly nint waveform;
    private readonly nint sound;
    private readonly Thread? thread;
    private readonly bool engineInitialized;
    private readonly bool waveformInitialized;
    private readonly bool soundInitialized;
    private readonly bool soundStarted;
    private volatile bool running;

    public MelodyPlayer(Ma ma)
    {
        this.ma = ma;
        engine = Marshal.AllocHGlobal((int)ma.SizeofMaEngine());
        waveform = Marshal.AllocHGlobal((int)ma.SizeofMaWaveform());
        sound = Marshal.AllocHGlobal((int)ma.SizeofMaSound());

        engineInitialized = ma.EngineInit(0, engine) == MaResult.Success;
        if (engineInitialized)
        {
            var config = ma.WaveformConfigInit(MaFormat.FormatF32, Channels, SampleRate, MaWaveformType.WaveformTypeSine, Volume, Notes[0]);
            waveformInitialized = ma.WaveformInit(in config, waveform) == MaResult.Success;
            soundInitialized = waveformInitialized && ma.SoundInitFromDataSource(engine, waveform, 0, 0, sound) == MaResult.Success;
            soundStarted = soundInitialized && ma.SoundStart(sound) == MaResult.Success;
        }

        Playing = soundStarted;
        if (soundStarted)
        {
            running = true;
            thread = new Thread(Play) { IsBackground = true };
            thread.Start();
        }
    }

    public bool Playing { get; }

    public void Dispose()
    {
        if (soundStarted)
        {
            running = false;
            thread?.Join();
            ma.SoundStop(sound);
        }

        if (soundInitialized)
            ma.SoundUninit(sound);

        if (waveformInitialized)
            ma.WaveformUninit(waveform);

        if (engineInitialized)
            ma.EngineUninit(engine);

        Marshal.FreeHGlobal(sound);
        Marshal.FreeHGlobal(waveform);
        Marshal.FreeHGlobal(engine);
    }

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
}
