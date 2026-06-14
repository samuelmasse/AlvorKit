namespace AlvorKit.Script.NativeBuild;

/// <summary>Runs platform-specific checks against a built native binary.</summary>
internal sealed class NativeBuildVerifier
{
    /// <summary>Resolved library metadata and paths.</summary>
    private readonly LibraryBuildContext library;

    /// <summary>Target runtime identifier being verified.</summary>
    private readonly TargetRid target;

    /// <summary>External process runner used for inspection tools.</summary>
    private readonly IProcessRunner processRunner;

    /// <summary>PowerShell runner used for Windows dumpbin checks.</summary>
    private readonly WindowsScriptRunner windows;

    /// <summary>Creates a verifier for one library and target.</summary>
    public NativeBuildVerifier(
        LibraryBuildContext library,
        TargetRid target,
        IProcessRunner processRunner,
        WindowsScriptRunner windows)
    {
        this.library = library;
        this.target = target;
        this.processRunner = processRunner;
        this.windows = windows;
    }

    /// <summary>Runs dependency verification for the target platform.</summary>
    public async Task VerifyAsync(PlatformBuildConfig platform)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
            await windows.RunAsync(WindowsVerifyScript());
        else if (target.OperatingSystem == TargetOperatingSystem.Linux)
            await VerifyLinuxAsync(platform);
        else
            await VerifyMacAsync();
    }

    /// <summary>Generates Windows import-library verification script.</summary>
    private string WindowsVerifyScript() =>
        $$"""
        $Deps = & dumpbin /nologo /dependents {{CommandText.PowerShellQuote(library.OutputFile(target))}} | Select-String '\.dll'
        $Deps | ForEach-Object { Write-Host $_.Line.Trim() }
        if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime - static CRT did not take.' }
        """;

    /// <summary>Verifies Linux ELF dependencies against the manifest allow-list.</summary>
    private async Task VerifyLinuxAsync(PlatformBuildConfig platform)
    {
        var output = await processRunner.CaptureAsync(new(target.LinuxReadElf, ["-d", library.OutputFile(target)]));
        var dependencies = NativeDependencyVerifier.ElfDependencies(output);
        Console.WriteLine($"ELF dependencies for {library.OutputFile(target)}:");
        foreach (var dependency in dependencies)
            Console.WriteLine("  " + dependency);
        NativeDependencyVerifier.EnsureElfDependenciesAllowed(dependencies, platform.AllowedDependencies);
    }

    /// <summary>Verifies the macOS binary architecture and prints linked libraries.</summary>
    private async Task VerifyMacAsync()
    {
        var fileOutput = await processRunner.CaptureAsync(new("file", [library.OutputFile(target)]));
        Console.Write(fileOutput);
        if (!fileOutput.Contains(target.MacArchitecture, StringComparison.Ordinal))
            throw new InvalidOperationException($"{library.OutputFile(target)} is not {target.MacArchitecture}.");
        Console.Write(await processRunner.CaptureAsync(new("otool", ["-L", library.OutputFile(target)])));
    }
}
