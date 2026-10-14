namespace AlvorKit;

/// <summary>Exercises the packaged native audio ABI without requiring an audio device or game assets.</summary>
internal static class Program
{
    /// <summary>Mixes a waveform, verifies controls and seeking, then releases sounds before the engine and resources.</summary>
    private static unsafe void Main()
    {
        Ma ma = new MaBackend();
        var resources = (MaResourceManager*)NativeMemory.AllocZeroed((nuint)sizeof(MaResourceManager));
        var engine = (MaEngine*)NativeMemory.AllocZeroed((nuint)sizeof(MaEngine));
        var waveform = (MaWaveform*)NativeMemory.AllocZeroed((nuint)sizeof(MaWaveform));
        var sound = (MaSound*)NativeMemory.AllocZeroed((nuint)sizeof(MaSound));
        var resourceReady = false;
        var engineReady = false;
        var waveformReady = false;
        var soundReady = false;

        try
        {
            var resourceConfig = ma.ResourceManagerConfigInitVorbis();
            resourceConfig.DecodedFormat = MaFormat.FormatF32;
            resourceConfig.DecodedSampleRate = 48000;

            if (resourceConfig.CustomDecodingBackendCount != 1 || resourceConfig.PpCustomDecodingBackendVTables == 0)
                throw new InvalidOperationException("The native resource manager is missing its Vorbis backend.");

            Require(ma.ResourceManagerInit(&resourceConfig, resources));
            resourceReady = true;
            var engineConfig = ma.EngineConfigInit();
            engineConfig.PResourceManager = resources;
            engineConfig.Channels = 2;
            engineConfig.SampleRate = 48000;
            engineConfig.NoDevice = 1;
            Require(ma.EngineInit(&engineConfig, engine));
            engineReady = true;
            var waveformConfig = ma.WaveformConfigInit(MaFormat.FormatF32, 2, 48000, MaWaveformType.WaveformTypeSine, 0.5, 440);
            Require(ma.WaveformInit(in waveformConfig, waveform));
            waveformReady = true;
            Require(ma.SoundInitFromDataSource(engine, (nint)waveform, (uint)MaSoundFlags.SoundFlagNoSpatialization, null, sound));
            soundReady = true;
            Require(ma.SoundStart(sound));
            Span<float> samples = stackalloc float[1024];

            fixed (float* output = samples)
                Require(ma.EngineReadPcmFrames(engine, (nint)output, 512, 0));

            var energy = 0f;

            foreach (var sample in samples)
            {
                if (!float.IsFinite(sample))
                    throw new InvalidOperationException("The packaged mixer produced non-finite PCM.");

                energy += sample * sample;
            }

            if (energy <= 1 || !ma.SoundIsPlaying(sound))
                throw new InvalidOperationException("The packaged mixer did not play the waveform.");

            ma.SoundSetVolume(sound, 0.25f);

            if (ma.SoundGetVolume(sound) != 0.25f)
                throw new InvalidOperationException("Volume did not round-trip through the native sound.");

            Require(ma.SoundSeekToPcmFrame(sound, 128));
            Require(ma.SoundStop(sound));

            if (ma.SoundIsPlaying(sound))
                throw new InvalidOperationException("SoundStop did not stop the native voice.");

            new VorbisProbe(ma).Verify(engine);
            Console.WriteLine($"PASS {RuntimeInformation.RuntimeIdentifier}: native workers, PCM, volume, Vorbis decode/stream/seek/loop.");
        }
        finally
        {
            if (soundReady)
                ma.SoundUninit(sound);

            if (waveformReady)
                ma.WaveformUninit(waveform);

            if (engineReady)
                ma.EngineUninit(engine);

            if (resourceReady)
                ma.ResourceManagerUninit(resources);

            NativeMemory.Free(sound);
            NativeMemory.Free(waveform);
            NativeMemory.Free(engine);
            NativeMemory.Free(resources);
        }
    }

    /// <summary>Rejects a failed native operation at the call that produced it.</summary>
    private static void Require(MaResult result)
    {
        if (result != MaResult.Success)
            throw new InvalidOperationException($"MiniAudio returned {result}.");
    }
}
