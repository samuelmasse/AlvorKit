namespace AlvorKit.XxHash.Test;

/// <summary>Covers generated xxHash APIs against the packaged native backend.</summary>
[TestClass]
public sealed class XxHashRuntimeTest
{
    private const uint Seed32 = 0x9E37_79B1u;
    private const ulong Seed64 = 0x9E37_79B1_85EB_CA87ul;

    [TestMethod]
    public void BackendPackage_HashesOneShotAndStreamingPayload()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash package payload"u8;
        var first = input[..15];
        var second = input[15..];
        Assert.AreEqual((uint)803, xxh.GetVersionNumber());

        var state32 = xxh.CreateHash32State();
        var state64 = xxh.CreateHash64State();
        try
        {
            RequireState(state32.Handle);
            RequireState(state64.Handle);
            RequireOk(xxh.ResetHash32(state32, Seed32));
            RequireOk(xxh.UpdateHash32(state32, first));
            RequireOk(xxh.UpdateHash32(state32, second));
            Assert.AreEqual(xxh.Hash32(input, Seed32), xxh.DigestHash32(state32));

            RequireOk(xxh.ResetHash64(state64, Seed64));
            RequireOk(xxh.UpdateHash64(state64, first));
            RequireOk(xxh.UpdateHash64(state64, second));
            Assert.AreEqual(xxh.Hash64(input, Seed64), xxh.DigestHash64(state64));
        }
        finally
        {
            if (state32.Handle != 0)
                xxh.FreeHash32State(state32);
            if (state64.Handle != 0)
                xxh.FreeHash64State(state64);
        }
    }

#if LOCAL_BINDINGS
    [TestMethod]
    public void UInt128Results_RoundTripCanonicalAndCompare()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash test payload"u8;
        var hash = xxh.Hash3To128(input);
        var seededHash = xxh.Hash128(input, Seed64);

        Assert.IsTrue(xxh.Hash128Equals(hash, hash));
        Assert.IsFalse(xxh.Hash128Equals(hash, seededHash));
        Assert.AreEqual(0, xxh.CompareHash128(hash, hash));
        var comparison = Math.Sign(xxh.CompareHash128(hash, seededHash));
        Assert.AreNotEqual(0, comparison);
        Assert.AreEqual(-comparison, Math.Sign(xxh.CompareHash128(seededHash, hash)));

        var canonical = default(XxhCanonical128);
        xxh.Hash128ToCanonical(out canonical, hash);
        Assert.AreEqual(hash, xxh.Hash128FromCanonical(in canonical));
    }

    [TestMethod]
    public void Streaming128_MatchesOneShot()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash streaming payload"u8;
        var expected = xxh.Hash3To128(input, Seed64);
        var state = xxh.CreateHash3State();
        try
        {
            RequireState(state.Handle);
            RequireOk(xxh.ResetHash3To128(state, Seed64));
            RequireOk(xxh.UpdateHash3To128(state, input[..16]));
            RequireOk(xxh.UpdateHash3To128(state, input[16..]));
            Assert.AreEqual(expected, xxh.DigestHash3To128(state));
        }
        finally
        {
            if (state.Handle != 0)
                xxh.FreeHash3State(state);
        }
    }

    [TestMethod]
    public void XxhSecret_ValidatesAndDrivesSecretResetOverloads()
    {
        Xxh xxh = new XxhBackend();
        ReadOnlySpan<byte> input = "runtime xxhash secret payload"u8;
        ReadOnlySpan<byte> material = "runtime secret material"u8;
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new XxhSecret((nuint)(Xxh.Xxh3SecretSizeMin - 1)));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new XxhSecret((nuint)int.MaxValue + 1));

        using XxhSecret secret = new(Xxh.Xxh3SecretSizeMin);
        Assert.AreEqual((nuint)Xxh.Xxh3SecretSizeMin, secret.Size);
        Assert.AreNotEqual(0, secret.Pointer);
        Assert.AreEqual(Xxh.Xxh3SecretSizeMin, secret.Bytes.Length);
        RequireOk(xxh.GenerateHash3Secret(secret.Bytes, material));

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
#endif

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
}
