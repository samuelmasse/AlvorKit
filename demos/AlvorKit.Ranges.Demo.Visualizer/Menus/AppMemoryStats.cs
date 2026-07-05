namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Computes aggregate readouts over the current allocator snapshot.</summary>
[App]
public class AppMemoryStats(AppSession session)
{
    /// <summary>Gets the used-to-backing-size ratio.</summary>
    public double UsedRatio() => Ratio(session.Runner.Current.Used, session.Runner.Current.Size);

    /// <summary>Gets how much free space is unusable because it is split across small blocks.</summary>
    public double ExternalFragmentationRatio()
    {
        var freeBytes = FreeBytes();
        return freeBytes <= 0 ? 0 : 1.0 - Ratio(LargestFreeSpan(), freeBytes);
    }

    /// <summary>Gets the size of the largest contiguous free span.</summary>
    public long LargestFreeSpan()
    {
        var largest = 0L;
        var spans = session.Runner.Current.FreeSpans;
        for (var i = 0; i < spans.Length; i++)
            largest = Math.Max(largest, spans[i].Size);

        return largest;
    }

    private long FreeBytes()
    {
        var total = 0L;
        var spans = session.Runner.Current.FreeSpans;
        for (var i = 0; i < spans.Length; i++)
            total += spans[i].Size;

        return total;
    }

    private static double Ratio(double numerator, double denominator) => denominator <= 0 ? 0 : numerator / denominator;
}
