CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

const uint SampleRate = 48000;
const uint Channels = 2;
const int PreviewFrameCount = 1024;
const int OfflineSeconds = 2;
const int Pcm16WavHeaderByteCount = 44;
const int Pcm16BytesPerSample = 2;

var outputRoot = Path.GetFullPath(Path.Combine("out", "miniaudio-demo"));
Directory.CreateDirectory(outputRoot);

Ma ma = new MaBackend();

Console.WriteLine("AlvorKit.MiniAudio.Demo - generated miniaudio.h binding tour");
Console.WriteLine($"Output: {outputRoot}");

// Start with simple metadata calls so the generated binding shape is visible before any native lifetime work begins.
ReportRuntime(ma);

unsafe
{
    // Generate a tiny WAV in memory, then decode it through miniaudio's memory decoder APIs.
    var generatedWav = CreateSineWaveWav(SampleRate, Channels, frequency: 330, amplitude: 0.35, seconds: 1);
    DecodeMemoryWav(ma, generatedWav, PreviewFrameCount);

    // Pull samples from procedural data sources and run one of them through miniaudio's direct DSP API.
    ReadProceduralSources(ma, SampleRate, Channels, PreviewFrameCount);

    // Build a no-device engine graph and render it offline so the demo works without opening an audio device.
    var offlineMix = RenderOfflineEngine(ma, SampleRate, Channels, OfflineSeconds);
    var offlineMixPath = Path.Combine(outputRoot, "offline-engine-mix.wav");
    WritePcm16Wav(offlineMixPath, offlineMix, SampleRate, Channels);
    Console.WriteLine($"Wrote offline engine mix: {offlineMixPath}");
}

return 0;

// Prints miniaudio version and representative backend metadata.
static void ReportRuntime(Ma ma)
{
    Section("Runtime metadata");

    ma.VersionString(out var version);
    Print("ma_version_string / VersionString", version ?? "<null>");
    Print("ma_get_backend_name / WASAPI", BackendName(ma, MaBackendKind.BackendWasapi));
    Print("ma_get_backend_name / CoreAudio", BackendName(ma, MaBackendKind.BackendCoreaudio));
    Print("ma_get_backend_name / ALSA", BackendName(ma, MaBackendKind.BackendAlsa));
    Print("ma_get_format_name / f32", FormatName(ma, MaFormat.FormatF32));
}

// Creates a small WAV file in memory so decoder examples do not need checked-in audio assets.
static byte[] CreateSineWaveWav(uint sampleRate, uint channels, double frequency, double amplitude, int seconds)
{
    var frameCount = checked((int)sampleRate * seconds);
    var dataSize = checked(frameCount * (int)channels * Pcm16BytesPerSample);
    var wav = new byte[Pcm16WavHeaderByteCount + dataSize];

    WritePcm16WavHeader(wav, dataSize, sampleRate, channels);
    WriteSinePcm16(wav.AsSpan(Pcm16WavHeaderByteCount), sampleRate, channels, frequency, amplitude);
    return wav;
}

// Decodes generated WAV bytes from memory and reports the converted PCM shape.
static unsafe void DecodeMemoryWav(Ma ma, byte[] wavBytes, int previewFrameCount)
{
    Section("Decoder from memory");

    fixed (byte* wavData = wavBytes)
    {
        var decoder = default(MaDecoder);
        var config = ma.DecoderConfigInit(MaFormat.FormatF32, 2, 48000);
        MiniAudioStatus.Require(ma, "ma_decoder_init_memory", ma.DecoderInitMemory((nint)wavData, (nuint)wavBytes.Length, &config, &decoder));

        try
        {
            var format = MaFormat.FormatUnknown;
            uint channels = 0;
            uint sampleRate = 0;
            ulong lengthFrames = 0;

            MiniAudioStatus.Require(
                ma,
                "ma_decoder_get_data_format",
                ma.DecoderGetDataFormat(&decoder, (nint)(&format), (nint)(&channels), (nint)(&sampleRate), 0, 0));

            MiniAudioStatus.Require(
                ma,
                "ma_decoder_get_length_in_pcm_frames",
                ma.DecoderGetLengthInPcmFrames(&decoder, (nint)(&lengthFrames)));

            var preview = new float[previewFrameCount * checked((int)channels)];
            fixed (float* previewData = preview)
            {
                ulong framesRead = 0;
                MiniAudioStatus.Require(
                    ma,
                    "ma_decoder_read_pcm_frames",
                    ma.DecoderReadPcmFrames(&decoder, (nint)previewData, (ulong)previewFrameCount, (nint)(&framesRead)));

                var summary = Analyze(preview.AsSpan(0, checked((int)(framesRead * channels))));
                Print("decoded format", $"{FormatName(ma, format)}, {channels} channel(s), {sampleRate} Hz");
                Print("decoded length", $"{lengthFrames} frame(s)");
                Print("decoded preview", Describe(summary));
            }

            MiniAudioStatus.Require(ma, "ma_decoder_seek_to_pcm_frame", ma.DecoderSeekToPcmFrame(&decoder, lengthFrames / 2));
            Print("ma_decoder_seek_to_pcm_frame", $"cursor moved to frame {lengthFrames / 2}");
        }
        finally
        {
            _ = ma.DecoderUninit(&decoder);
        }
    }
}

