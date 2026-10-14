namespace AlvorKit;

/// <summary>Verifies real Vorbis decoding and streaming with an authored one-second stereo tone.</summary>
internal unsafe class VorbisProbe(Ma ma)
{
    /// <summary>Checks standalone decoding and a streamed voice on the supplied production engine.</summary>
    internal void Verify(MaEngine* engine)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tone.ogg");
        VerifyDecoder(File.ReadAllBytes(path));
        MaSound sound = default;
        var flags = (uint)(MaSoundFlags.SoundFlagStream | MaSoundFlags.SoundFlagNoSpatialization);

        if (OperatingSystem.IsWindows())
        {
            fixed (char* filePath = path)
                Require(ma.SoundInitFromFileW(engine, (nint)filePath, flags, null, null, &sound));
        }
        else Require(ma.SoundInitFromFile(engine, path, flags, null, null, &sound));

        try
        {
            ulong length;
            Require(ma.SoundGetLengthInPcmFrames(&sound, (nint)(&length)));

            if (length != 48000)
                throw new InvalidOperationException($"Vorbis stream length was {length}, expected 48000 frames.");

            ma.SoundSetLooping(&sound, true);
            Require(ma.SoundSeekToPcmFrame(&sound, 47500));
            Require(ma.SoundStart(&sound));
            float* output = stackalloc float[1024];
            var energy = 0f;

            for (var batch = 0; batch < 12; batch++)
            {
                Require(ma.EngineReadPcmFrames(engine, (nint)output, 512, 0));
                energy += Energy(new ReadOnlySpan<float>(output, 1024));
                Thread.Sleep(11);
            }

            if (energy <= 1 || ma.SoundAtEnd(&sound))
                throw new InvalidOperationException("The Vorbis stream did not produce PCM across its loop boundary.");

            Require(ma.SoundStop(&sound));
        }
        finally
        {
            ma.SoundUninit(&sound);
        }
    }

    /// <summary>Exercises the returned-by-value decoder configuration and exact length and seek behavior.</summary>
    private void VerifyDecoder(byte[] encoded)
    {
        MaDecoder decoder = default;
        var config = ma.DecoderConfigInitVorbis(MaFormat.FormatF32, 2, 48000);

        fixed (byte* input = encoded)
        {
            Require(ma.DecoderInitMemory((nint)input, (nuint)encoded.Length, &config, &decoder));

            try
            {
                ulong length;
                Require(ma.DecoderGetLengthInPcmFrames(&decoder, (nint)(&length)));

                if (length != 48000)
                    throw new InvalidOperationException($"Vorbis decoder length was {length}, expected 48000 frames.");

                Require(ma.DecoderSeekToPcmFrame(&decoder, 24000));
                float* output = stackalloc float[1024];
                ulong frames;
                Require(ma.DecoderReadPcmFrames(&decoder, (nint)output, 512, (nint)(&frames)));

                if (frames != 512 || Energy(new ReadOnlySpan<float>(output, 1024)) <= 1)
                    throw new InvalidOperationException("The Vorbis decoder did not produce PCM after seeking.");
            }
            finally
            {
                Require(ma.DecoderUninit(&decoder));
            }
        }
    }

    /// <summary>Checks finite output and measures nonzero decoded audio.</summary>
    private static float Energy(ReadOnlySpan<float> samples)
    {
        var energy = 0f;

        foreach (var sample in samples)
        {
            if (!float.IsFinite(sample))
                throw new InvalidOperationException("Vorbis produced non-finite PCM.");

            energy += sample * sample;
        }

        return energy;
    }

    /// <summary>Reports a failed decoder or sound operation at its source.</summary>
    private static void Require(MaResult result)
    {
        if (result != MaResult.Success)
            throw new InvalidOperationException($"Vorbis returned {result}.");
    }
}
