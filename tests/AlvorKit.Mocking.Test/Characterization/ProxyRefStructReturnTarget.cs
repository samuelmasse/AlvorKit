namespace AlvorKit.Mocking.Test.Characterization;

public class ProxyRefStructReturnTarget
{
    private static readonly int[] values = [13, 21];

    public int Calls;

    public virtual ReadOnlySpan<int> Read()
    {
        Calls++;
        return values;
    }
}
