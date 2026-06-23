namespace AlvorKit.XxHash.Test;

/// <summary>Covers generated xxHash APIs against the packaged native backend.</summary>
[TestClass]
public sealed class XxHashRuntimeTest
{
    private const uint Seed32 = 0x9E37_79B1u;
    private const ulong Seed64 = 0x9E37_79B1_85EB_CA87ul;
    private const string KnownAnswerGeneratedSecretHex =
        "2BFD3FE6E764B23A913F35752F631F44B3B24B2C4086B240344980C046399A18" +
        "B0A7E261100F5B188C51FA7E62B0AB3EC4713B2F136C86C990AD0EFA896FD22" +
        "C8ECE5E0BFB68CAA50ED99ED0B5024ECDFDA3968C79AF609D0D351BB0D896F" +
        "101DDE5427C9F19EC116281FB5744A5AE3A19A2A4C7C0FC1251799000A19F8" +
        "1AC5A61341486E74A92FAA520841EF4C6681807D12E6FC0CE21EB85032319E" +
        "EB024CFA9CD897DAFA0F5D5A8652334C546E9B705575CA2FFDFB31F0DCEB83" +
        "A757B1B95";
    private const int VersionMajor = (int)XxhEnum.VersionMajor;
    private const int VersionMinor = (int)XxhEnum.VersionMinor;
    private const int VersionRelease = (int)XxhEnum.VersionRelease;
    private const int VersionNumber = (int)XxhEnum.VersionNumber;
    private const int Xxh3SecretDefaultSize = (int)XxhEnum.Xxh3SecretDefaultSize;
    private const int Xxh3SecretSizeMin = (int)XxhEnum.Xxh3SecretSizeMin;

    /// <summary>Verifies that the backend reports the same version and exported constants as the generated API surface.</summary>
    [TestMethod]
    public void BackendPackage_ExposesDemoVersionAndConstants()
    {
        Xxh xxh = new XxhBackend();

        Assert.AreEqual((uint)VersionNumber, xxh.GetVersionNumber());
        Assert.AreEqual(VersionMajor * 10_000 + VersionMinor * 100 + VersionRelease, VersionNumber);
        Assert.AreEqual(0, VersionMajor);
        Assert.AreEqual(8, VersionMinor);
        Assert.AreEqual(3, VersionRelease);
    }

    /// <summary>Verifies a small xxHash 0.8.3 known-answer subset generated from the pinned upstream C source.</summary>
    [TestMethod]
    public void HashKnownAnswerSubset_MatchesPinnedUpstreamVectors()
    {
        Xxh xxh = new XxhBackend();
        Span<byte> secret = stackalloc byte[Xxh3SecretDefaultSize];

        RequireOk(xxh.GenerateHash3Secret(secret, KnownAnswerMaterial()));
        CollectionAssert.AreEqual(ParseHex(KnownAnswerGeneratedSecretHex), secret.ToArray());

        foreach (var (
            length,
            xxh32,
            xxh64,
            xxh3To64,
            xxh3To64Seed,
            xxh3To64Secret,
            xxh3To64SecretSeed,
            xxh3To128,
            xxh3To128Seed,
            xxh3To128Secret,
            xxh3To128SecretSeed) in KnownAnswerSubset())
        {
            var input = KnownAnswerPayload(length);

            Assert.AreEqual(xxh32, xxh.Hash32(input, Seed32), $"XXH32 length {length}.");
            Assert.AreEqual(xxh64, xxh.Hash64(input, Seed64), $"XXH64 length {length}.");
            Assert.AreEqual(xxh3To64, xxh.Hash3To64(input), $"XXH3_64bits length {length}.");
            Assert.AreEqual(xxh3To64Seed, xxh.Hash3To64(input, Seed64), $"XXH3_64bits_withSeed length {length}.");
            Assert.AreEqual(xxh3To64Secret, xxh.Hash3To64(input, secret), $"XXH3_64bits_withSecret length {length}.");
            Assert.AreEqual(
                xxh3To64SecretSeed,
                xxh.Hash3To64(input, secret, Seed64),
                $"XXH3_64bits_withSecretandSeed length {length}.");

            Assert.AreEqual(xxh3To128, xxh.Hash3To128(input), $"XXH3_128bits length {length}.");
            Assert.AreEqual(xxh3To128Seed, xxh.Hash3To128(input, Seed64), $"XXH3_128bits_withSeed length {length}.");
            Assert.AreEqual(xxh3To128Secret, xxh.Hash3To128(input, secret), $"XXH3_128bits_withSecret length {length}.");
            Assert.AreEqual(
                xxh3To128SecretSeed,
                xxh.Hash3To128(input, secret, Seed64),
                $"XXH3_128bits_withSecretandSeed length {length}.");

            xxh.Hash32ToCanonical(out var canonical32, xxh32);
            xxh.Hash64ToCanonical(out var canonical64, xxh64);
            xxh.Hash128ToCanonical(out var canonical128, xxh3To128);
            CollectionAssert.AreEqual(BigEndian(xxh32), StructBytes(ref canonical32));
            CollectionAssert.AreEqual(BigEndian(xxh64), StructBytes(ref canonical64));
            CollectionAssert.AreEqual(BigEndian(xxh3To128), StructBytes(ref canonical128));
        }
    }

