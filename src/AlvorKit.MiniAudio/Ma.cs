namespace AlvorKit.MiniAudio;

/// <summary>
/// Raw bindings over the miniaudio shared library (AlvorKit.MiniAudio.Native).
/// ma_engine/ma_waveform/ma_sound are caller-allocated opaque blocks — allocate
/// them with the Sizeof* helpers exported by the native build.
/// </summary>
public static partial class Ma
{
    private const string Lib = "miniaudio";

    [LibraryImport(Lib, EntryPoint = "alvorkit_sizeof_ma_engine")]
    public static partial nuint SizeofEngine();

    [LibraryImport(Lib, EntryPoint = "alvorkit_sizeof_ma_waveform")]
    public static partial nuint SizeofWaveform();

    [LibraryImport(Lib, EntryPoint = "alvorkit_sizeof_ma_sound")]
    public static partial nuint SizeofSound();

    [LibraryImport(Lib, EntryPoint = "ma_engine_init")]
    public static partial MaResult EngineInit(nint config, nint engine);

    [LibraryImport(Lib, EntryPoint = "ma_engine_uninit")]
    public static partial void EngineUninit(nint engine);

    [LibraryImport(Lib, EntryPoint = "ma_waveform_config_init")]
    public static partial MaWaveformConfig WaveformConfigInit(MaFormat format, uint channels, uint sampleRate, MaWaveformType type, double amplitude, double frequency);

    [LibraryImport(Lib, EntryPoint = "ma_waveform_init")]
    public static partial MaResult WaveformInit(in MaWaveformConfig config, nint waveform);

    [LibraryImport(Lib, EntryPoint = "ma_waveform_uninit")]
    public static partial void WaveformUninit(nint waveform);

    [LibraryImport(Lib, EntryPoint = "ma_waveform_set_frequency")]
    public static partial MaResult WaveformSetFrequency(nint waveform, double frequency);

    [LibraryImport(Lib, EntryPoint = "ma_sound_init_from_data_source")]
    public static partial MaResult SoundInitFromDataSource(nint engine, nint dataSource, uint flags, nint group, nint sound);

    [LibraryImport(Lib, EntryPoint = "ma_sound_uninit")]
    public static partial void SoundUninit(nint sound);

    [LibraryImport(Lib, EntryPoint = "ma_sound_start")]
    public static partial MaResult SoundStart(nint sound);

    [LibraryImport(Lib, EntryPoint = "ma_sound_stop")]
    public static partial MaResult SoundStop(nint sound);
}
