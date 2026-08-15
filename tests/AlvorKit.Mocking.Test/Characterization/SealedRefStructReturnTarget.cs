namespace AlvorKit;

public sealed class SealedRefStructReturnTarget
{
    private static readonly int[] values = [34, 55];

    public int Calls;

    public ReadOnlySpan<int> Read()
    {
        Calls++;
        return values;
    }
}
