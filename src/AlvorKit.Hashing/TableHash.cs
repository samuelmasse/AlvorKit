namespace AlvorKit;

/// <summary>Maps integer keys to indices in power-of-two hash tables.</summary>
public static class TableHash
{
    private const uint Key32Factor = 2_654_435_761u;
    private const ulong PairSalt = 0x9E3779B97F4A7C15UL;
    private const ulong PairFactor = 0xD6E8FEB86659FD93UL;
    private const ulong MixFirstFactor = 0xBF58476D1CE4E5B9UL;
    private const ulong MixSecondFactor = 0x94D049BB133111EBUL;

    /// <summary>Maps one signed 32-bit key using a capacity-minus-one mask.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(int key, int mask)
    {
        var bits = Unsafe.BitCast<int, uint>(key);
        var mixed = (uint)(((ulong)bits * Key32Factor) & uint.MaxValue);
        return (int)(mixed & (uint)mask);
    }

    /// <summary>Maps two ordered signed 32-bit key parts using a capacity-minus-one mask.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(int first, int second, int mask) => Mask(Mix(Pack(first, second)), mask);

    /// <summary>Maps one signed 64-bit key using a capacity-minus-one mask.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(long key, int mask) => Index(Unsafe.BitCast<long, ulong>(key), mask);

    /// <summary>Maps one unsigned 64-bit key using a capacity-minus-one mask.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(ulong key, int mask) => Mask(Mix(key), mask);

    /// <summary>Maps ordered unsigned 64-bit and signed 32-bit key parts using a capacity-minus-one mask.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(ulong first, int second, int mask)
    {
        var secondBits = Unsafe.BitCast<int, uint>(second);
        var secondMix = MultiplyLow(PairSalt + secondBits, PairFactor);
        return Mask(Mix(first ^ secondMix), mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Pack(int first, int second)
    {
        var firstBits = Unsafe.BitCast<int, uint>(first);
        var secondBits = Unsafe.BitCast<int, uint>(second);
        return ((ulong)firstBits << 32) | secondBits;
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
