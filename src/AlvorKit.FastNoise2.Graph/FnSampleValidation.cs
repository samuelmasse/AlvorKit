namespace AlvorKit;

/// <summary>Validates caller-owned FastNoise2 sampling buffers before crossing the native boundary.</summary>
internal static class FnSampleValidation
{
    /// <summary>Validates a two-dimensional grid output and positive count product.</summary>
    public static void Grid2(Span<float> output, Vec2i count, string countName) =>
        RequireOutput(output, Required(count.X, count.Y, countName));

    /// <summary>Validates a three-dimensional grid output and positive count product.</summary>
    public static void Grid3(Span<float> output, Vec3i count) =>
        RequireOutput(output, Required(count.X, count.Y, count.Z, nameof(count)));

    /// <summary>Validates a four-dimensional grid output and positive count product.</summary>
    public static void Grid4(Span<float> output, Vec4i count) =>
        RequireOutput(output, Required(count.X, count.Y, count.Z, count.W, nameof(count)));

    /// <summary>Validates two-dimensional structure-of-arrays positions and disjoint output.</summary>
    public static void Positions2(Span<float> output, ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        RequireOutput(output, output.Length);
        RequirePosition(x, output.Length, nameof(x));
        RequirePosition(y, output.Length, nameof(y));
        RequireNoOverlap(output, x, nameof(x));
        RequireNoOverlap(output, y, nameof(y));
    }

    /// <summary>Validates three-dimensional structure-of-arrays positions and disjoint output.</summary>
    public static void Positions3(
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z)
    {
        Positions2(output, x, y);
        RequirePosition(z, output.Length, nameof(z));
        RequireNoOverlap(output, z, nameof(z));
    }

    /// <summary>Validates four-dimensional structure-of-arrays positions and disjoint output.</summary>
    public static void Positions4(
        Span<float> output,
        ReadOnlySpan<float> x,
        ReadOnlySpan<float> y,
        ReadOnlySpan<float> z,
        ReadOnlySpan<float> w)
    {
        Positions3(output, x, y, z);
        RequirePosition(w, output.Length, nameof(w));
        RequireNoOverlap(output, w, nameof(w));
    }

    /// <summary>Validates a two-value range destination that does not overlap generated output.</summary>
    public static void MinMax(Span<float> output, Span<float> outputMinMax)
    {
        if (outputMinMax.Length < 2)
            throw new ArgumentException("The minimum/maximum output requires at least two floats.", nameof(outputMinMax));

        RequireNoOverlap(output, outputMinMax, nameof(outputMinMax));
    }

    /// <summary>Rejects an input span that shares storage with a native output span.</summary>
    public static void RequireNoOverlap(Span<float> output, ReadOnlySpan<float> input, string inputName)
    {
        if (output.Overlaps(input))
            throw new ArgumentException("FastNoise2 input and output spans must not overlap.", inputName);
    }

    /// <summary>Computes a positive two-dimensional sample count.</summary>
    private static int Required(int x, int y, string name)
    {
        RequirePositive(x, name);
        RequirePositive(y, name);
        return ToLength((long)x * y, name);
    }

    /// <summary>Computes a positive three-dimensional sample count.</summary>
    private static int Required(int x, int y, int z, string name)
    {
        RequirePositive(z, name);
        return ToLength((long)Required(x, y, name) * z, name);
    }

    /// <summary>Computes a positive four-dimensional sample count.</summary>
    private static int Required(int x, int y, int z, int w, string name)
    {
        RequirePositive(w, name);
        return ToLength((long)Required(x, y, z, name) * w, name);
    }

    /// <summary>Converts a sample count only when it fits the binding's Int32 count contract.</summary>
    private static int ToLength(long value, string name)
    {
        if (value > int.MaxValue)
            throw new ArgumentOutOfRangeException(name, value, "The requested sample count exceeds Int32 capacity.");

        return (int)value;
    }

    /// <summary>Rejects zero and negative grid dimensions.</summary>
    private static void RequirePositive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(name, value, "Sample dimensions must be positive.");
    }

    /// <summary>Rejects empty or undersized output storage.</summary>
    private static void RequireOutput(Span<float> output, int required)
    {
        if (required <= 0)
            throw new ArgumentException("At least one output sample is required.", nameof(output));

        if (output.Length < required)
            throw new ArgumentException($"The output span requires at least {required} floats.", nameof(output));
    }

    /// <summary>Rejects a coordinate span shorter than the requested position count.</summary>
    private static void RequirePosition(ReadOnlySpan<float> position, int required, string name)
    {
        if (position.Length < required)
            throw new ArgumentException($"The position span requires at least {required} floats.", name);
    }
}
