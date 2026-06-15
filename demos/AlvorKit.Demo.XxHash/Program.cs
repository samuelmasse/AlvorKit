using System.Globalization;
using AlvorKit.XxHash;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

// The same payload is used as one span and as two chunks so one-shot hashes can be compared with streaming hashes.
ReadOnlySpan<byte> input = "xxHash is built for very fast, non-cryptographic hashing."u8;
ReadOnlySpan<byte> firstChunk = input[..28];
ReadOnlySpan<byte> secondChunk = input[28..];
ReadOnlySpan<byte> secretMaterial = "application-specific secret material for the demo"u8;

const uint Seed32 = 0x9E37_79B1u;
const ulong Seed64 = 0x9E37_79B1_85EB_CA87ul;
const int VersionMajor = (int)XxhEnum.VersionMajor;
const int VersionMinor = (int)XxhEnum.VersionMinor;
const int VersionRelease = (int)XxhEnum.VersionRelease;
const int VersionNumber = (int)XxhEnum.VersionNumber;
const int Xxh3SecretSizeMin = (int)XxhEnum.Xxh3SecretSizeMin;
const int Xxh3SecretDefaultSize = (int)XxhEnum.Xxh3SecretDefaultSize;
const int Xxh3MidsizeMax = (int)XxhEnum.Xxh3MidsizeMax;
const int Xxh3InternalbufferSize = (int)XxhEnum.Xxh3InternalbufferSize;

Xxh xxh = new XxhBackend();

// Each output label names the C entry point first and the generated managed member second.
Section("Backend and constants");

var runtimeVersion = xxh.GetVersionNumber();
Console.WriteLine($"XXH_versionNumber / GetVersionNumber: {runtimeVersion} ({VersionString(runtimeVersion)})");
// Native constants are emitted onto an enum so they stay distinct from callable entry points.
Console.WriteLine($"XXH_VERSION_NUMBER / XxhEnum.VersionNumber: {VersionNumber} ({VersionMajor}.{VersionMinor}.{VersionRelease})");
Console.WriteLine($"XXH3_SECRET_SIZE_MIN / XxhEnum.Xxh3SecretSizeMin: {Xxh3SecretSizeMin} bytes");
Console.WriteLine($"XXH3_SECRET_DEFAULT_SIZE / XxhEnum.Xxh3SecretDefaultSize: {Xxh3SecretDefaultSize} bytes");
Console.WriteLine($"XXH3_MIDSIZE_MAX / XxhEnum.Xxh3MidsizeMax: {Xxh3MidsizeMax} bytes");
Console.WriteLine($"XXH3_INTERNALBUFFER_SIZE / XxhEnum.Xxh3InternalbufferSize: {Xxh3InternalbufferSize} bytes");

Section("XXH32 one-shot, streaming, copy, canonical");

// XXH32 is the one-shot path; reset/update/digest is the equivalent streaming path.
var xxh32 = xxh.Hash32(input, Seed32);
Print("XXH32 / Hash32", Hex32(xxh32));

var xxh32State = xxh.CreateHash32State();
var xxh32Copy = xxh.CreateHash32State();
RequireState(xxh32State.Handle, "XXH32_createState");
RequireState(xxh32Copy.Handle, "XXH32_createState");
RequireOk(xxh.ResetHash32(xxh32State, Seed32), "XXH32_reset");
RequireOk(xxh.UpdateHash32(xxh32State, firstChunk), "XXH32_update");
// Copying a partially updated state lets both streams finish from the same prefix.
xxh.CopyHash32State(xxh32Copy, xxh32State);
RequireOk(xxh.UpdateHash32(xxh32State, secondChunk), "XXH32_update");
RequireOk(xxh.UpdateHash32(xxh32Copy, secondChunk), "XXH32_update");

var stream32 = xxh.DigestHash32(xxh32State);
var copied32 = xxh.DigestHash32(xxh32Copy);
Print("XXH32_digest / DigestHash32", Hex32(stream32));
Print("XXH32_copyState / CopyHash32State", HashMatch(stream32, copied32));
xxh.FreeHash32State(xxh32Copy);
xxh.FreeHash32State(xxh32State);

var canonical32 = default(XxhCanonical32);
// Canonical forms are byte-stable storage formats; hashFromCanonical converts them back to native integers.
xxh.Hash32ToCanonical(out canonical32, xxh32);
Print("XXH32_canonicalFromHash / Hash32ToCanonical", CanonicalHex(ref canonical32));
Print("XXH32_hashFromCanonical / Hash32FromCanonical", Hex32(xxh.Hash32FromCanonical(in canonical32)));

Section("XXH64 one-shot, streaming, copy, canonical");

// XXH64 has the same lifecycle as XXH32, with a wider result and 64-bit seed.
var xxh64 = xxh.Hash64(input, Seed64);
Print("XXH64 / Hash64", Hex64(xxh64));

