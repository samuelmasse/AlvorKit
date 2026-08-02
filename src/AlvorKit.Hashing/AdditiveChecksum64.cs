namespace AlvorKit.Hashing;

/// <summary>Accumulates primitive values as a modulo-2^64 additive checksum.</summary>
public struct AdditiveChecksum64
{
    private ulong value;

    /// <summary>Gets the checksum accumulated from all values added so far.</summary>
    public readonly ulong Value => value;

    /// <summary>Adds zero for <see langword="false"/> or one for <see langword="true"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(bool input) => Add(input ? 1UL : 0UL);

    /// <summary>Adds the signed value modulo 2^64.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int input) => Add((long)input);

    /// <summary>Adds the unsigned value modulo 2^64.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(uint input) => Add((ulong)input);

    /// <summary>Adds the signed value modulo 2^64.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(long input) => Add(Unsafe.BitCast<long, ulong>(input));

    /// <summary>Adds the unsigned value modulo 2^64.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ulong input) => value = (ulong)(((UInt128)value + input) & ulong.MaxValue);
}
