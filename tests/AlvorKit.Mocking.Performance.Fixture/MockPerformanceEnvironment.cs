namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Records runtime and machine properties needed to interpret one fixture run.</summary>
internal sealed record MockPerformanceEnvironment(
    DateTimeOffset RecordedAtUtc,
    string OperatingSystem,
    string Processor,
    string ProcessArchitecture,
    int LogicalProcessorCount,
    string Framework,
    string BuildConfiguration,
    string GarbageCollector,
    long StopwatchFrequency)
{
    /// <summary>Captures the current process environment.</summary>
    internal static MockPerformanceEnvironment Capture() =>
        new(
            DateTimeOffset.UtcNow,
            RuntimeInformation.OSDescription,
            Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")
                ?? "Unavailable",
            RuntimeInformation.ProcessArchitecture.ToString(),
            Environment.ProcessorCount,
            RuntimeInformation.FrameworkDescription,
#if DEBUG
            "Debug",
#else
            "Release",
#endif
            GCSettings.IsServerGC ? "Server" : "Workstation",
            Stopwatch.Frequency);
}
