using System.Globalization;
using AlvorKit.XxHash;
using static XxHashDemo;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

ReadOnlySpan<byte> input = "xxHash is built for very fast, non-cryptographic hashing."u8;
ReadOnlySpan<byte> firstChunk = input[..28];
ReadOnlySpan<byte> secondChunk = input[28..];
ReadOnlySpan<byte> secretMaterial = "application-specific secret material for the demo"u8;

const uint Seed32 = 0x9E37_79B1u;
const ulong Seed64 = 0x9E37_79B1_85EB_CA87ul;

Xxh xxh = new XxhBackend();

Section("Backend and constants");

// XXH_versionNumber -> GetVersionNumber: ask the loaded native library for its encoded version number.
var runtimeVersion = xxh.GetVersionNumber();
Console.WriteLine($"XXH_versionNumber / GetVersionNumber: {runtimeVersion} ({VersionString(runtimeVersion)})");

// XXH_VERSION_* and XXH3_* constants are emitted as static fields on Xxh.
Console.WriteLine($"XXH_VERSION_NUMBER / Xxh.VersionNumber: {Xxh.VersionNumber} ({Xxh.VersionMajor}.{Xxh.VersionMinor}.{Xxh.VersionRelease})");
Console.WriteLine($"XXH3_SECRET_SIZE_MIN / Xxh.Xxh3SecretSizeMin: {Xxh.Xxh3SecretSizeMin} bytes");
Console.WriteLine($"XXH3_SECRET_DEFAULT_SIZE / Xxh.Xxh3SecretDefaultSize: {Xxh.Xxh3SecretDefaultSize} bytes");
Console.WriteLine($"XXH3_MIDSIZE_MAX / Xxh.Xxh3MidsizeMax: {Xxh.Xxh3MidsizeMax} bytes");
Console.WriteLine($"XXH3_INTERNALBUFFER_SIZE / Xxh.Xxh3InternalbufferSize: {Xxh.Xxh3InternalbufferSize} bytes");

Section("XXH32 one-shot, streaming, copy, canonical");

// XXH32 -> Hash32: one-shot 32-bit hashing. The span overload pins managed data and forwards byte length to C.
var xxh32 = xxh.Hash32(input, Seed32);
Print("XXH32 / Hash32", Hex32(xxh32));

var xxh32State = xxh.CreateHash32State();
var xxh32Copy = xxh.CreateHash32State();
try
{
    RequireState(xxh32State.Handle, "XXH32_createState");
    RequireState(xxh32Copy.Handle, "XXH32_createState");

    // XXH32_reset -> ResetHash32: initialize a streaming state with the same seed as the one-shot call.
    RequireOk(xxh.ResetHash32(xxh32State, Seed32), "XXH32_reset");

    // XXH32_update -> UpdateHash32: feed any number of chunks into the streaming state.
    RequireOk(xxh.UpdateHash32(xxh32State, firstChunk), "XXH32_update");

    // XXH32_copyState -> CopyHash32State: duplicate a partially consumed stream so both can continue independently.
    xxh.CopyHash32State(xxh32Copy, xxh32State);

    RequireOk(xxh.UpdateHash32(xxh32State, secondChunk), "XXH32_update");
    RequireOk(xxh.UpdateHash32(xxh32Copy, secondChunk), "XXH32_update");

    // XXH32_digest -> DigestHash32: read the hash from the current stream without destroying the state.
    var stream32 = xxh.DigestHash32(xxh32State);
    var copied32 = xxh.DigestHash32(xxh32Copy);
    Print("XXH32_digest / DigestHash32", Hex32(stream32));
    Print("XXH32_copyState / CopyHash32State", HashMatch(stream32, copied32));
}
finally
{
    // XXH32_freeState -> FreeHash32State: every state returned by createState must be released.
    if (xxh32Copy.Handle != IntPtr.Zero)
        xxh.FreeHash32State(xxh32Copy);

    if (xxh32State.Handle != IntPtr.Zero)
        xxh.FreeHash32State(xxh32State);
}

var canonical32 = default(XxhCanonical32);

