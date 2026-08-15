namespace AlvorKit;

/// <summary>Verifies the production host contract for isolated profiler children.</summary>
[TestClass]
public sealed class CoreClrProfilerGuardTest
{
    /// <summary>Requires an explicit interception launch before evaluating host support.</summary>
    [TestMethod]
    public void EvaluateRequiresOptIn()
    {
        var result = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with { IsOptedIn = false });

        Assert.IsFalse(result.Supported);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.OptInRequired,
            result.FailureKind);
        StringAssert.Contains(result.Failure, "explicit");
    }

    /// <summary>Accepts the supported Windows, Linux, and macOS .NET 10 hosts.</summary>
    [TestMethod]
    public void EvaluateAcceptsSupportedHost()
    {
        var windows = CoreClrProfilerGuard.Evaluate(SupportedInput());
        var linux = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                IsWindows = false,
                IsLinux = true,
                IsMacOS = false
            });
        var linuxArm64 = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                IsWindows = false,
                IsLinux = true,
                IsMacOS = false,
                ProcessArchitecture = Architecture.Arm64,
                OsArchitecture = Architecture.Arm64
            });
        var macArm64 = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                IsWindows = false,
                IsLinux = false,
                IsMacOS = true,
                ProcessArchitecture = Architecture.Arm64,
                OsArchitecture = Architecture.Arm64
            });

        Assert.IsTrue(windows.Supported, windows.Failure);
        Assert.IsTrue(linux.Supported, linux.Failure);
        Assert.IsTrue(linuxArm64.Supported, linuxArm64.Failure);
        Assert.IsTrue(macArm64.Supported, macArm64.Failure);
        Assert.AreEqual(CoreClrProfilerGuardFailureKind.None, windows.FailureKind);
        Assert.AreEqual(CoreClrProfilerGuardFailureKind.None, linux.FailureKind);
        Assert.IsNull(windows.Failure);
        Assert.IsNull(linux.Failure);
    }

    /// <summary>Rejects disabled diagnostics and inherited profiler state deterministically.</summary>
    [TestMethod]
    public void EvaluateRejectsConflictingRuntimeState()
    {
        var diagnostics = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                Environment = new Dictionary<string, string?>
                {
                    ["DOTNET_EnableDiagnostics"] = "0"
                }
            });
        var profiler = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                Environment = new Dictionary<string, string?>
                {
                    ["DOTNET_PROFILER"] = "{foreign}",
                    ["CORECLR_PROFILER_PATH_64"] = @"C:\foreign\profiler.dll"
                }
            });

        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.DiagnosticsDisabled,
            diagnostics.FailureKind);
        StringAssert.Contains(diagnostics.Failure, "diagnostics");
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.ActiveProfiler,
            profiler.FailureKind);
        StringAssert.Contains(profiler.Failure, "CORECLR_PROFILER_PATH_64");
    }

    /// <summary>Reports each unsupported host condition with its stable category.</summary>
    [TestMethod]
    public void EvaluateReportsUnsupportedHostConditions()
    {
        var operatingSystem = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                IsWindows = false,
                IsLinux = false,
                IsMacOS = false
            });
        var windowsArm64 = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                ProcessArchitecture = Architecture.Arm64
            });
        var mismatchedArchitecture = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                OsArchitecture = Architecture.Arm64
            });
        var runtimeVersion = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with { RuntimeMajor = 9 });
        var runtimeFamily = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with { IsMicrosoftCoreClr = false });
        var dynamicCode = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with { IsDynamicCodeSupported = false });
        var debugger = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with { IsDebuggerAttached = true });

        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.OperatingSystem,
            operatingSystem.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.Architecture,
            windowsArm64.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.Architecture,
            mismatchedArchitecture.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.Runtime,
            runtimeVersion.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.Runtime,
            runtimeFamily.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.DynamicCode,
            dynamicCode.FailureKind);
        Assert.AreEqual(
            CoreClrProfilerGuardFailureKind.Debugger,
            debugger.FailureKind);
        StringAssert.Contains(operatingSystem.Failure, "Windows, Linux, and macOS");
        StringAssert.Contains(windowsArm64.Failure, "Windows x64");
        StringAssert.Contains(runtimeVersion.Failure, ".NET 10");
        StringAssert.Contains(dynamicCode.Failure, "dynamic-code");
        StringAssert.Contains(debugger.Failure, "debugger");
    }

    /// <summary>Allows empty or explicitly disabled inherited profiler settings.</summary>
    [TestMethod]
    public void EvaluateIgnoresInactiveProfilerSettings()
    {
        var result = CoreClrProfilerGuard.Evaluate(
            SupportedInput() with
            {
                Environment = new Dictionary<string, string?>
                {
                    ["CORECLR_ENABLE_PROFILING"] = "0",
                    ["COR_ENABLE_PROFILING"] = "false",
                    ["CORECLR_PROFILER"] = string.Empty
                }
            });

        Assert.IsTrue(result.Supported, result.Failure);
    }

    /// <summary>Creates the exact supported host snapshot without inherited state.</summary>
    private static CoreClrProfilerGuardInput SupportedInput() =>
        new(
            IsOptedIn: true,
            IsWindows: true,
            IsLinux: false,
            IsMacOS: false,
            ProcessArchitecture: Architecture.X64,
            OsArchitecture: Architecture.X64,
            RuntimeMajor: 10,
            IsMicrosoftCoreClr: true,
            IsDynamicCodeSupported: true,
            IsDebuggerAttached: false,
            new Dictionary<string, string?>());
}