var xxh64State = xxh.CreateHash64State();
var xxh64Copy = xxh.CreateHash64State();
RequireState(xxh64State.Handle, "XXH64_createState");
RequireState(xxh64Copy.Handle, "XXH64_createState");
RequireOk(xxh.ResetHash64(xxh64State, Seed64), "XXH64_reset");
RequireOk(xxh.UpdateHash64(xxh64State, firstChunk), "XXH64_update");
xxh.CopyHash64State(xxh64Copy, xxh64State);
RequireOk(xxh.UpdateHash64(xxh64State, secondChunk), "XXH64_update");
RequireOk(xxh.UpdateHash64(xxh64Copy, secondChunk), "XXH64_update");

var stream64 = xxh.DigestHash64(xxh64State);
var copied64 = xxh.DigestHash64(xxh64Copy);
Print("XXH64_digest / DigestHash64", Hex64(stream64));
Print("XXH64_copyState / CopyHash64State", HashMatch(stream64, copied64));
xxh.FreeHash64State(xxh64Copy);
xxh.FreeHash64State(xxh64State);

var canonical64 = default(XxhCanonical64);
xxh.Hash64ToCanonical(out canonical64, xxh64);
Print("XXH64_canonicalFromHash / Hash64ToCanonical", CanonicalHex(ref canonical64));
Print("XXH64_hashFromCanonical / Hash64FromCanonical", Hex64(xxh.Hash64FromCanonical(in canonical64)));

Section("XXH3 secret generation");

// XxhSecret owns native memory because streaming reset-with-secret APIs retain the secret pointer.
using XxhSecret customSecret = new(Xxh3SecretSizeMin);
Span<byte> seededSecret = stackalloc byte[Xxh3SecretDefaultSize];

// generateSecret expands arbitrary user material; generateSecret_fromSeed produces the seeded secret family.
RequireOk(xxh.GenerateHash3Secret(customSecret.Bytes, secretMaterial), "XXH3_generateSecret");
Print("XXH3_generateSecret / GenerateHash3Secret", $"{customSecret.Size} bytes, starts {HexBytes(customSecret.Bytes[..8])}");
xxh.GenerateHash3SecretFromSeed(seededSecret, Seed64);
Print("XXH3_generateSecret_fromSeed / GenerateHash3SecretFromSeed", $"{seededSecret.Length} bytes, starts {HexBytes(seededSecret[..8])}");

Section("XXH3 64-bit one-shot variants");

// XXH3 exposes default, seed-only, secret-only, and secret-plus-seed one-shot entry points.
var xxh3To64 = xxh.Hash3To64(input);
var xxh3To64WithSeed = xxh.Hash3To64(input, Seed64);
var xxh3To64WithSecret = xxh.Hash3To64(input, customSecret.Bytes);
var xxh3To64WithSecretAndSeed = xxh.Hash3To64(input, customSecret.Bytes, Seed64);
Print("XXH3_64bits / Hash3To64", Hex64(xxh3To64));
Print("XXH3_64bits_withSeed / Hash3To64", Hex64(xxh3To64WithSeed));
Print("XXH3_64bits_withSecret / Hash3To64", Hex64(xxh3To64WithSecret));
Print("XXH3_64bits_withSecretandSeed / Hash3To64", Hex64(xxh3To64WithSecretAndSeed));

Section("XXH3 64-bit streaming variants");

var xxh3State = xxh.CreateHash3State();
var xxh3Copy = xxh.CreateHash3State();
RequireState(xxh3State.Handle, "XXH3_createState");
RequireState(xxh3Copy.Handle, "XXH3_createState");
// XXH3 uses one state type; reset chooses whether the digest will be 64-bit or 128-bit.
RequireOk(xxh.ResetHash3To64(xxh3State), "XXH3_64bits_reset");
RequireOk(xxh.UpdateHash3To64(xxh3State, firstChunk), "XXH3_64bits_update");
xxh.CopyHash3State(xxh3Copy, xxh3State);
RequireOk(xxh.UpdateHash3To64(xxh3State, secondChunk), "XXH3_64bits_update");
RequireOk(xxh.UpdateHash3To64(xxh3Copy, secondChunk), "XXH3_64bits_update");

var stream3To64 = xxh.DigestHash3To64(xxh3State);
var copied3To64 = xxh.DigestHash3To64(xxh3Copy);
Print("XXH3_64bits_digest / DigestHash3To64", Hex64(stream3To64));
Print("XXH3_copyState / CopyHash3State", HashMatch(stream3To64, copied3To64));