// XXH32_canonicalFromHash -> Hash32ToCanonical: write a portable big-endian representation through an out parameter.
xxh.Hash32ToCanonical(out canonical32, xxh32);
Print("XXH32_canonicalFromHash / Hash32ToCanonical", CanonicalHex(ref canonical32));

// XXH32_hashFromCanonical -> Hash32FromCanonical: convert that portable representation back to the native integer.
Print("XXH32_hashFromCanonical / Hash32FromCanonical", Hex32(xxh.Hash32FromCanonical(in canonical32)));

Section("XXH64 one-shot, streaming, copy, canonical");

// XXH64 -> Hash64: one-shot 64-bit hashing, still using the managed span convenience overload.
var xxh64 = xxh.Hash64(input, Seed64);
Print("XXH64 / Hash64", Hex64(xxh64));

var xxh64State = xxh.CreateHash64State();
var xxh64Copy = xxh.CreateHash64State();
try
{
    RequireState(xxh64State.Handle, "XXH64_createState");
    RequireState(xxh64Copy.Handle, "XXH64_createState");

    // XXH64_reset, XXH64_update, XXH64_copyState, and XXH64_digest mirror the XXH32 streaming flow.
    RequireOk(xxh.ResetHash64(xxh64State, Seed64), "XXH64_reset");
    RequireOk(xxh.UpdateHash64(xxh64State, firstChunk), "XXH64_update");
    xxh.CopyHash64State(xxh64Copy, xxh64State);
    RequireOk(xxh.UpdateHash64(xxh64State, secondChunk), "XXH64_update");
    RequireOk(xxh.UpdateHash64(xxh64Copy, secondChunk), "XXH64_update");

    var stream64 = xxh.DigestHash64(xxh64State);
    var copied64 = xxh.DigestHash64(xxh64Copy);
    Print("XXH64_digest / DigestHash64", Hex64(stream64));
    Print("XXH64_copyState / CopyHash64State", HashMatch(stream64, copied64));
}
finally
{
    // XXH64_freeState -> FreeHash64State.
    if (xxh64Copy.Handle != IntPtr.Zero)
        xxh.FreeHash64State(xxh64Copy);

    if (xxh64State.Handle != IntPtr.Zero)
        xxh.FreeHash64State(xxh64State);
}

var canonical64 = default(XxhCanonical64);

// XXH64_canonicalFromHash and XXH64_hashFromCanonical are the 64-bit portable storage pair.
xxh.Hash64ToCanonical(out canonical64, xxh64);
Print("XXH64_canonicalFromHash / Hash64ToCanonical", CanonicalHex(ref canonical64));
Print("XXH64_hashFromCanonical / Hash64FromCanonical", Hex64(xxh.Hash64FromCanonical(in canonical64)));

Section("XXH3 secret generation");

Span<byte> customSecret = stackalloc byte[(int)Xxh.Xxh3SecretSizeMin];
Span<byte> seededSecret = stackalloc byte[(int)Xxh.Xxh3SecretDefaultSize];

// XXH3_generateSecret -> GenerateHash3Secret: expand arbitrary user material into a high-entropy XXH3 secret.
RequireOk(xxh.GenerateHash3Secret(customSecret, secretMaterial), "XXH3_generateSecret");
Print("XXH3_generateSecret / GenerateHash3Secret", $"{customSecret.Length} bytes, starts {HexBytes(customSecret[..8])}");

// XXH3_generateSecret_fromSeed -> GenerateHash3SecretFromSeed: precompute the same secret family used by _withSeed.
xxh.GenerateHash3SecretFromSeed(seededSecret, Seed64);
Print("XXH3_generateSecret_fromSeed / GenerateHash3SecretFromSeed", $"{seededSecret.Length} bytes, starts {HexBytes(seededSecret[..8])}");

Section("XXH3 64-bit one-shot variants");

// XXH3_64bits -> Hash3To64: unseeded XXH3 with a 64-bit result.
var xxh3To64 = xxh.Hash3To64(input);
Print("XXH3_64bits / Hash3To64", Hex64(xxh3To64));

// XXH3_64bits_withSeed -> Hash3To64(data, seed): seed-only variant.
var xxh3To64WithSeed = xxh.Hash3To64(input, Seed64);
Print("XXH3_64bits_withSeed / Hash3To64", Hex64(xxh3To64WithSeed));

