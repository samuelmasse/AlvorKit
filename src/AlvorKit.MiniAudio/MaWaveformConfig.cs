namespace AlvorKit.MiniAudio;

/// <summary>
/// Maps ma_waveform_config: 32 bytes, naturally aligned. Layout verified
/// against MSVC sizeof/offsetof for miniaudio 0.11.25.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MaWaveformConfig
{
    public MaFormat Format;
    public uint Channels;
    public uint SampleRate;
    public MaWaveformType Type;
    public double Amplitude;
    public double Frequency;
}
