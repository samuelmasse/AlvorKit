namespace AlvorKit;

/// <summary>Identifies one stable reason an interception-profiler launch is unavailable.</summary>
internal enum CoreClrProfilerGuardFailureKind
{
    /// <summary>Every required guard passed.</summary>
    None,

    /// <summary>No explicit interception launch was requested.</summary>
    OptInRequired,

    /// <summary>The operating system is unsupported.</summary>
    OperatingSystem,

    /// <summary>The process or operating-system architecture is unsupported.</summary>
    Architecture,

    /// <summary>The active runtime is not the supported Microsoft CoreCLR version.</summary>
    Runtime,

    /// <summary>The runtime cannot generate dynamic code.</summary>
    DynamicCode,

    /// <summary>A managed debugger is attached to the launcher.</summary>
    Debugger,

    /// <summary>CoreCLR diagnostics were explicitly disabled.</summary>
    DiagnosticsDisabled,

    /// <summary>The launcher inherited active profiler state.</summary>
    ActiveProfiler
}
