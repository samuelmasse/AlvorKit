namespace AlvorKit.MiniAudio;

/// <summary>
/// The miniaudio API surface. ma_engine/ma_waveform/ma_sound are caller-allocated
/// opaque blocks — allocate them with the Sizeof* methods. Every method throws
/// NotImplementedException until a backend overrides it (e.g. MaBackend from
/// AlvorKit.MiniAudio.Backend).
/// </summary>
public class Ma
{
    public virtual nuint SizeofEngine() => throw new NotImplementedException();

    public virtual nuint SizeofWaveform() => throw new NotImplementedException();

    public virtual nuint SizeofSound() => throw new NotImplementedException();

    public virtual MaResult EngineInit(nint config, nint engine) => throw new NotImplementedException();

    public virtual void EngineUninit(nint engine) => throw new NotImplementedException();

    public virtual MaWaveformConfig WaveformConfigInit(MaFormat format, uint channels, uint sampleRate, MaWaveformType type, double amplitude, double frequency) => throw new NotImplementedException();

    public virtual MaResult WaveformInit(in MaWaveformConfig config, nint waveform) => throw new NotImplementedException();

    public virtual void WaveformUninit(nint waveform) => throw new NotImplementedException();

    public virtual MaResult WaveformSetFrequency(nint waveform, double frequency) => throw new NotImplementedException();

    public virtual MaResult SoundInitFromDataSource(nint engine, nint dataSource, uint flags, nint group, nint sound) => throw new NotImplementedException();

    public virtual void SoundUninit(nint sound) => throw new NotImplementedException();

    public virtual MaResult SoundStart(nint sound) => throw new NotImplementedException();

    public virtual MaResult SoundStop(nint sound) => throw new NotImplementedException();
}
