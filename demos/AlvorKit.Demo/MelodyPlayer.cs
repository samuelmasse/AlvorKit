using AlvorKit.MiniAudio;

namespace AlvorKit.Demo;

/// <summary>Plays an endless Ode to Joy on a sine waveform through miniaudio.</summary>
public class MelodyPlayer : IDisposable
{
    private static readonly double[] Notes = [330, 330, 349, 392, 392, 349, 330, 294, 262, 262, 294, 330, 330, 294, 294];

    private readonly nint engine = Marshal.AllocHGlobal((int)Ma.SizeofEngine());
    private readonly nint waveform = Marshal.AllocHGlobal((int)Ma.SizeofWaveform());
    private readonly nint sound = Marshal.AllocHGlobal((int)Ma.SizeofSound());
    private readonly Thread? thread;
    private volatile bool running;

    public bool Playing { get; }

    public MelodyPlayer()
    {
        Playing = Ma.EngineInit(0, engine) == MaResult.Success;
        if (Playing)
        {
            var config = Ma.WaveformConfigInit(MaFormat.F32, 2, 48000, MaWaveformType.Sine, 0.2, Notes[0]);
            Playing = Ma.WaveformInit(in config, waveform) == MaResult.Success
                && Ma.SoundInitFromDataSource(engine, waveform, 0, 0, sound) == MaResult.Success
                && Ma.SoundStart(sound) == MaResult.Success;
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
            Ma.SoundStop(sound);
            Ma.SoundUninit(sound);
            Ma.WaveformUninit(waveform);
            Ma.EngineUninit(engine);
        }

        Marshal.FreeHGlobal(sound);
        Marshal.FreeHGlobal(waveform);
        Marshal.FreeHGlobal(engine);
    }

    private void Play()
    {
        for (var i = 0; running; i = (i + 1) % Notes.Length)
        {
            Ma.WaveformSetFrequency(waveform, Notes[i]);
            for (var slept = 0; running && slept < (i == Notes.Length - 1 ? 700 : 350); slept += 10)
                Thread.Sleep(10);
        }
    }
}
