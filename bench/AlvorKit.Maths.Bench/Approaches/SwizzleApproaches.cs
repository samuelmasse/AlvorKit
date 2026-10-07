namespace AlvorKit;

internal static class SwizzleApproaches
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vec3 IntrinsicXzy(Vec3 value)
    {
        var packed = Vector128.Create(value.X, value.Y, value.Z, 0f);
        var shuffled = Sse.Shuffle(packed, packed, 0b11_01_10_00);
        return new(shuffled[0], shuffled[1], shuffled[2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vec3i IntrinsicXzy(Vec3i value)
    {
        var shuffled = Sse2.Shuffle(Vector128.Create(value.X, value.Y, value.Z, 0), 0b11_01_10_00);
        return new(shuffled[0], shuffled[1], shuffled[2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vec4 IntrinsicReverse(Vec4 value)
    {
        var packed = Unsafe.BitCast<Vec4, Vector128<float>>(value);
        return Unsafe.BitCast<Vector128<float>, Vec4>(Sse.Shuffle(packed, packed, 0b00_01_10_11));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vec4i IntrinsicReverse(Vec4i value)
    {
        var packed = Unsafe.BitCast<Vec4i, Vector128<int>>(value);
        return Unsafe.BitCast<Vector128<int>, Vec4i>(Sse2.Shuffle(packed, 0b00_01_10_11));
    }
}
