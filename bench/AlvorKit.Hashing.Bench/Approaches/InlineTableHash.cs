namespace AlvorKit;

/// <summary>Same approved TableHash algorithm, local to the benchmark assembly.</summary>
internal static class InlineTableHash
{
    private const uint Key32Factor = 2_654_435_761u;
    private const ulong PairSalt = 0x9E3779B97F4A7C15UL;
    private const ulong PairFactor = 0xD6E8FEB86659FD93UL;
    private const ulong MixFirstFactor = 0xBF58476D1CE4E5B9UL;
    private const ulong MixSecondFactor = 0x94D049BB133111EBUL;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Index(int key, int mask)
    {
        var bits = Unsafe.BitCast<int, uint>(key);
        var mixed = (uint)(((ulong)bits * Key32Factor) & uint.MaxValue);
        return (int)(mixed & (uint)mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Index(int first, int second, int mask)
    {
        var firstBits = Unsafe.BitCast<int, uint>(first);
        var secondBits = Unsafe.BitCast<int, uint>(second);
        return Mask(Mix(((ulong)firstBits << 32) | secondBits), mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Index(ulong key, int mask) => Mask(Mix(key), mask);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Index(ulong first, int second, int mask)
    {
        var secondBits = Unsafe.BitCast<int, uint>(second);
        return Mask(Mix(first ^ MultiplyLow(PairSalt + secondBits, PairFactor)), mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ulong value)
    {
        value = MultiplyLow(value ^ (value >> 30), MixFirstFactor);
        value = MultiplyLow(value ^ (value >> 27), MixSecondFactor);
        return value ^ (value >> 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MultiplyLow(ulong left, ulong right) => (ulong)(((UInt128)left * right) & ulong.MaxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Mask(ulong value, int mask) => (int)(value & (uint)mask);
}
