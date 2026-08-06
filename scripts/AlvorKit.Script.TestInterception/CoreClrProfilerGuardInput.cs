namespace AlvorKit.Script.TestInterception;

/// <summary>Captures the managed host state evaluated before a profiled child launch.</summary>
internal sealed record CoreClrProfilerGuardInput(
    bool IsOptedIn,
    bool IsWindows,
    bool IsLinux,
    bool IsMacOS,
    Architecture ProcessArchitecture,
    Architecture OsArchitecture,
    int RuntimeMajor,
    bool IsMicrosoftCoreClr,
    bool IsDynamicCodeSupported,
    bool IsDebuggerAttached,
    IReadOnlyDictionary<string, string?> Environment);
