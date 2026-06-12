using AlvorKit.MiniAudio;

namespace AlvorKit.Demo;

/// <summary>Plays an endless Ode to Joy on a sine waveform through miniaudio.</summary>
public class MelodyPlayer : IDisposable
{
    private static readonly double[] Notes = [330, 330, 349, 392, 392, 349, 330, 294, 262, 262, 294, 330, 330, 294, 294];

    private readonly Ma ma;
    private readonly nint engine;
    private readonly nint waveform;
    private readonly nint sound;
    private readonly Thread? thread;
    private volatile bool running;

    public bool Playing { get; }

    public MelodyPlayer(Ma ma)
    {
        this.ma = ma;
        engine = Marshal.AllocHGlobal((int)ma.SizeofEngine());
        waveform = Marshal.AllocHGlobal((int)ma.SizeofWaveform());
        sound = Marshal.AllocHGlobal((int)ma.SizeofSound());

        Playing = ma.EngineInit(0, engine) == MaResult.Success;
        if (Playing)
        {
            var config = ma.WaveformConfigInit(MaFormat.F32, 2, 48000, MaWaveformType.Sine, 0.2, Notes[0]);
            Playing = ma.WaveformInit(in config, waveform) == MaResult.Success
                && ma.SoundInitFromDataSource(engine, waveform, 0, 0, sound) == MaResult.Success
                && ma.SoundStart(sound) == MaResult.Success;
        }

        if (Playing)
        {
            running = true;
            thread = new(Play) { IsBackground = true };
            thread.Start();
        }
    }

    public void Dispose()
    {
        if (Playing)
        {
            running = false;
            thread?.Join();
            ma.SoundStop(sound);
            ma.SoundUninit(sound);
            ma.WaveformUninit(waveform);
            ma.EngineUninit(engine);
        }

        Marshal.FreeHGlobal(sound);
        Marshal.FreeHGlobal(waveform);
        Marshal.FreeHGlobal(engine);
    }

    private void Play()
    {
        for (var i = 0; running; i = (i + 1) % Notes.Length)
        {
            ma.WaveformSetFrequency(waveform, Notes[i]);
            for (var slept = 0; running && slept < (i == Notes.Length - 1 ? 700 : 350); slept += 10)
                Thread.Sleep(10);
        }
    }
}
