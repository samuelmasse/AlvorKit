namespace AlvorKit.Script.NativeBuild;

/// <summary>Executes native build plans for one library and target RID.</summary>
/// <param name="library">Resolved library metadata and paths.</param>
/// <param name="target">Target runtime identifier to build.</param>
/// <param name="processRunner">External process runner.</param>
/// <param name="host">Host values used for compatibility checks.</param>
[ExcludeFromCodeCoverage]
internal sealed class NativeBuildRunner(
    LibraryBuildContext library,
    TargetRid target,
    IProcessRunner processRunner,
    HostInfo host)
{
    /// <summary>Creates a runner with default side-effect services.</summary>
    public NativeBuildRunner(LibraryBuildContext library, TargetRid target)
        : this(library, target, new ProcessRunner(), HostInfo.Current())
    {
    }

    /// <summary>Builds the target binary and verifies its platform dependencies.</summary>
    public async Task BuildAsync()
    {
        HostCompatibility.EnsureCanBuild(target, host);
        var platform = library.Build.Platform(target.OperatingSystem);
        Console.WriteLine($"Building {library.Name} {target.Value} ({library.NativeVersion})");
        await SourceArchiveFetcher.EnsureSourceAsync(library);
        Directory.CreateDirectory(library.OutputDirectory(target));
        await BuildAsync(platform);
        await NativeBuildVerifier.VerifyAsync(library, target, processRunner, platform);
        Console.WriteLine($"OK {library.OutputFile(target)}");
    }

    /// <summary>Dispatches the manifest build kind to the matching build strategy.</summary>
    private Task BuildAsync(PlatformBuildConfig platform) =>
        library.Build.Kind switch
        {
            "single-c" => BuildSingleCAsync(platform),
            "cmake" => BuildCMakeAsync(platform),
            _ => throw new InvalidOperationException($"{library.Name}: unknown native build kind '{library.Build.Kind}'.")
        };

    /// <summary>Runs a direct compiler build for the selected platform.</summary>
    private async Task BuildSingleCAsync(PlatformBuildConfig platform)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
        {
            await WindowsScriptRunner.RunAsync(processRunner, WindowsBuildScripts.SingleC(library, target, platform));
            return;
        }

        var commands = target.OperatingSystem == TargetOperatingSystem.Linux
            ? NativeBuildPlanner.LinuxInstallCommands(target, platform).Concat(NativeBuildPlanner.SingleCLinuxCommands(library, target, platform))
            : NativeBuildPlanner.SingleCMacCommands(library, target, platform);
        foreach (var command in commands)
            await processRunner.RunAsync(command);
    }

    /// <summary>Runs a CMake build for the selected platform.</summary>
    private async Task BuildCMakeAsync(PlatformBuildConfig platform)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
        {
            await WindowsScriptRunner.RunAsync(processRunner, WindowsBuildScripts.CMake(library, target, platform));
            return;
        }

        var installCommands = target.OperatingSystem == TargetOperatingSystem.Linux
            ? NativeBuildPlanner.LinuxInstallCommands(target, platform)
            : [];
        var buildCommands = target.OperatingSystem == TargetOperatingSystem.Linux
            ? NativeBuildPlanner.CMakeLinuxCommands(library, target, platform)
            : NativeBuildPlanner.CMakeMacCommands(library, target, platform);
        foreach (var command in installCommands.Concat(buildCommands.SkipLast(1)))
            await processRunner.RunAsync(command);
        CopyCMakeOutput(platform);
        await processRunner.RunAsync(buildCommands[^1]);
    }

    /// <summary>Copies the resolved CMake output into the package runtimes directory.</summary>
    private void CopyCMakeOutput(PlatformBuildConfig platform)
    {
        var cmakeOutput = NativeBuildPlanner.RequiredCMakeOutput(library, platform);
        File.Copy(ResolveFile(library.BuildFile(target, cmakeOutput)), library.OutputFile(target), overwrite: true);
    }

    /// <summary>Returns the final linked target when the file is a link, otherwise the original path.</summary>
    private static string ResolveFile(string path)
    {
        var info = new FileInfo(path);
        return info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? path;
    }
}
