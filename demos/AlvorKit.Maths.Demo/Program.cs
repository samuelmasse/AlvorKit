Console.WriteLine("AlvorKit.Maths demo");

Section("Floating vectors");

var velocity = new Vec3(3f, 4f, 0f);
var direction = velocity.Normalized;
var displacement = direction * 12f * (1f / 60f);

Print("velocity", Format3(velocity));
Print("length", velocity.Length.ToString("0.###", CultureInfo.InvariantCulture));
Print("normalized", Format3(direction));
Print("one 60 Hz step at speed 12", Format3(displacement));

ExpectClose(velocity.Length, 5f, "Vec3 length should use all three components.");
ExpectClose3(direction, new Vec3(0.6f, 0.8f, 0f), "Vec3.Normalized should keep direction and unit length.");
ExpectClose3(displacement, new Vec3(0.12f, 0.16f, 0f), "Scalar and vector products should compose naturally.");

Section("Swizzles and composition");

var color = new Vec4(0.10f, 0.20f, 0.40f, 1f);
var redGreen = color.RG;
var sample = new Vec3(1f, 2f, 3f)
{
    YX = new Vec2(8f, 9f),
};
var swappedBeforeColorWrite = sample.XY;
sample.BGR = new Vec3(4f, 5f, 6f);

Print("rgba color", Format4(color));
Print("color.RG", Format2(redGreen));
Print("after YX write", Format2(swappedBeforeColorWrite));
Print("after BGR write", Format3(sample.RGB));
Print("repeated swizzle", Format4(sample.RRGB));

ExpectClose2(redGreen, new Vec2(0.10f, 0.20f), "Vec4 color aliases should read the same component storage.");
ExpectClose2(swappedBeforeColorWrite, new Vec2(9f, 8f), "Writable swizzles should assign each distinct component once.");
ExpectClose3(sample.RGB, new Vec3(6f, 5f, 4f), "RGB aliases should match XYZ storage.");
ExpectClose4(sample.RRGB, new Vec4(6f, 6f, 5f, 4f), "Read swizzles may repeat components.");

Section("Masks and selection");

var requestedPosition = new Vec3(12f, -3f, 5f);
var worldMin = Vec3.Zero;
var worldMax = new Vec3(10f);
var clampedPosition = Vec3.Clamp(requestedPosition, worldMin, worldMax);
var outsideWorld = requestedPosition < worldMin | requestedPosition > worldMax;
var acceptedPosition = outsideWorld.Select(clampedPosition, requestedPosition);

Print("requested", Format3(requestedPosition));
Print("outside mask", Format3b(outsideWorld));
Print("clamped", Format3(acceptedPosition));
Print("mask.Any", outsideWorld.Any.ToString());

Expect(outsideWorld == new Vec3b(true, true, false), "Relational operators should return component masks.");
ExpectClose3(acceptedPosition, new Vec3(10f, 0f, 5f), "Mask selection should pick only the corrected components.");

Section("Integer grids");

var tile = new Vec3i(27, 18, 7);
var chunk = tile >> 3;
var localTile = tile & 0b111;
var powerOfTwo = Vec3i.IsPowerOfTwo(new Vec3i(16, 18, 32));

Print("tile", Format3i(tile));
Print("tile >> 3", Format3i(chunk));
Print("tile & 7", Format3i(localTile));
Print("power-of-two mask", Format3b(powerOfTwo));

Expect(chunk == new Vec3i(3, 2, 0), "Integer vectors should shift each component.");
Expect(localTile == new Vec3i(3, 2, 7), "Bitwise operators should apply component-wise.");
Expect(powerOfTwo == new Vec3b(true, false, true), "Integer helper masks should preserve per-component answers.");

Section("Span and System.Numerics interop");

System.Numerics.Vector3 systemVector = direction;
Vec3 roundTrip = systemVector;
Span<float> packed = stackalloc float[3];
roundTrip.CopyTo(packed);

Print("System.Numerics", systemVector.ToString());
Print("packed span", $"[{packed[0]:0.###}, {packed[1]:0.###}, {packed[2]:0.###}]");

ExpectClose3(roundTrip, direction, "Vec3 should round-trip through System.Numerics.Vector3.");
ExpectClose3(new Vec3(packed[0], packed[1], packed[2]), direction, "CopyTo should preserve component order.");

Section("Scalar helpers");

var wrapped = ScalarMath.Modulo(-1.25f, 1f);
var eased = ScalarMath.SmoothStep(0f, 1f, 0.35f);
var bits = ScalarMath.BitCount(0b1011_0001);

Print("Modulo(-1.25, 1)", wrapped.ToString("0.###", CultureInfo.InvariantCulture));
Print("SmoothStep(0, 1, .35)", eased.ToString("0.###", CultureInfo.InvariantCulture));
Print("BitCount(0b10110001)", bits.ToString(CultureInfo.InvariantCulture));

ExpectClose(wrapped, 0.75f, "ScalarMath.Modulo should return GLSL-style positive remainders.");
ExpectClose(eased, 0.28175f, "ScalarMath.SmoothStep should use the cubic Hermite curve.");
Expect(bits == 4, "ScalarMath.BitCount should inspect the scalar bit pattern.");

Console.WriteLine();
Console.WriteLine("All maths checks passed.");

// Writes a section heading for the demo narrative.
static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

// Writes one aligned demo observation.
static void Print(string label, string value) =>
    Console.WriteLine($"{label,-30} {value}");

// Formats a two-component floating vector with invariant tuple-style text.
static string Format2(Vec2 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a three-component floating vector with invariant tuple-style text.
static string Format3(Vec3 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats a four-component floating vector with invariant tuple-style text.
static string Format4(Vec4 value) =>
    value.ToString("0.###", CultureInfo.InvariantCulture);

// Formats an integer vector with invariant tuple-style text.
static string Format3i(Vec3i value) =>
    value.ToString(CultureInfo.InvariantCulture);

// Formats a Boolean mask with tuple-style text.
static string Format3b(Vec3b value) =>
    value.ToString();

// Fails fast when a demo invariant no longer holds.
static void Expect(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

// Compares two floating scalars with a small tolerance for cross-platform math.
static void ExpectClose(float actual, float expected, string message) =>
    Expect(MathF.Abs(actual - expected) <= 0.00001f, message);

// Compares two floating vectors with a small tolerance for cross-platform math.
static void ExpectClose2(Vec2 actual, Vec2 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
}

// Compares three-component floating vectors with a small tolerance for cross-platform math.
static void ExpectClose3(Vec3 actual, Vec3 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
    ExpectClose(actual.Z, expected.Z, message);
}

// Compares four-component floating vectors with a small tolerance for cross-platform math.
static void ExpectClose4(Vec4 actual, Vec4 expected, string message)
{
    ExpectClose(actual.X, expected.X, message);
    ExpectClose(actual.Y, expected.Y, message);
    ExpectClose(actual.Z, expected.Z, message);
    ExpectClose(actual.W, expected.W, message);
}
