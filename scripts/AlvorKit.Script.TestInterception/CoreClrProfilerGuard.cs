namespace AlvorKit.Script.TestInterception;

/// <summary>Evaluates the supported private-child CoreCLR profiler launch contract.</summary>
internal static class CoreClrProfilerGuard
{
    /// <summary>Captures and evaluates the current managed launcher process.</summary>
    internal static CoreClrProfilerGuardResult EvaluateCurrent(
        bool isOptedIn)
    {
        Dictionary<string, string?> environment = new(
            StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            environment[(string)entry.Key] = entry.Value?.ToString();

        var framework = RuntimeInformation.FrameworkDescription;
        var coreLibrary = typeof(object).Assembly.GetName().Name ?? string.Empty;
        return Evaluate(
            new(
                isOptedIn,
                OperatingSystem.IsWindows(),
                OperatingSystem.IsLinux(),
                OperatingSystem.IsMacOS(),
                RuntimeInformation.ProcessArchitecture,
                RuntimeInformation.OSArchitecture,
                Environment.Version.Major,
                framework.StartsWith(".NET ", StringComparison.Ordinal) &&
                coreLibrary == "System.Private.CoreLib",
                RuntimeFeature.IsDynamicCodeSupported,
                Debugger.IsAttached,
                environment));
    }

    /// <summary>Evaluates a supplied immutable host snapshot in stable guard order.</summary>
    internal static CoreClrProfilerGuardResult Evaluate(
        CoreClrProfilerGuardInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.IsOptedIn)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.OptInRequired,
                "An explicit interception-profiler child launch is required.");
        }
        var operatingSystemCount =
            (input.IsWindows ? 1 : 0) +
            (input.IsLinux ? 1 : 0) +
            (input.IsMacOS ? 1 : 0);
        if (operatingSystemCount != 1)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.OperatingSystem,
                "The interception profiler supports Windows, Linux, and macOS only.");
        }
        var matchingArchitecture =
            input.ProcessArchitecture == input.OsArchitecture;
        var supportedArchitecture =
            ((input.IsWindows || input.IsLinux) &&
                input.ProcessArchitecture == Architecture.X64) ||
            ((input.IsLinux || input.IsMacOS) &&
                input.ProcessArchitecture == Architecture.Arm64);
        if (!matchingArchitecture || !supportedArchitecture)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.Architecture,
                "The interception profiler requires Windows x64, Linux x64/Arm64, or macOS Arm64 with matching process and OS architectures.");
        }
        if (input.RuntimeMajor != 10 || !input.IsMicrosoftCoreClr)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.Runtime,
                "The interception profiler requires Microsoft CoreCLR for .NET 10.");
        }
        if (!input.IsDynamicCodeSupported)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.DynamicCode,
                "The interception profiler requires JIT dynamic-code support.");
        }
        if (input.IsDebuggerAttached)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.Debugger,
                "The interception-profiler launcher cannot run under a debugger.");
        }
        if (DiagnosticsDisabled(input.Environment))
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.DiagnosticsDisabled,
                "CoreCLR diagnostics are disabled.");
        }

        var activeProfiler = InterceptionProfilerEnvironment
            .ActiveVariables(input.Environment)
            .FirstOrDefault();
        if (activeProfiler is not null)
        {
            return Failed(
                CoreClrProfilerGuardFailureKind.ActiveProfiler,
                $"An inherited profiler setting is active: {activeProfiler}.");
        }

        return CoreClrProfilerGuardResult.Success;
    }

    /// <summary>
    /// Requires the current host and throws an availability-appropriate launch exception.
    /// </summary>
    internal static void RequireCurrent(bool isOptedIn)
    {
        var result = EvaluateCurrent(isOptedIn);
        if (result.Supported)
            return;

        if (result.FailureKind is
            CoreClrProfilerGuardFailureKind.OperatingSystem or
            CoreClrProfilerGuardFailureKind.Architecture or
            CoreClrProfilerGuardFailureKind.Runtime or
            CoreClrProfilerGuardFailureKind.DynamicCode or
            CoreClrProfilerGuardFailureKind.Debugger)
        {
            throw new PlatformNotSupportedException(result.Failure);
        }

        throw new InvalidOperationException(result.Failure);
    }

    /// <summary>Returns whether either supported runtime variable disables diagnostics.</summary>
    private static bool DiagnosticsDisabled(
        IReadOnlyDictionary<string, string?> environment) =>
        IsZero(environment, "DOTNET_EnableDiagnostics") ||
        IsZero(environment, "COMPlus_EnableDiagnostics");

    /// <summary>Returns whether one case-insensitive environment value is numeric zero.</summary>
    private static bool IsZero(
        IReadOnlyDictionary<string, string?> environment,
        string name) =>
        environment.TryGetValue(name, out var value) &&
        string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);

    /// <summary>Creates one unsupported result.</summary>
    private static CoreClrProfilerGuardResult Failed(
        CoreClrProfilerGuardFailureKind kind,
        string failure) =>
        new(false, kind, failure);
}