    /// <summary>Checks that XXH32 one-shot, streaming, copied state, and canonical conversion paths agree.</summary>
    [TestMethod]
    public void Hash32_StreamingCopyAndCanonicalRoundTripMatchOneShot()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "xxHash is built for very fast, non-cryptographic hashing."u8;
        var first = input[..28];
        var second = input[28..];
        var expected = xxh.Hash32(input, Seed32);

        var state = xxh.CreateHash32State();
        var copy = xxh.CreateHash32State();
        try
        {
            RequireState(state.Handle);
            RequireState(copy.Handle);
            RequireOk(xxh.ResetHash32(state, Seed32));
            RequireOk(xxh.UpdateHash32(state, first));
            xxh.CopyHash32State(copy, state);
            RequireOk(xxh.UpdateHash32(state, second));
            RequireOk(xxh.UpdateHash32(copy, second));

            Assert.AreEqual(expected, xxh.DigestHash32(state));
            Assert.AreEqual(expected, xxh.DigestHash32(copy));
        }
        finally
        {
            if (copy.Handle != 0)
                xxh.FreeHash32State(copy);
            if (state.Handle != 0)
                xxh.FreeHash32State(state);
        }

        xxh.Hash32ToCanonical(out var canonical, expected);
        Assert.AreEqual(expected, xxh.Hash32FromCanonical(in canonical));
        CollectionAssert.AreEqual(BigEndian(expected), StructBytes(ref canonical));
    }

    /// <summary>Checks that XXH64 one-shot, streaming, copied state, and canonical conversion paths agree.</summary>
    [TestMethod]
    public void Hash64_StreamingCopyAndCanonicalRoundTripMatchOneShot()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash package payload"u8;
        var first = input[..15];
        var second = input[15..];
        var expected = xxh.Hash64(input, Seed64);

        var state = xxh.CreateHash64State();
        var copy = xxh.CreateHash64State();
        try
        {
            RequireState(state.Handle);
            RequireState(copy.Handle);
            RequireOk(xxh.ResetHash64(state, Seed64));
            RequireOk(xxh.UpdateHash64(state, first));
            xxh.CopyHash64State(copy, state);
            RequireOk(xxh.UpdateHash64(state, second));
            RequireOk(xxh.UpdateHash64(copy, second));

            Assert.AreEqual(expected, xxh.DigestHash64(state));
            Assert.AreEqual(expected, xxh.DigestHash64(copy));
        }
        finally
        {
            if (copy.Handle != 0)
                xxh.FreeHash64State(copy);
            if (state.Handle != 0)
                xxh.FreeHash64State(state);
        }

        xxh.Hash64ToCanonical(out var canonical, expected);
        Assert.AreEqual(expected, xxh.Hash64FromCanonical(in canonical));
        CollectionAssert.AreEqual(BigEndian(expected), StructBytes(ref canonical));
    }

    /// <summary>Verifies the pointer-shaped overloads used by the benchmark demo against the managed span overloads.</summary>
    [TestMethod]
    public unsafe void NativePointerOverloads_MatchSpanOverloadsUsedByBenchmarkDemo()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash benchmark payload"u8;

        fixed (byte* inputPtr = input)
        {
            var pointer = (nint)inputPtr;
            var length = (nuint)input.Length;

            Assert.AreEqual(xxh.Hash32(input, 0), xxh.Hash32(pointer, length, 0));
            Assert.AreEqual(xxh.Hash64(input, 0), xxh.Hash64(pointer, length, 0));
            Assert.AreEqual(xxh.Hash3To64(input), xxh.Hash3To64(pointer, length));
            Assert.AreEqual(xxh.Hash3To128(input), xxh.Hash3To128(pointer, length));
        }
    }

    /// <summary>Checks XxhSecret validation plus custom and seeded secret generation used by the demo.</summary>
    [TestMethod]
    public void XxhSecret_ValidatesAndGeneratesDemoSecretVariants()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> material = "application-specific secret material for the demo"u8;
        Span<byte> firstSeededSecret = stackalloc byte[Xxh3SecretDefaultSize];
        Span<byte> secondSeededSecret = stackalloc byte[Xxh3SecretDefaultSize];

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new XxhSecret((nuint)(Xxh3SecretSizeMin - 1)));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new XxhSecret((nuint)int.MaxValue + 1));

        using XxhSecret secret = new(Xxh3SecretSizeMin);
        Assert.AreEqual((nuint)Xxh3SecretSizeMin, secret.Size);
        Assert.AreNotEqual(0, secret.Pointer);
        Assert.AreEqual(Xxh3SecretSizeMin, secret.Bytes.Length);
        RequireOk(xxh.GenerateHash3Secret(secret.Bytes, material));
        AssertHasAnyNonZero(secret.Bytes);

        xxh.GenerateHash3SecretFromSeed(firstSeededSecret, Seed64);
        xxh.GenerateHash3SecretFromSeed(secondSeededSecret, Seed64);
        CollectionAssert.AreEqual(firstSeededSecret.ToArray(), secondSeededSecret.ToArray());
        AssertHasAnyNonZero(firstSeededSecret);
    }

    /// <summary>Checks XXH3 64-bit default, seeded, secret, and secret-plus-seed one-shot and streaming paths.</summary>
    [TestMethod]
    public void Hash3To64_OneShotAndStreamingVariantsMatch()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash streaming payload with a generated secret"u8;
        var first = input[..24];
        var second = input[24..];
        using var secret = CreateGeneratedSecret(xxh);

        var expectedDefault = xxh.Hash3To64(input);
        var expectedSeed = xxh.Hash3To64(input, Seed64);
        var expectedSecret = xxh.Hash3To64(input, secret.Bytes);
        var expectedSecretAndSeed = xxh.Hash3To64(input, secret.Bytes, Seed64);

        var state = xxh.CreateHash3State();
        var copy = xxh.CreateHash3State();
        try
        {
            RequireState(state.Handle);
            RequireState(copy.Handle);
            RequireOk(xxh.ResetHash3To64(state));
            RequireOk(xxh.UpdateHash3To64(state, first));
            xxh.CopyHash3State(copy, state);
            RequireOk(xxh.UpdateHash3To64(state, second));
            RequireOk(xxh.UpdateHash3To64(copy, second));
            Assert.AreEqual(expectedDefault, xxh.DigestHash3To64(state));
            Assert.AreEqual(expectedDefault, xxh.DigestHash3To64(copy));

            RequireOk(xxh.ResetHash3To64(state, Seed64));
            RequireOk(xxh.UpdateHash3To64(state, input));
            Assert.AreEqual(expectedSeed, xxh.DigestHash3To64(state));

            RequireOk(xxh.ResetHash3To64(state, secret));
            RequireOk(xxh.UpdateHash3To64(state, input));
            Assert.AreEqual(expectedSecret, xxh.DigestHash3To64(state));

            RequireOk(xxh.ResetHash3To64(state, secret, Seed64));
            RequireOk(xxh.UpdateHash3To64(state, input));
            Assert.AreEqual(expectedSecretAndSeed, xxh.DigestHash3To64(state));
        }
        finally
        {
            if (copy.Handle != 0)
                xxh.FreeHash3State(copy);
            if (state.Handle != 0)
                xxh.FreeHash3State(state);
        }
    }

    /// <summary>Checks XXH3 128-bit variants, XXH128 aliasing, comparison, and canonical conversion.</summary>
    [TestMethod]
    public void Hash3To128_OneShotStreamingCanonicalAndCompareVariantsMatch()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash 128-bit streaming payload with a generated secret"u8;
        var first = input[..31];
        var second = input[31..];
        using var secret = CreateGeneratedSecret(xxh);

        var expectedDefault = xxh.Hash3To128(input);
        var expectedSeed = xxh.Hash3To128(input, Seed64);
        var expectedSecret = xxh.Hash3To128(input, secret.Bytes);
        var expectedSecretAndSeed = xxh.Hash3To128(input, secret.Bytes, Seed64);
        var expectedHash128 = xxh.Hash128(input, Seed64);
        var streamingDefault = default(UInt128);

        Assert.AreEqual(expectedSeed, expectedHash128);

        var state = xxh.CreateHash3State();
        try
        {
            RequireState(state.Handle);
            RequireOk(xxh.ResetHash3To128(state));
            RequireOk(xxh.UpdateHash3To128(state, first));
            RequireOk(xxh.UpdateHash3To128(state, second));
            streamingDefault = xxh.DigestHash3To128(state);
            Assert.AreEqual(expectedDefault, streamingDefault);

            RequireOk(xxh.ResetHash3To128(state, Seed64));
            RequireOk(xxh.UpdateHash3To128(state, input));
            Assert.AreEqual(expectedSeed, xxh.DigestHash3To128(state));

            RequireOk(xxh.ResetHash3To128(state, secret));
            RequireOk(xxh.UpdateHash3To128(state, input));
            Assert.AreEqual(expectedSecret, xxh.DigestHash3To128(state));

            RequireOk(xxh.ResetHash3To128(state, secret, Seed64));
            RequireOk(xxh.UpdateHash3To128(state, input));
            Assert.AreEqual(expectedSecretAndSeed, xxh.DigestHash3To128(state));
        }
        finally
        {
            if (state.Handle != 0)
                xxh.FreeHash3State(state);
        }

        Assert.IsTrue(xxh.Hash128Equals(expectedDefault, streamingDefault));
        Assert.IsFalse(xxh.Hash128Equals(expectedDefault, expectedHash128));
        Assert.AreEqual(0, xxh.CompareHash128(expectedDefault, streamingDefault));
        var comparison = Math.Sign(xxh.CompareHash128(expectedDefault, expectedHash128));
        Assert.AreNotEqual(0, comparison);
        Assert.AreEqual(-comparison, Math.Sign(xxh.CompareHash128(expectedHash128, expectedDefault)));

        xxh.Hash128ToCanonical(out var canonical, expectedDefault);
        Assert.AreEqual(expectedDefault, xxh.Hash128FromCanonical(in canonical));
        CollectionAssert.AreEqual(BigEndian(expectedDefault), StructBytes(ref canonical));
    }

    /// <summary>Verifies that the shared XXH3 streaming state can reset between 64-bit and 128-bit modes.</summary>
    [TestMethod]
    public void Hash3State_CanSwitchBetween64And128BitModes()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash shared state mode switch payload"u8;
        using var secret = CreateGeneratedSecret(xxh);

        var state = xxh.CreateHash3State();
        try
        {
            RequireState(state.Handle);
            RequireOk(xxh.ResetHash3To64(state, secret));
            RequireOk(xxh.UpdateHash3To64(state, input));
            Assert.AreEqual(xxh.Hash3To64(input, secret.Bytes), xxh.DigestHash3To64(state));

            RequireOk(xxh.ResetHash3To128(state, secret));
            RequireOk(xxh.UpdateHash3To128(state, input));
            Assert.AreEqual(xxh.Hash3To128(input, secret.Bytes), xxh.DigestHash3To128(state));
        }
        finally
        {
            if (state.Handle != 0)
                xxh.FreeHash3State(state);
        }
    }

    private static XxhSecret CreateGeneratedSecret(Xxh xxh)
    {
        ReadOnlySpan<byte> material = "runtime secret material"u8;
        var secret = new XxhSecret(Xxh3SecretSizeMin);
        RequireOk(xxh.GenerateHash3Secret(secret.Bytes, material));
        return secret;
    }

    private static byte[] KnownAnswerPayload(int length)
    {
        var payload = new byte[length];
        for (var i = 0; i < payload.Length; i++)
            payload[i] = (byte)((i * 131u + 17u) & 0xffu);
        return payload;
    }

    private static byte[] KnownAnswerMaterial()
    {
        var material = new byte[64];
        for (var i = 0; i < material.Length; i++)
            material[i] = (byte)((i * 73u + 41u) & 0xffu);
        return material;
    }

    private static (int Length,
        uint Xxh32,
        ulong Xxh64,
        ulong Xxh3To64,
        ulong Xxh3To64Seed,
        ulong Xxh3To64Secret,
        ulong Xxh3To64SecretSeed,
        UInt128 Xxh3To128,
        UInt128 Xxh3To128Seed,
        UInt128 Xxh3To128Secret,
        UInt128 Xxh3To128SecretSeed)[] KnownAnswerSubset() =>
    [
        (0,
            0x36B7_8AE7u,
            0x6EC6_D05F_61C7_E7A7ul,
            0x2D06_8005_38D3_94C2ul,
            0x07F7_0F81_9703_314Dul,
            0x86EB_764D_11B4_FDEBul,
            0x07F7_0F81_9703_314Dul,
            UInt128From(0x99AA_06D3_0147_98D8ul, 0x6001_C324_468D_497Ful),
            UInt128From(0x45EF_6DDC_7AFB_225Aul, 0xF9EC_E103_6ECB_B2EDul),
            UInt128From(0x6564_4854_738D_9DF0ul, 0xCCC3_4E65_D681_BF28ul),
            UInt128From(0x45EF_6DDC_7AFB_225Aul, 0xF9EC_E103_6ECB_B2EDul)),
        (31,
            0x35E9_C2F1u,
            0x7236_0FC4_2554_9DCFul,
            0x91F3_E7DD_D073_ABE6ul,
            0xF28B_E02A_F2F0_BE6Aul,
            0xC8E0_06D6_EFCD_6879ul,
            0xF28B_E02A_F2F0_BE6Aul,
            UInt128From(0x73F8_8275_1EE3_7F9Bul, 0x1EF8_DA53_01EE_BFB1ul),
            UInt128From(0x9CDD_10BC_D328_2B07ul, 0xCA06_CF4E_8BDC_4493ul),
            UInt128From(0x5F2B_DFFF_5AB3_76FFul, 0x92C0_A0B3_7EA9_4CBEul),
            UInt128From(0x9CDD_10BC_D328_2B07ul, 0xCA06_CF4E_8BDC_4493ul)),
        (241,
            0x3D4B_61B3u,
            0xBFC0_62B1_D4C7_6A43ul,
            0xED93_572E_52AC_AC83ul,
            0xC57A_17FA_E073_6FD1ul,
            0xDC79_1240_23E1_C0ACul,
            0xDC79_1240_23E1_C0ACul,
            UInt128From(0x0622_9596_E1DE_710Aul, 0xED93_572E_52AC_AC83ul),
            UInt128From(0x097B_0293_916E_8E9Cul, 0xC57A_17FA_E073_6FD1ul),
            UInt128From(0x4F68_44C1_B6B1_642Dul, 0xDC79_1240_23E1_C0ACul),
            UInt128From(0x4F68_44C1_B6B1_642Dul, 0xDC79_1240_23E1_C0ACul)),
    ];

    private static UInt128 UInt128From(ulong high64, ulong low64) =>
        ((UInt128)high64 << 64) | low64;

    private static void RequireOk(XxhErrorCode code)
    {
        if (code != XxhErrorCode.Ok)
            throw new AssertFailedException($"xxHash returned {code}.");
    }

    private static void RequireState(nint handle)
    {
        if (handle == 0)
            throw new AssertFailedException("xxHash returned a null state.");
    }

    private static byte[] StructBytes<T>(ref T value)
        where T : unmanaged =>
        MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();

    private static byte[] ParseHex(string hex) =>
        Convert.FromHexString(hex);

    private static byte[] BigEndian(uint value) =>
    [
        (byte)(value >> 24),
        (byte)(value >> 16),
        (byte)(value >> 8),
        (byte)value,
    ];

    private static byte[] BigEndian(ulong value) =>
    [
        (byte)(value >> 56),
        (byte)(value >> 48),
        (byte)(value >> 40),
        (byte)(value >> 32),
        (byte)(value >> 24),
        (byte)(value >> 16),
        (byte)(value >> 8),
        (byte)value,
    ];

    private static byte[] BigEndian(UInt128 value)
    {
        var bytes = new byte[16];
        for (var i = bytes.Length - 1; i >= 0; i--)
        {
            bytes[i] = (byte)value;
            value >>= 8;
        }

        return bytes;
    }

    private static void AssertHasAnyNonZero(ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
        {
            if (value != 0)
                return;
        }

        throw new AssertFailedException("Expected the generated secret to contain at least one non-zero byte.");
    }
}
