namespace AlvorKit;

/// <summary>Provides scalar reference parameters for partial-original dispatch.</summary>
public sealed class PartialRefOutDispatchTarget
{
    /// <summary>Mutates one reference, writes one output, and returns their sum.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(ref int value, out int doubled)
    {
        value++;
        doubled = value * 2;
        return value + doubled;
    }
}
