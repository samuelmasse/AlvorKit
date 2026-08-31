namespace AlvorKit;

/// <summary>Validates caller-owned FastNoise2 sampling buffers before crossing the native boundary.</summary>
internal static class FnSampleValidation
{
    public static void Grid2(Span<float> output, Vec2i count) =>
        RequireOutput(output, Required(count.X, count.Y));

    public static void Grid3(Span<float> output, Vec3i count) =>
        RequireOutput(output, Required(count.X, count.Y, count.Z));

    public static void Grid4(Span<float> output, Vec4i count) =>
        RequireOutput(output, Required(count.X, count.Y, count.Z, count.W));

    public static void Positions2(Span<float> output, ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        RequireOutput(output, output.Length);
        RequirePosition(x, output.Length, nameof(x));
        RequirePosition(y, output.Length, nameof(y));
        RequireNoOverlap(output, x, nameof(x));
        RequireNoOverlap(output, y, nameof(y));
    }

    public static void Positions3(
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z)
    {
        Positions2(output, x, y);
        RequirePosition(z, output.Length, nameof(z));
    }

    public static void Positions4(
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        ReadOnlySpan<float> w)
    {
        Positions3(output, x, y, z);
        RequirePosition(w, output.Length, nameof(w));
    }

    public static void MinMax(Span<float> output, Span<float> outputMinMax)
    {
        if (outputMinMax.Length < 2)
            throw new ArgumentException("The minimum/maximum output requires at least two floats.", nameof(outputMinMax));

        RequireNoOverlap(output, outputMinMax, nameof(outputMinMax));
    }

    public static void RequireNoOverlap(Span<float> output, ReadOnlySpan<float> input, string inputName)
    {
        if (output.Overlaps(input))
            throw new ArgumentException("FastNoise2 input and output spans must not overlap.", inputName);
    }

    private static int Required(int x, int y)
    {
        RequirePositive(x, nameof(x));
        RequirePositive(y, nameof(y));
        return ToLength((long)x * y);
    }

    private static int Required(int x, int y, int z)
    {
        RequirePositive(z, nameof(z));
        return ToLength((long)Required(x, y) * z);
    }

    private static int Required(int x, int y, int z, int w)
    {
        RequirePositive(w, nameof(w));
        return ToLength((long)Required(x, y, z) * w);
    }

    private static int ToLength(long value)
    {
        if (value > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), value, "The requested sample count exceeds Int32 capacity.");

        return (int)value;
    }

    private static void RequirePositive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(name, value, "Sample dimensions must be positive.");
    }

    private static void RequireOutput(Span<float> output, int required)
    {
        if (required <= 0)
            throw new ArgumentException("At least one output sample is required.", nameof(output));

        if (output.Length < required)
            throw new ArgumentException($"The output span requires at least {required} floats.", nameof(output));
    }

    private static void RequirePosition(ReadOnlySpan<float> position, int required, string name)
    {
        if (position.Length < required)
            throw new ArgumentException($"The position span requires at least {required} floats.", name);
    }
}
