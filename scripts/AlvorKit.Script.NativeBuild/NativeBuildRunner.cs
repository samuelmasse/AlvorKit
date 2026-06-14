namespace AlvorKit.Script.NativeBuild;

/// <summary>Executes native build plans for one library and target RID.</summary>
internal sealed class NativeBuildRunner
{
    /// <summary>Resolved library metadata and paths.</summary>
    private readonly LibraryBuildContext library;

    /// <summary>Target runtime identifier to build.</summary>
    private readonly TargetRid target;

    /// <summary>External process runner.</summary>
    private readonly IProcessRunner processRunner;

    /// <summary>PowerShell script runner for Windows build fragments.</summary>
    private readonly WindowsScriptRunner windows;

    /// <summary>Source archive fetcher.</summary>
    private readonly SourceArchiveFetcher sources;

    /// <summary>Host values used for compatibility checks.</summary>
    private readonly HostInfo host;

    /// <summary>Creates a runner with default side-effect services.</summary>
    public NativeBuildRunner(LibraryBuildContext library, TargetRid target)
        : this(library, target, new ProcessRunner(), new SourceArchiveFetcher(), HostInfo.Current())
    {
    }

    /// <summary>Creates a runner with injected side-effect services for tests.</summary>
    public NativeBuildRunner(
        LibraryBuildContext library,
        TargetRid target,
        IProcessRunner processRunner,
        SourceArchiveFetcher sources,
        HostInfo host)
    {
        this.library = library;
        this.target = target;
        this.processRunner = processRunner;
        this.sources = sources;
        this.host = host;
        windows = new(processRunner);
    }

    /// <summary>Builds the target binary and verifies its platform dependencies.</summary>
    public async Task BuildAsync()
    {
        HostCompatibility.EnsureCanBuild(target, host);
        var platform = library.Build.Platform(target.OperatingSystem);
        Console.WriteLine($"Building {library.Name} {target.Value} ({library.NativeVersion})");
        await sources.EnsureSourceAsync(library);
        Directory.CreateDirectory(library.OutputDirectory(target));
        await BuildAsync(platform);
        await new NativeBuildVerifier(library, target, processRunner, windows).VerifyAsync(platform);
        Console.WriteLine($"OK {library.OutputFile(target)}");
    }

    /// <summary>Dispatches the manifest build kind to the matching build strategy.</summary>
    private Task BuildAsync(PlatformBuildConfig platform) =>
        library.Build.Kind switch
        {
            NativeBuildKinds.SingleC => BuildSingleCAsync(platform),
            NativeBuildKinds.CMake => BuildCMakeAsync(platform),
            _ => throw new InvalidOperationException($"{library.Name}: unknown native build kind '{library.Build.Kind}'.")
        };

    /// <summary>Runs a direct compiler build for the selected platform.</summary>
    private async Task BuildSingleCAsync(PlatformBuildConfig platform)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
        {
            await windows.RunAsync(WindowsBuildScripts.SingleC(library, target, platform));
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
            await windows.RunAsync(WindowsBuildScripts.CMake(library, target, platform));
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
        File.Copy(FileLinkResolver.ResolveFile(library.BuildFile(target, cmakeOutput)), library.OutputFile(target), overwrite: true);
    }
}