// Shows miniaudio's procedural data sources and direct DSP filter APIs.
static unsafe void ReadProceduralSources(Ma ma, uint sampleRate, uint channels, int frameCount)
{
    Section("Procedural sources and DSP");

    var waveformFrames = new float[frameCount * checked((int)channels)];
    var filteredFrames = new float[waveformFrames.Length];
    var noiseFrames = new float[waveformFrames.Length];

    ReadWaveform(ma, sampleRate, channels, frameCount, waveformFrames);
    ReadNoise(ma, channels, frameCount, noiseFrames);
    FilterWithLowPass(ma, sampleRate, channels, frameCount, waveformFrames, filteredFrames);

    Print("ma_waveform_read_pcm_frames", Describe(Analyze(waveformFrames)));
    Print("ma_noise_read_pcm_frames", Describe(Analyze(noiseFrames)));
    Print("ma_lpf_process_pcm_frames", Describe(Analyze(filteredFrames)));
}

// Builds an engine graph, attaches sounds to a group, applies spatial controls, and renders offline PCM.
static unsafe float[] RenderOfflineEngine(Ma ma, uint sampleRate, uint channels, int seconds)
{
    Section("Offline engine, groups, resources, and spatialization");

    var engine = default(MaEngine);
    var engineConfig = ma.EngineConfigInit();
    engineConfig.Channels = channels;
    engineConfig.SampleRate = sampleRate;
    engineConfig.ListenerCount = 1;
    engineConfig.NoDevice = 1;

    MiniAudioStatus.Require(ma, "ma_engine_init", ma.EngineInit(&engineConfig, &engine));

    try
    {
        Print("ma_engine_get_channels", ma.EngineGetChannels(&engine).ToString(CultureInfo.InvariantCulture));
        Print("ma_engine_get_sample_rate", $"{ma.EngineGetSampleRate(&engine)} Hz");

        ma.EngineListenerSetPosition(&engine, 0, 0, 0, 0);
        ma.EngineListenerSetDirection(&engine, 0, 0, 0, -1);

        var group = default(MaSound);
        MiniAudioStatus.Require(ma, "ma_sound_group_init", ma.SoundGroupInit(&engine, 0, null, &group));

        try
        {
            ma.SoundGroupSetVolume(&group, 0.65f);
            ma.SoundGroupSetPan(&group, -0.2f);

            var sourceFrames = checked((int)sampleRate * seconds);
            var registeredPcm = CreateSinePcm(sampleRate, channels, sourceFrames, frequency: 660, amplitude: 0.22);

            fixed (float* registeredPcmData = registeredPcm)
                return RenderEngineSounds(ma, &engine, &group, registeredPcmData, (ulong)sourceFrames, sampleRate, channels, seconds);
        }
        finally
        {
            ma.SoundGroupUninit(&group);
        }
    }
    finally
    {
        ma.EngineUninit(&engine);
    }
}

// Writes interleaved floating-point PCM samples as a 16-bit WAV file.
static void WritePcm16Wav(string path, ReadOnlySpan<float> samples, uint sampleRate, uint channels)
{
    var dataSize = checked(samples.Length * Pcm16BytesPerSample);
    var wav = new byte[Pcm16WavHeaderByteCount + dataSize];

    WritePcm16WavHeader(wav, dataSize, sampleRate, channels);

    var destination = wav.AsSpan(Pcm16WavHeaderByteCount);
    for (var i = 0; i < samples.Length; i++)
    {
        var offset = i * Pcm16BytesPerSample;
        BinaryPrimitives.WriteInt16LittleEndian(destination.Slice(offset, Pcm16BytesPerSample), FloatToPcm16(samples[i]));
    }

    File.WriteAllBytes(path, wav);
}

