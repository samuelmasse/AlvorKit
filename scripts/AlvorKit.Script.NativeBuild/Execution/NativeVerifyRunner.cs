namespace AlvorKit.Script.NativeBuild;

/// <summary>Executes native verifier plans for one library and target RID.</summary>
/// <param name="library">Resolved library metadata and paths.</param>
/// <param name="target">Target runtime identifier to verify.</param>
/// <param name="processRunner">External process runner.</param>
/// <param name="host">Host values used for compatibility checks.</param>
internal sealed class NativeVerifyRunner(
    LibraryBuildContext library,
    TargetRid target,
    IProcessRunner processRunner,
    HostInfo host)
{
    /// <summary>Environment variable naming a linux-arm launcher, such as qemu-arm.</summary>
    private const string LinuxArmRunnerVariable = "ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER";

    /// <summary>Environment variable naming the optional linux-arm runtime sysroot passed with qemu -L.</summary>
    private const string LinuxArmSysrootVariable = "ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT";

    /// <summary>Creates a runner with default side-effect services.</summary>
    [ExcludeFromCodeCoverage]
    public NativeVerifyRunner(LibraryBuildContext library, TargetRid target)
        : this(library, target, new ProcessRunner(), HostInfo.Current())
    {
    }

    /// <summary>Compiles and runs the native verifier for the target runtime library.</summary>
    public async Task VerifyAsync()
    {
        HostCompatibility.EnsureCanBuild(target, host);
        var plan = NativeVerifyPlanner.XxHash(library, target, LinuxArmRunPrefix());
        EnsureFileExists(plan.SourcePath, "verification source");
        EnsureFileExists(plan.LibraryPath, "native runtime library");
        Directory.CreateDirectory(plan.ArtifactDirectory);

        Console.WriteLine($"Verifying {library.Name} {target.Value}");
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
            await WindowsScriptRunner.RunAsync(processRunner, WindowsBuildScripts.Verify(plan, target));
        else
            await processRunner.RunAsync(plan.CompileCommand);
        await processRunner.RunAsync(plan.RunCommand);
        Console.WriteLine($"OK {plan.ReportPath}");
    }

    /// <summary>Returns an emulator prefix when a linux-arm verifier cannot run directly on the host.</summary>
    private NativeVerifyRunPrefix? LinuxArmRunPrefix()
    {
        if (target.Value != "linux-arm" || host.Architecture == Architecture.Arm)
            return null;

        var runner = Environment.GetEnvironmentVariable(LinuxArmRunnerVariable);
        if (string.IsNullOrWhiteSpace(runner))
            throw new PlatformNotSupportedException(
                $"linux-arm verification on this host requires {LinuxArmRunnerVariable}, for example qemu-arm.");

        var sysroot = Environment.GetEnvironmentVariable(LinuxArmSysrootVariable);
        var arguments = string.IsNullOrWhiteSpace(sysroot) ? [] : new[] { "-L", sysroot };
        return new(runner, arguments);
    }

    /// <summary>Throws an actionable error when a required verifier input is missing.</summary>
    private static void EnsureFileExists(string path, string description)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing {description}: {path}", path);
    }
}
