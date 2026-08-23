namespace AlvorKit;

/// <summary>Provides scalar math helpers shared by vector component operations.</summary>
public static class ScalarMath
{
    /// <summary>Constrains a value using regular System vector minimum and maximum semantics.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Clamp<T>(T value, T min, T max)
        where T : INumber<T> =>
        T.Min(T.Max(value, min), max);

    /// <summary>Returns the absolute value using regular System vector semantics for IEEE floating-point values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Abs<T>(T value)
        where T : INumber<T> =>
        typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int) ||
        typeof(T) == typeof(long) || typeof(T) == typeof(nint) || typeof(T) == typeof(Int128)
            ? (value < T.Zero ? -value : value)
            : T.Abs(value);

    /// <summary>Linearly interpolates between two values without clamping amount.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Lerp<T>(T from, T to, T amount)
        where T : INumber<T> =>
        from + ((to - from) * amount);

    /// <summary>Returns the barycentric blend of three values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Barycentric<T>(T a, T b, T c, T u, T v)
        where T : INumber<T> =>
        a + ((b - a) * u) + ((c - a) * v);

    /// <summary>Constrains a value to the inclusive zero-to-one range.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Saturate<T>(T value)
        where T : INumber<T> =>
        Clamp(value, T.Zero, T.One);

    /// <summary>Returns the fractional part using floor-based modulo semantics.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FractionalPart<T>(T value)
        where T : IFloatingPoint<T> =>
        value - T.Floor(value);

    /// <summary>Returns floor-based modulo.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Modulo<T>(T left, T right)
        where T : IFloatingPoint<T> =>
        left - (right * T.Floor(left / right));

    /// <summary>Returns zero when value is below edge and one otherwise.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Step<T>(T edge, T value)
        where T : INumber<T> =>
        value < edge ? T.Zero : T.One;

    /// <summary>Smoothly interpolates from zero to one between edge values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SmoothStep<T>(T edge0, T edge1, T value)
        where T : IFloatingPoint<T>
    {
        var t = Saturate((value - edge0) / (edge1 - edge0));
        var two = T.One + T.One;
        return t * t * ((two + T.One) - (two * t));
    }

    /// <summary>Returns one divided by the square root of a value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T InverseSqrt<T>(T value)
        where T : IFloatingPointIeee754<T> =>
        T.One / T.Sqrt(value);

    /// <summary>Returns the least-significant set-bit index, or -1 for zero.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindLeastSignificantBit<T>(T value)
        where T : unmanaged, IBinaryInteger<T> =>
        value == T.Zero ? -1 : int.CreateChecked(T.TrailingZeroCount(value));

    /// <summary>Returns the most-significant set-bit index, or -1 for zero.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindMostSignificantBit<T>(T value)
        where T : unmanaged, IBinaryInteger<T>
    {
        if (value == T.Zero)
            return -1;

        return ((Unsafe.SizeOf<T>() * 8) - 1) - int.CreateChecked(T.LeadingZeroCount(value));
    }

    /// <summary>Selects one of two values according to a condition.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Select<T>(bool condition, T whenTrue, T whenFalse) =>
        condition ? whenTrue : whenFalse;
}