// XXH3_64bits_withSecret -> Hash3To64(data, secret): custom secret variant.
var xxh3To64WithSecret = xxh.Hash3To64(input, customSecret);
Print("XXH3_64bits_withSecret / Hash3To64", Hex64(xxh3To64WithSecret));

// XXH3_64bits_withSecretandSeed -> Hash3To64(data, secret, seed): combines a long-lived secret with a seed.
var xxh3To64WithSecretAndSeed = xxh.Hash3To64(input, customSecret, Seed64);
Print("XXH3_64bits_withSecretandSeed / Hash3To64", Hex64(xxh3To64WithSecretAndSeed));

Section("XXH3 64-bit streaming variants");

var xxh3State = xxh.CreateHash3State();
var xxh3Copy = xxh.CreateHash3State();
try
{
    RequireState(xxh3State.Handle, "XXH3_createState");
    RequireState(xxh3Copy.Handle, "XXH3_createState");

    // XXH3_64bits_reset, update, digest: streaming form of XXH3_64bits.
    RequireOk(xxh.ResetHash3To64(xxh3State), "XXH3_64bits_reset");
    RequireOk(xxh.UpdateHash3To64(xxh3State, firstChunk), "XXH3_64bits_update");

    // XXH3_copyState -> CopyHash3State: XXH3 has one state type shared by 64-bit and 128-bit modes.
    xxh.CopyHash3State(xxh3Copy, xxh3State);
    RequireOk(xxh.UpdateHash3To64(xxh3State, secondChunk), "XXH3_64bits_update");
    RequireOk(xxh.UpdateHash3To64(xxh3Copy, secondChunk), "XXH3_64bits_update");
    Print("XXH3_64bits_digest / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));
    Print("XXH3_copyState / CopyHash3State", HashMatch(xxh.DigestHash3To64(xxh3State), xxh.DigestHash3To64(xxh3Copy)));

    // XXH3_64bits_reset_withSeed -> ResetHash3To64(state, seed).
    RequireOk(xxh.ResetHash3To64(xxh3State, Seed64), "XXH3_64bits_reset_withSeed");
    RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
    Print("XXH3_64bits_reset_withSeed / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));

    unsafe
    {
        fixed (byte* customSecretPtr = customSecret)
        {
            var customSecretAddress = (nint)customSecretPtr;
            var customSecretLength = (nuint)customSecret.Length;

            // XXH3_64bits_reset_withSecret -> ResetHash3To64(state, secret, size).
            // The secret is referenced until digest, so the pointer must remain valid for the full streaming session.
            RequireOk(xxh.ResetHash3To64(xxh3State, customSecretAddress, customSecretLength), "XXH3_64bits_reset_withSecret");
            RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
            Print("XXH3_64bits_reset_withSecret / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));

            // XXH3_64bits_reset_withSecretandSeed -> ResetHash3To64(state, secret, size, seed).
            RequireOk(xxh.ResetHash3To64(xxh3State, customSecretAddress, customSecretLength, Seed64), "XXH3_64bits_reset_withSecretandSeed");
            RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
            Print("XXH3_64bits_reset_withSecretandSeed / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));
        }
    }
}
finally
{
    // XXH3_freeState -> FreeHash3State.
    if (xxh3Copy.Handle != IntPtr.Zero)
        xxh.FreeHash3State(xxh3Copy);

    if (xxh3State.Handle != IntPtr.Zero)
        xxh.FreeHash3State(xxh3State);
}

Section("XXH3 128-bit one-shot variants");

// XXH3_128bits -> Hash3To128: unseeded XXH3 with a 128-bit result.
var xxh3To128 = XxHash128Native.Hash3To128(input);
Print("XXH3_128bits / Hash3To128", Hex128(xxh3To128.ToUInt128()));

// XXH3_128bits_withSeed -> Hash3To128(data, seed): seed-only variant.
var xxh3To128WithSeed = XxHash128Native.Hash3To128(input, Seed64);
Print("XXH3_128bits_withSeed / Hash3To128", Hex128(xxh3To128WithSeed.ToUInt128()));

// XXH3_128bits_withSecret -> Hash3To128(data, secret): custom secret variant.
var xxh3To128WithSecret = XxHash128Native.Hash3To128(input, customSecret);
Print("XXH3_128bits_withSecret / Hash3To128", Hex128(xxh3To128WithSecret.ToUInt128()));

// XXH3_128bits_withSecretandSeed -> Hash3To128(data, secret, seed): secret plus seed.
var xxh3To128WithSecretAndSeed = XxHash128Native.Hash3To128(input, customSecret, Seed64);
Print("XXH3_128bits_withSecretandSeed / Hash3To128", Hex128(xxh3To128WithSecretAndSeed.ToUInt128()));

// XXH128 -> Hash128: compatibility entry point for seeded 128-bit hashing.
var xxh128 = XxHash128Native.Hash128(input, Seed64);
Print("XXH128 / Hash128", Hex128(xxh128.ToUInt128()));

Section("XXH3 128-bit streaming variants");

var xxh3To128State = xxh.CreateHash3State();
try
{
    RequireState(xxh3To128State.Handle, "XXH3_createState");

    // XXH3_128bits_reset, update, digest: streaming form of XXH3_128bits.
    RequireOk(xxh.ResetHash3To128(xxh3To128State), "XXH3_128bits_reset");
    RequireOk(xxh.UpdateHash3To128(xxh3To128State, firstChunk), "XXH3_128bits_update");
    RequireOk(xxh.UpdateHash3To128(xxh3To128State, secondChunk), "XXH3_128bits_update");
    Print("XXH3_128bits_digest / DigestHash3To128", Hex128(XxHash128Native.DigestHash3To128(xxh3To128State).ToUInt128()));

    // XXH3_128bits_reset_withSeed -> ResetHash3To128(state, seed).
    RequireOk(xxh.ResetHash3To128(xxh3To128State, Seed64), "XXH3_128bits_reset_withSeed");
    RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
    Print(
        "XXH3_128bits_reset_withSeed / DigestHash3To128",
        Hex128(XxHash128Native.DigestHash3To128(xxh3To128State).ToUInt128()));

    unsafe
    {
        fixed (byte* customSecretPtr = customSecret)
        {
            var customSecretAddress = (nint)customSecretPtr;
            var customSecretLength = (nuint)customSecret.Length;

            // XXH3_128bits_reset_withSecret -> ResetHash3To128(state, secret, size).
            RequireOk(xxh.ResetHash3To128(xxh3To128State, customSecretAddress, customSecretLength), "XXH3_128bits_reset_withSecret");
            RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
            Print(
                "XXH3_128bits_reset_withSecret / DigestHash3To128",
                Hex128(XxHash128Native.DigestHash3To128(xxh3To128State).ToUInt128()));

            // XXH3_128bits_reset_withSecretandSeed -> ResetHash3To128(state, secret, size, seed).
            RequireOk(
                xxh.ResetHash3To128(xxh3To128State, customSecretAddress, customSecretLength, Seed64),
                "XXH3_128bits_reset_withSecretandSeed");
            RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
            Print(
                "XXH3_128bits_reset_withSecretandSeed / DigestHash3To128",
                Hex128(XxHash128Native.DigestHash3To128(xxh3To128State).ToUInt128()));
        }
    }
}
finally
{
    if (xxh3To128State.Handle != IntPtr.Zero)
        xxh.FreeHash3State(xxh3To128State);
}

Section("128-bit comparison and canonical form");

// XXH128_isEqual -> Hash128Equals: C returns non-zero for equality.
Print("XXH128_isEqual / Hash128Equals", XxHash128Native.Hash128Equals(xxh3To128, xxh3To128).ToString(CultureInfo.InvariantCulture));

// XXH128_cmp -> CompareHash128: C compares two XXH128_hash_t values through pointers.
Print("XXH128_cmp / CompareHash128", XxHash128Native.CompareHash128(xxh3To128, xxh128).ToString(CultureInfo.InvariantCulture));

var canonical128 = default(XxhCanonical128);

// XXH128_canonicalFromHash and XXH128_hashFromCanonical are the 128-bit portable storage pair.
XxHash128Native.Hash128ToCanonical(out canonical128, xxh3To128);
Print("XXH128_canonicalFromHash / Hash128ToCanonical", CanonicalHex(ref canonical128));
Print("XXH128_hashFromCanonical / Hash128FromCanonical", Hex128(XxHash128Native.Hash128FromCanonical(in canonical128).ToUInt128()));

/// <summary>Small formatting and guard helpers used by the top-level xxHash walkthrough.</summary>
file static class XxHashDemo
{
    /// <summary>Prints a demo section heading.</summary>
    /// <param name="title">The human-readable section title.</param>
    public static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }

    /// <summary>Prints one named xxHash result in a consistent layout.</summary>
    /// <param name="label">The C and managed API names being demonstrated.</param>
    /// <param name="value">The formatted result value.</param>
    public static void Print(string label, string value) =>
        Console.WriteLine($"{label,-58} {value}");

    /// <summary>Formats a 32-bit hash value as fixed-width hexadecimal.</summary>
    /// <param name="value">The hash value to format.</param>
    /// <returns>A fixed-width uppercase hexadecimal value.</returns>
    public static string Hex32(uint value) =>
        $"0x{value:X8}";

    /// <summary>Formats a 64-bit hash value as fixed-width hexadecimal.</summary>
    /// <param name="value">The hash value to format.</param>
    /// <returns>A fixed-width uppercase hexadecimal value.</returns>
    public static string Hex64(ulong value) =>
        $"0x{value:X16}";

    /// <summary>Formats a 128-bit hash value as fixed-width hexadecimal.</summary>
    /// <param name="value">The hash value to format.</param>
    /// <returns>A fixed-width uppercase hexadecimal value.</returns>
    public static string Hex128(UInt128 value) =>
        $"0x{value:X32}";

    /// <summary>Formats bytes as uppercase hexadecimal.</summary>
    /// <param name="bytes">The bytes to format.</param>
    /// <returns>The uppercase hexadecimal bytes.</returns>
    public static string HexBytes(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(bytes);

    /// <summary>Formats an unmanaged canonical xxHash structure as its raw big-endian byte sequence.</summary>
    /// <typeparam name="T">The canonical structure type.</typeparam>
    /// <param name="value">The canonical structure to inspect.</param>
    /// <returns>The uppercase hexadecimal bytes contained in the structure.</returns>
    public static string CanonicalHex<T>(ref T value)
        where T : unmanaged =>
        HexBytes(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)));

    /// <summary>Summarizes whether two hashes match.</summary>
    /// <typeparam name="T">The hash value type.</typeparam>
    /// <param name="left">The first hash value.</param>
    /// <param name="right">The second hash value.</param>
    /// <returns>A compact equality summary.</returns>
    public static string HashMatch<T>(T left, T right)
        where T : IEquatable<T> =>
        left.Equals(right) ? "matches" : "differs";

    /// <summary>Throws when an xxHash streaming or secret-generation call returns <see cref="XxhErrorCode.Error"/>.</summary>
    /// <param name="result">The native xxHash error code.</param>
    /// <param name="cName">The C API name associated with the call.</param>
    public static void RequireOk(XxhErrorCode result, string cName)
    {
        if (result != XxhErrorCode.Ok)
            throw new InvalidOperationException($"{cName} returned {result}.");
    }

    /// <summary>Throws when a native state allocation returns a null handle.</summary>
    /// <param name="handle">The handle returned by an xxHash createState call.</param>
    /// <param name="cName">The C API name associated with the allocation.</param>
    public static void RequireState(nint handle, string cName)
    {
        if (handle == 0)
            throw new InvalidOperationException($"{cName} returned a null state.");
    }

    /// <summary>Formats the encoded integer returned by <c>XXH_versionNumber</c>.</summary>
    /// <param name="versionNumber">The native xxHash version number encoded as major/minor/patch decimal groups.</param>
    /// <returns>A dotted version string.</returns>
    public static string VersionString(uint versionNumber) =>
        $"{versionNumber / 10000}.{versionNumber / 100 % 100}.{versionNumber % 100}";

}

/// <summary>An <c>XXH128_hash_t</c> value represented in the C layout used by xxHash.</summary>
[StructLayout(LayoutKind.Sequential)]
file readonly struct Xxh128Hash(ulong low64, ulong high64) : IEquatable<Xxh128Hash>
{
    /// <summary>The low 64 bits of the xxHash 128-bit value.</summary>
    public readonly ulong Low64 = low64;

    /// <summary>The high 64 bits of the xxHash 128-bit value.</summary>
    public readonly ulong High64 = high64;

    /// <summary>Converts the C-layout hash fields into a managed <see cref="UInt128"/> for display.</summary>
    /// <returns>The combined 128-bit value.</returns>
    public UInt128 ToUInt128() =>
        ((UInt128)High64 << 64) | Low64;

    /// <inheritdoc/>
    public bool Equals(Xxh128Hash other) =>
        Low64 == other.Low64 && High64 == other.High64;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Xxh128Hash other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(Low64, High64);
}

/// <summary>
/// Direct 128-bit xxHash calls that use the C <c>XXH128_hash_t</c> struct layout.
/// The packaged generated API names are shown in the walkthrough labels; this
/// bridge avoids returning <see cref="UInt128"/> directly from unmanaged code.
/// </summary>
file static class XxHash128Native
{
    /// <summary>Calls <c>XXH3_128bits</c>.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static unsafe Xxh128Hash Hash3To128(ReadOnlySpan<byte> data)
    {
        fixed (byte* dataPtr = data)
            return Xxh3To128((nint)dataPtr, (nuint)data.Length);
    }

    /// <summary>Calls <c>XXH3_128bits_withSeed</c>.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static unsafe Xxh128Hash Hash3To128(ReadOnlySpan<byte> data, ulong seed)
    {
        fixed (byte* dataPtr = data)
            return Xxh3To128WithSeed((nint)dataPtr, (nuint)data.Length, seed);
    }

    /// <summary>Calls <c>XXH3_128bits_withSecret</c>.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The high-entropy secret.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static unsafe Xxh128Hash Hash3To128(ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret)
    {
        fixed (byte* dataPtr = data)
        fixed (byte* secretPtr = secret)
            return Xxh3To128WithSecret((nint)dataPtr, (nuint)data.Length, (nint)secretPtr, (nuint)secret.Length);
    }

    /// <summary>Calls <c>XXH3_128bits_withSecretandSeed</c>.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The high-entropy secret.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static unsafe Xxh128Hash Hash3To128(ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret, ulong seed)
    {
        fixed (byte* dataPtr = data)
        fixed (byte* secretPtr = secret)
            return Xxh3To128WithSecretAndSeed((nint)dataPtr, (nuint)data.Length, (nint)secretPtr, (nuint)secret.Length, seed);
    }

    /// <summary>Calls <c>XXH128</c>.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static unsafe Xxh128Hash Hash128(ReadOnlySpan<byte> data, ulong seed)
    {
        fixed (byte* dataPtr = data)
            return Xxh128((nint)dataPtr, (nuint)data.Length, seed);
    }

    /// <summary>Calls <c>XXH3_128bits_digest</c>.</summary>
    /// <param name="state">The streaming XXH3 state to digest.</param>
    /// <returns>The 128-bit hash result.</returns>
    public static Xxh128Hash DigestHash3To128(Xxh3State state) =>
        Xxh3To128Digest(state.Handle);

    /// <summary>Calls <c>XXH128_isEqual</c>.</summary>
    /// <param name="left">The first hash value.</param>
    /// <param name="right">The second hash value.</param>
    /// <returns>The native non-zero equality result.</returns>
    public static int Hash128Equals(Xxh128Hash left, Xxh128Hash right) =>
        Xxh128IsEqual(left, right);

    /// <summary>Calls <c>XXH128_cmp</c>.</summary>
    /// <param name="left">The first hash value.</param>
    /// <param name="right">The second hash value.</param>
    /// <returns>The native comparison result.</returns>
    public static unsafe int CompareHash128(Xxh128Hash left, Xxh128Hash right) =>
        Xxh128Compare((nint)(&left), (nint)(&right));

    /// <summary>Calls <c>XXH128_canonicalFromHash</c>.</summary>
    /// <param name="canonical">The canonical big-endian output.</param>
    /// <param name="hash">The hash value to convert.</param>
    public static void Hash128ToCanonical(out XxhCanonical128 canonical, Xxh128Hash hash) =>
        Xxh128CanonicalFromHash(out canonical, hash);

    /// <summary>Calls <c>XXH128_hashFromCanonical</c>.</summary>
    /// <param name="canonical">The canonical big-endian representation.</param>
    /// <returns>The native-layout 128-bit hash value.</returns>
    public static Xxh128Hash Hash128FromCanonical(in XxhCanonical128 canonical) =>
        Xxh128HashFromCanonical(in canonical);

    /// <summary>Native import for <c>XXH3_128bits</c>.</summary>
    /// <param name="data">Native data pointer.</param>
    /// <param name="length">Native data length, in bytes.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH3_128bits", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh3To128(nint data, nuint length);

    /// <summary>Native import for <c>XXH3_128bits_withSeed</c>.</summary>
    /// <param name="data">Native data pointer.</param>
    /// <param name="length">Native data length, in bytes.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH3_128bits_withSeed", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh3To128WithSeed(nint data, nuint length, ulong seed);

    /// <summary>Native import for <c>XXH3_128bits_withSecret</c>.</summary>
    /// <param name="data">Native data pointer.</param>
    /// <param name="length">Native data length, in bytes.</param>
    /// <param name="secret">Native secret pointer.</param>
    /// <param name="secretSize">Native secret length, in bytes.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH3_128bits_withSecret", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh3To128WithSecret(nint data, nuint length, nint secret, nuint secretSize);

    /// <summary>Native import for <c>XXH3_128bits_withSecretandSeed</c>.</summary>
    /// <param name="data">Native data pointer.</param>
    /// <param name="length">Native data length, in bytes.</param>
    /// <param name="secret">Native secret pointer.</param>
    /// <param name="secretSize">Native secret length, in bytes.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH3_128bits_withSecretandSeed", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh3To128WithSecretAndSeed(nint data, nuint length, nint secret, nuint secretSize, ulong seed);

    /// <summary>Native import for <c>XXH128</c>.</summary>
    /// <param name="data">Native data pointer.</param>
    /// <param name="length">Native data length, in bytes.</param>
    /// <param name="seed">The 64-bit seed.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH128", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh128(nint data, nuint length, ulong seed);

    /// <summary>Native import for <c>XXH3_128bits_digest</c>.</summary>
    /// <param name="state">Native state pointer.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH3_128bits_digest", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh3To128Digest(nint state);

    /// <summary>Native import for <c>XXH128_isEqual</c>.</summary>
    /// <param name="left">The first hash value.</param>
    /// <param name="right">The second hash value.</param>
    /// <returns>The native non-zero equality result.</returns>
    [DllImport("xxhash", EntryPoint = "XXH128_isEqual", CallingConvention = CallingConvention.Cdecl)]
    private static extern int Xxh128IsEqual(Xxh128Hash left, Xxh128Hash right);

    /// <summary>Native import for <c>XXH128_cmp</c>.</summary>
    /// <param name="left">Native pointer to the first hash value.</param>
    /// <param name="right">Native pointer to the second hash value.</param>
    /// <returns>The native comparison result.</returns>
    [DllImport("xxhash", EntryPoint = "XXH128_cmp", CallingConvention = CallingConvention.Cdecl)]
    private static extern int Xxh128Compare(nint left, nint right);

    /// <summary>Native import for <c>XXH128_canonicalFromHash</c>.</summary>
    /// <param name="canonical">The canonical big-endian output.</param>
    /// <param name="hash">The hash value to convert.</param>
    [DllImport("xxhash", EntryPoint = "XXH128_canonicalFromHash", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Xxh128CanonicalFromHash(out XxhCanonical128 canonical, Xxh128Hash hash);

    /// <summary>Native import for <c>XXH128_hashFromCanonical</c>.</summary>
    /// <param name="canonical">The canonical big-endian representation.</param>
    /// <returns>The native 128-bit hash value.</returns>
    [DllImport("xxhash", EntryPoint = "XXH128_hashFromCanonical", CallingConvention = CallingConvention.Cdecl)]
    private static extern Xxh128Hash Xxh128HashFromCanonical(in XxhCanonical128 canonical);
}
