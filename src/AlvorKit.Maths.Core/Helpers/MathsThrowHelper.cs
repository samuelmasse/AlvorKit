namespace AlvorKit.Maths;

/// <summary>Provides cold-path exception helpers for generated maths types.</summary>
internal static class MathsThrowHelper
{
    /// <summary>Throws when a vector, quaternion, or matrix component index is outside its valid range.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowComponentIndexOutOfRange(int index, int maxIndex) =>
        throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {maxIndex}.");

    /// <summary>Throws when a box component index is outside its valid range.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowBoxComponentIndexOutOfRange(int index, int componentCount) =>
        throw new IndexOutOfRangeException($"Component index {index} is outside 0..{componentCount - 1}.");

    /// <summary>Throws when a matrix column index is outside its valid range.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowColumnIndexOutOfRange(int column, int maxColumnIndex) =>
        throw new ArgumentOutOfRangeException(nameof(column), column, $"Index must be between 0 and {maxColumnIndex}.");

    /// <summary>Throws when a source span cannot provide every component.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowSourceSpanTooShort(int componentCount) =>
        throw new ArgumentException($"Span must contain at least {componentCount} components.", "values");

    /// <summary>Throws when a destination span cannot receive every component.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowDestinationSpanTooShort(int componentCount) =>
        throw new ArgumentException($"Span must contain at least {componentCount} components.", "destination");
}