// Creates a floating-point sine buffer that can be registered with the resource manager.
static float[] CreateSinePcm(uint sampleRate, uint channels, int frameCount, double frequency, double amplitude)
{
    var channelCount = checked((int)channels);
    var samples = new float[frameCount * channelCount];
    var phaseStep = 2 * Math.PI * frequency / sampleRate;

    for (var frame = 0; frame < frameCount; frame++)
    {
        var sample = (float)(Math.Sin(frame * phaseStep) * amplitude);
        for (var channel = 0; channel < channelCount; channel++)
            samples[frame * channelCount + channel] = sample;
    }

    return samples;
}

// Reads frames from a miniaudio waveform data source.
static unsafe void ReadWaveform(Ma ma, uint sampleRate, uint channels, int frameCount, Span<float> destination)
{
    var waveform = default(MaWaveform);
    var config = ma.WaveformConfigInit(MaFormat.FormatF32, channels, sampleRate, MaWaveformType.WaveformTypeTriangle, 0.35, 220);
    MiniAudioStatus.Require(ma, "ma_waveform_init", ma.WaveformInit(in config, &waveform));

    try
    {
        fixed (float* destinationData = destination)
        {
            ulong framesRead = 0;
            MiniAudioStatus.Require(
                ma,
                "ma_waveform_read_pcm_frames",
                ma.WaveformReadPcmFrames(&waveform, (nint)destinationData, (ulong)frameCount, (nint)(&framesRead)));
        }

        MiniAudioStatus.Require(ma, "ma_waveform_set_type", ma.WaveformSetType(&waveform, MaWaveformType.WaveformTypeSawtooth));
        MiniAudioStatus.Require(ma, "ma_waveform_set_frequency", ma.WaveformSetFrequency(&waveform, 440));
    }
    finally
    {
        ma.WaveformUninit(&waveform);
    }
}

// Reads frames from a miniaudio pink-noise data source.
static unsafe void ReadNoise(Ma ma, uint channels, int frameCount, Span<float> destination)
{
    var noise = default(MaNoise);
    var config = ma.NoiseConfigInit(MaFormat.FormatF32, channels, MaNoiseType.NoiseTypePink, seed: 12345, amplitude: 0.12);
    MiniAudioStatus.Require(ma, "ma_noise_init", ma.NoiseInit(&config, null, &noise));

    try
    {
        fixed (float* destinationData = destination)
        {
            ulong framesRead = 0;
            MiniAudioStatus.Require(
                ma,
                "ma_noise_read_pcm_frames",
                ma.NoiseReadPcmFrames(&noise, (nint)destinationData, (ulong)frameCount, (nint)(&framesRead)));
        }
    }
    finally
    {
        ma.NoiseUninit(&noise, null);
    }
}

// Processes a source buffer through miniaudio's low-pass filter API.
static unsafe void FilterWithLowPass(Ma ma, uint sampleRate, uint channels, int frameCount, ReadOnlySpan<float> source, Span<float> destination)
{
    var filter = default(MaLpf);
    var config = ma.LpfConfigInit(MaFormat.FormatF32, channels, sampleRate, cutoffFrequency: 900, order: 4);
    MiniAudioStatus.Require(ma, "ma_lpf_init", ma.LpfInit(&config, null, &filter));

    try
    {
        fixed (float* sourceData = source)
        fixed (float* destinationData = destination)
        {
            MiniAudioStatus.Require(
                ma,
                "ma_lpf_process_pcm_frames",
                ma.LpfProcessPcmFrames(&filter, (nint)destinationData, (nint)sourceData, (ulong)frameCount));
        }
    }
    finally
    {
        ma.LpfUninit(&filter, null);
    }
}

