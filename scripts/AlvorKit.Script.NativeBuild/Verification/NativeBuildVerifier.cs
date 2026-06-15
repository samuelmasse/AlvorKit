namespace AlvorKit.Script.NativeBuild;

/// <summary>Runs platform-specific checks against a built native binary.</summary>
internal static class NativeBuildVerifier
{
    /// <summary>Runs dependency verification for the target platform.</summary>
    public static async Task VerifyAsync(
        LibraryBuildContext library,
        TargetRid target,
        IProcessRunner processRunner,
        PlatformBuildConfig platform)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows)
            await WindowsScriptRunner.RunAsync(processRunner, WindowsVerifyScript(library, target));
        else if (target.OperatingSystem == TargetOperatingSystem.Linux)
            await VerifyLinuxAsync(library, target, processRunner, platform);
        else
            await VerifyMacAsync(library, target, processRunner);
    }

    /// <summary>Generates Windows import-library verification script.</summary>
    internal static string WindowsVerifyScript(LibraryBuildContext library, TargetRid target) =>
        $$"""
        {{WindowsBuildScripts.VisualStudioDevShell(target)}}
        $Deps = & dumpbin /nologo /dependents {{CommandText.PowerShellQuote(library.OutputFile(target))}} | Select-String '\.dll'
        $Deps | ForEach-Object { Write-Host $_.Line.Trim() }
        if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime - static CRT did not take.' }
        """;

    /// <summary>Verifies Linux ELF dependencies against the manifest allow-list.</summary>
    private static async Task VerifyLinuxAsync(
        LibraryBuildContext library,
        TargetRid target,
        IProcessRunner processRunner,
        PlatformBuildConfig platform)
    {
        var output = await processRunner.CaptureAsync(new(target.LinuxReadElf, ["-d", library.OutputFile(target)]));
        var dependencies = NativeDependencyVerifier.ElfDependencies(output);
        Console.WriteLine($"ELF dependencies for {library.OutputFile(target)}:");
        foreach (var dependency in dependencies)
            Console.WriteLine("  " + dependency);
        NativeDependencyVerifier.EnsureElfDependenciesAllowed(dependencies, platform.AllowedDependencies);
    }

    /// <summary>Verifies the macOS binary architecture and prints linked libraries.</summary>
    private static async Task VerifyMacAsync(LibraryBuildContext library, TargetRid target, IProcessRunner processRunner)
    {
        var fileOutput = await processRunner.CaptureAsync(new("file", [library.OutputFile(target)]));
        Console.Write(fileOutput);
        if (!fileOutput.Contains(target.MacArchitecture, StringComparison.Ordinal))
            throw new InvalidOperationException($"{library.OutputFile(target)} is not {target.MacArchitecture}.");
        Console.Write(await processRunner.CaptureAsync(new("otool", ["-L", library.OutputFile(target)])));
    }
}
