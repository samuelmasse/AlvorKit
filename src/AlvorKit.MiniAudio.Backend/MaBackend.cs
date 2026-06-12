namespace AlvorKit.MiniAudio;

/// <summary>Implements <see cref="Ma"/> against the miniaudio shared library.</summary>
public class MaBackend : Ma
{
    public override nuint SizeofEngine() => MaNative.SizeofEngine();

    public override nuint SizeofWaveform() => MaNative.SizeofWaveform();

    public override nuint SizeofSound() => MaNative.SizeofSound();

    public override MaResult EngineInit(nint config, nint engine) => MaNative.EngineInit(config, engine);

    public override void EngineUninit(nint engine) => MaNative.EngineUninit(engine);

    public override MaWaveformConfig WaveformConfigInit(MaFormat format, uint channels, uint sampleRate, MaWaveformType type, double amplitude, double frequency) => MaNative.WaveformConfigInit(format, channels, sampleRate, type, amplitude, frequency);

    public override MaResult WaveformInit(in MaWaveformConfig config, nint waveform) => MaNative.WaveformInit(in config, waveform);

    public override void WaveformUninit(nint waveform) => MaNative.WaveformUninit(waveform);

    public override MaResult WaveformSetFrequency(nint waveform, double frequency) => MaNative.WaveformSetFrequency(waveform, frequency);

    public override MaResult SoundInitFromDataSource(nint engine, nint dataSource, uint flags, nint group, nint sound) => MaNative.SoundInitFromDataSource(engine, dataSource, flags, group, sound);

    public override void SoundUninit(nint sound) => MaNative.SoundUninit(sound);

    public override MaResult SoundStart(nint sound) => MaNative.SoundStart(sound);

    public override MaResult SoundStop(nint sound) => MaNative.SoundStop(sound);
}
