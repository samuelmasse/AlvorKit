namespace AlvorKit.Script.TestInterception;

/// <summary>Reports whether a host can launch one isolated interception-profiler child.</summary>
internal sealed record CoreClrProfilerGuardResult(
    bool Supported,
    CoreClrProfilerGuardFailureKind FailureKind,
    string? Failure)
{
    /// <summary>Gets the shared successful guard result.</summary>
    internal static CoreClrProfilerGuardResult Success { get; } =
        new(true, CoreClrProfilerGuardFailureKind.None, null);
}