RequireOk(xxh.ResetHash3To64(xxh3State, Seed64), "XXH3_64bits_reset_withSeed");
RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
Print("XXH3_64bits_reset_withSeed / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));

// The managed overload passes XxhSecret's owned native pointer and size to XXH3_64bits_reset_withSecret.
RequireOk(xxh.ResetHash3To64(xxh3State, customSecret), "XXH3_64bits_reset_withSecret");
RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
Print("XXH3_64bits_reset_withSecret / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));

RequireOk(xxh.ResetHash3To64(xxh3State, customSecret, Seed64), "XXH3_64bits_reset_withSecretandSeed");
RequireOk(xxh.UpdateHash3To64(xxh3State, input), "XXH3_64bits_update");
Print("XXH3_64bits_reset_withSecretandSeed / DigestHash3To64", Hex64(xxh.DigestHash3To64(xxh3State)));
xxh.FreeHash3State(xxh3Copy);
xxh.FreeHash3State(xxh3State);

Section("XXH3 128-bit one-shot variants");

// The 128-bit return value is projected as UInt128 while the backend still uses xxHash's native struct layout.
var xxh3To128 = xxh.Hash3To128(input);
var xxh3To128WithSeed = xxh.Hash3To128(input, Seed64);
var xxh3To128WithSecret = xxh.Hash3To128(input, customSecret.Bytes);
var xxh3To128WithSecretAndSeed = xxh.Hash3To128(input, customSecret.Bytes, Seed64);
var xxh128 = xxh.Hash128(input, Seed64);
Print("XXH3_128bits / Hash3To128", Hex128(xxh3To128));
Print("XXH3_128bits_withSeed / Hash3To128", Hex128(xxh3To128WithSeed));
Print("XXH3_128bits_withSecret / Hash3To128", Hex128(xxh3To128WithSecret));
Print("XXH3_128bits_withSecretandSeed / Hash3To128", Hex128(xxh3To128WithSecretAndSeed));
Print("XXH128 / Hash128", Hex128(xxh128));

Section("XXH3 128-bit streaming variants");

var xxh3To128State = xxh.CreateHash3State();
RequireState(xxh3To128State.Handle, "XXH3_createState");
// Resetting the shared XXH3 state to 128-bit mode makes digest return the UInt128 projection.
RequireOk(xxh.ResetHash3To128(xxh3To128State), "XXH3_128bits_reset");
RequireOk(xxh.UpdateHash3To128(xxh3To128State, firstChunk), "XXH3_128bits_update");
RequireOk(xxh.UpdateHash3To128(xxh3To128State, secondChunk), "XXH3_128bits_update");
Print("XXH3_128bits_digest / DigestHash3To128", Hex128(xxh.DigestHash3To128(xxh3To128State)));

RequireOk(xxh.ResetHash3To128(xxh3To128State, Seed64), "XXH3_128bits_reset_withSeed");
RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
Print("XXH3_128bits_reset_withSeed / DigestHash3To128", Hex128(xxh.DigestHash3To128(xxh3To128State)));

RequireOk(xxh.ResetHash3To128(xxh3To128State, customSecret), "XXH3_128bits_reset_withSecret");
RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
Print("XXH3_128bits_reset_withSecret / DigestHash3To128", Hex128(xxh.DigestHash3To128(xxh3To128State)));

RequireOk(xxh.ResetHash3To128(xxh3To128State, customSecret, Seed64), "XXH3_128bits_reset_withSecretandSeed");
RequireOk(xxh.UpdateHash3To128(xxh3To128State, input), "XXH3_128bits_update");
Print("XXH3_128bits_reset_withSecretandSeed / DigestHash3To128", Hex128(xxh.DigestHash3To128(xxh3To128State)));
xxh.FreeHash3State(xxh3To128State);

Section("128-bit comparison and canonical form");

// Equality takes values directly; cmp is a managed overload over the C comparator's pointer-shaped contract.
Print("XXH128_isEqual / Hash128Equals", xxh.Hash128Equals(xxh3To128, xxh3To128).ToString());
Print("XXH128_cmp / CompareHash128", xxh.CompareHash128(xxh3To128, xxh128).ToString(CultureInfo.InvariantCulture));

var canonical128 = default(XxhCanonical128);
// Canonical 128-bit values are also portable big-endian byte sequences.
xxh.Hash128ToCanonical(out canonical128, xxh3To128);
Print("XXH128_canonicalFromHash / Hash128ToCanonical", CanonicalHex(ref canonical128));
Print("XXH128_hashFromCanonical / Hash128FromCanonical", Hex128(xxh.Hash128FromCanonical(in canonical128)));

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

static void Print(string label, string value) =>
    Console.WriteLine($"{label,-58} {value}");

static string Hex32(uint value) =>
    $"0x{value:X8}";

static string Hex64(ulong value) =>
    $"0x{value:X16}";

static string Hex128(UInt128 value) =>
    $"0x{value:X32}";

static string HexBytes(ReadOnlySpan<byte> bytes) =>
    Convert.ToHexString(bytes);

static string CanonicalHex<T>(ref T value)
    where T : unmanaged =>
    HexBytes(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)));

static string HashMatch<T>(T left, T right)
    where T : IEquatable<T> =>
    left.Equals(right) ? "matches" : "differs";

static void RequireOk(XxhErrorCode result, string cName)
{
    if (result != XxhErrorCode.Ok)
        throw new InvalidOperationException($"{cName} returned {result}.");
}

static void RequireState(nint handle, string cName)
{
    if (handle == 0)
        throw new InvalidOperationException($"{cName} returned a null state.");
}

static string VersionString(uint versionNumber) =>
    $"{versionNumber / 10000}.{versionNumber / 100 % 100}.{versionNumber % 100}";