// Registers decoded memory and renders a procedural waveform plus the registered sound through an offline engine.
static unsafe float[] RenderEngineSounds(
    Ma ma,
    MaEngine* engine,
    MaSound* group,
    float* registeredPcm,
    ulong registeredFrameCount,
    uint sampleRate,
    uint channels,
    int seconds)
{
    const string registeredName = "alvorkit.generated-tone";

    var resourceManager = ma.EngineGetResourceManager(engine);
    if (resourceManager == null)
        throw new InvalidOperationException("ma_engine_get_resource_manager returned null.");

    MiniAudioStatus.Require(
        ma,
        "ma_resource_manager_register_decoded_data",
        ma.ResourceManagerRegisterDecodedData(
            resourceManager,
            registeredName,
            (nint)registeredPcm,
            registeredFrameCount,
            MaFormat.FormatF32,
            channels,
            sampleRate));

    try
    {
        var waveform = default(MaWaveform);
        var waveformConfig = ma.WaveformConfigInit(MaFormat.FormatF32, channels, sampleRate, MaWaveformType.WaveformTypeSine, 0.18, 220);
        MiniAudioStatus.Require(ma, "ma_waveform_init", ma.WaveformInit(in waveformConfig, &waveform));

        try
        {
            var waveformSound = default(MaSound);
            MiniAudioStatus.Require(ma, "ma_sound_init_from_data_source", ma.SoundInitFromDataSource(engine, (nint)(&waveform), 0, group, &waveformSound));

            try
            {
                var resourceSound = default(MaSound);
                var resourceFlags = (uint)MaSoundFlags.SoundFlagNoSpatialization;
                MiniAudioStatus.Require(
                    ma,
                    "ma_sound_init_from_file",
                    ma.SoundInitFromFile(engine, registeredName, resourceFlags, group, null, &resourceSound));

                try
                {
                    ConfigureEngineSounds(ma, engine, &waveformSound, &resourceSound);
                    return ReadEngineMix(ma, engine, sampleRate, channels, seconds);
                }
                finally
                {
                    ma.SoundUninit(&resourceSound);
                }
            }
            finally
            {
                ma.SoundUninit(&waveformSound);
            }
        }
        finally
        {
            ma.WaveformUninit(&waveform);
        }
    }
    finally
    {
        _ = ma.ResourceManagerUnregisterData(resourceManager, registeredName);
    }
}

// Configures sound routing, group mixing, fade, pitch, pan, and spatial placement before rendering.
static unsafe void ConfigureEngineSounds(Ma ma, MaEngine* engine, MaSound* waveformSound, MaSound* resourceSound)
{
    ma.SoundSetVolume(waveformSound, 0.8f);
    ma.SoundSetPosition(waveformSound, x: -2.0f, y: 0, z: -3.0f);
    ma.SoundSetVelocity(waveformSound, x: 0.5f, y: 0, z: 0);
    ma.SoundSetMinDistance(waveformSound, 1.0f);
    ma.SoundSetFadeInMilliseconds(waveformSound, volumeBeg: 0, volumeEnd: 1, fadeLengthInMilliseconds: 150);
    ma.SoundSetStopTimeWithFadeInMilliseconds(waveformSound, stopAbsoluteGlobalTimeInMilliseconds: 1800, fadeLengthInMilliseconds: 200);

    ma.SoundSetVolume(resourceSound, 0.55f);
    ma.SoundSetPan(resourceSound, 0.35f);
    ma.SoundSetPitch(resourceSound, 0.75f);
    ma.SoundSetFadeInMilliseconds(resourceSound, volumeBeg: 0, volumeEnd: 1, fadeLengthInMilliseconds: 100);

    MiniAudioStatus.Require(ma, "ma_sound_start", ma.SoundStart(waveformSound));
    MiniAudioStatus.Require(ma, "ma_sound_start", ma.SoundStart(resourceSound));

    var listener = ma.EngineListenerGetPosition(engine, 0);
    var source = ma.SoundGetPosition(waveformSound);
    Print("listener position", FormatVec3(listener));
    Print("spatialized sound position", FormatVec3(source));
    Print("resource sound pitch", ma.SoundGetPitch(resourceSound).ToString("0.00", CultureInfo.InvariantCulture));
}

// Reads the current engine mix into a managed floating-point buffer.
static unsafe float[] ReadEngineMix(Ma ma, MaEngine* engine, uint sampleRate, uint channels, int seconds)
{
    var framesToRead = checked((int)sampleRate * seconds);
    var mix = new float[framesToRead * checked((int)channels)];

    fixed (float* mixData = mix)
    {
        ulong framesRead = 0;
        MiniAudioStatus.Require(
            ma,
            "ma_engine_read_pcm_frames",
            ma.EngineReadPcmFrames(engine, (nint)mixData, (ulong)framesToRead, (nint)(&framesRead)));

        var rendered = mix.AsSpan(0, checked((int)(framesRead * channels)));
        Print("ma_engine_read_pcm_frames", $"{framesRead} frame(s), {Describe(Analyze(rendered))}");

        if (rendered.Length != mix.Length)
            return rendered.ToArray();
    }

    return mix;
}

