namespace AlvorKit.XxHash.Test;

/// <summary>Covers generated xxHash APIs against the packaged native backend.</summary>
[TestClass]
public sealed class XxHashRuntimeTest
{
    private const uint Seed32 = 0x9E37_79B1u;
    private const ulong Seed64 = 0x9E37_79B1_85EB_CA87ul;
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
