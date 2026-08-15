namespace AlvorKit;

/// <summary>Writes concise human-readable and structured performance evidence.</summary>
internal static class InterceptionPerformanceOutput
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>Prints the recorded operations followed by one compact JSON evidence line.</summary>
    internal static void Print(InterceptionPerformanceReport report)
    {
        Print(report.ColdInstall);
        Print(report.WarmDirect);
        Print(report.WarmInertRoute);
        Print(report.WarmActiveExact);
        Print(report.WarmSwappedExact);
        Console.WriteLine(
            "interception-perf handler-swap: " +
            $"request {report.HandlerSwap.BeforeLastRequestId} unchanged, " +
            $"{report.HandlerSwap.AfterActivePatches} active patch");
        Print(report.ColdRemove);
        Console.WriteLine(
            $"INTERCEPTION_PERFORMANCE_JSON {JsonSerializer.Serialize(report, JsonOptions)}");
    }

    private static void Print(ColdInterceptionMeasurement measurement) =>
        Console.WriteLine(
            $"interception-perf {measurement.Name}: " +
            $"{measurement.WallMilliseconds:F3} ms wall, " +
            $"{measurement.NativeMilliseconds:F3} ms native");

    private static void Print(WarmInterceptionMeasurement measurement) =>
        Console.WriteLine(
            $"interception-perf {measurement.Name}: " +
            $"{measurement.MedianNanosecondsPerCall:F2} ns/call median, " +
            $"{measurement.AllocatedBytes} B allocated");
}