// Writes the fixed RIFF/WAVE PCM header used by both demo-generated WAV payloads.
static void WritePcm16WavHeader(Span<byte> wav, int dataSize, uint sampleRate, uint channels)
{
    "RIFF"u8.CopyTo(wav[..4]);
    BinaryPrimitives.WriteInt32LittleEndian(wav.Slice(4, 4), 36 + dataSize);
    "WAVE"u8.CopyTo(wav.Slice(8, 4));
    "fmt "u8.CopyTo(wav.Slice(12, 4));
    BinaryPrimitives.WriteInt32LittleEndian(wav.Slice(16, 4), 16);
    BinaryPrimitives.WriteInt16LittleEndian(wav.Slice(20, 2), 1);
    BinaryPrimitives.WriteInt16LittleEndian(wav.Slice(22, 2), checked((short)channels));
    BinaryPrimitives.WriteInt32LittleEndian(wav.Slice(24, 4), checked((int)sampleRate));
    BinaryPrimitives.WriteInt32LittleEndian(wav.Slice(28, 4), checked((int)(sampleRate * channels * Pcm16BytesPerSample)));
    BinaryPrimitives.WriteInt16LittleEndian(wav.Slice(32, 2), checked((short)(channels * Pcm16BytesPerSample)));
    BinaryPrimitives.WriteInt16LittleEndian(wav.Slice(34, 2), 16);
    "data"u8.CopyTo(wav.Slice(36, 4));
    BinaryPrimitives.WriteInt32LittleEndian(wav.Slice(40, 4), dataSize);
}

// Writes 16-bit sine samples into an interleaved PCM payload.
static void WriteSinePcm16(Span<byte> destination, uint sampleRate, uint channels, double frequency, double amplitude)
{
    var channelCount = checked((int)channels);
    var frameCount = destination.Length / (channelCount * Pcm16BytesPerSample);
    var phaseStep = 2 * Math.PI * frequency / sampleRate;

    for (var frame = 0; frame < frameCount; frame++)
    {
        var sample = FloatToPcm16((float)(Math.Sin(frame * phaseStep) * amplitude));
        for (var channel = 0; channel < channelCount; channel++)
        {
            var offset = checked((frame * channelCount + channel) * Pcm16BytesPerSample);
            BinaryPrimitives.WriteInt16LittleEndian(destination.Slice(offset, Pcm16BytesPerSample), sample);
        }
    }
}

// Returns the backend display name for a generated enum value.
static string BackendName(Ma ma, MaBackendKind backend)
{
    ma.GetBackendName(backend, out var name);
    return name ?? backend.ToString();
}

// Returns the format display name for a generated enum value.
static string FormatName(Ma ma, MaFormat format)
{
    ma.GetFormatName(format, out var name);
    return name ?? format.ToString();
}

// Computes simple peak and RMS values for a sample block.
static PcmSummary Analyze(ReadOnlySpan<float> samples)
{
    var peak = 0.0f;
    var squareSum = 0.0;

    foreach (var sample in samples)
    {
        var absolute = Math.Abs(sample);
        if (absolute > peak)
            peak = absolute;

        squareSum += sample * sample;
    }

    var rms = samples.Length == 0 ? 0.0f : (float)Math.Sqrt(squareSum / samples.Length);
    return new PcmSummary(samples.Length, peak, rms);
}

// Formats an analyzed PCM summary for console output.
static string Describe(PcmSummary summary) =>
    $"{summary.SampleCount} sample(s), peak {summary.Peak:0.000}, RMS {summary.Rms:0.000}";

// Formats a miniaudio three-dimensional vector.
static string FormatVec3(MaVec3f value) =>
    $"({value.X:0.00}, {value.Y:0.00}, {value.Z:0.00})";

// Converts a normalized floating-point sample into signed 16-bit PCM.
static short FloatToPcm16(float sample)
{
    var scaled = Math.Clamp(sample, -1.0f, 1.0f) * short.MaxValue;
    return (short)MathF.Round(scaled);
}

// Prints a section heading in the console walkthrough.
static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

// Prints one aligned label/value row.
static void Print(string label, string value) =>
    Console.WriteLine($"{label,-46} {value}");

/// <summary>Small result-checking helper for miniaudio calls that return <see cref="MaResult"/>.</summary>
internal static class MiniAudioStatus
{
    /// <summary>Throws with miniaudio's result description when a native call fails.</summary>
    public static void Require(Ma ma, string nativeName, MaResult result)
    {
        if (result == MaResult.Success)
            return;

        ma.ResultDescription(result, out var description);
        throw new InvalidOperationException($"{nativeName} returned {result}: {description ?? "no description"}.");
    }
}

/// <summary>A compact analysis of an interleaved PCM sample block.</summary>
internal readonly record struct PcmSummary(int SampleCount, float Peak, float Rms);
