namespace AlvorKit.Script.NativeBuild;

/// <summary>Creates side-effect-free command plans for native verification runs.</summary>
internal static class NativeVerifyPlanner
{
    /// <summary>Creates the verifier compile and run commands for one supported native library and target RID.</summary>
    public static NativeVerifyPlan Create(
        LibraryBuildContext library,
        TargetRid target,
        NativeVerifyRunPrefix? runPrefix = null)
    {
        var verifierName = VerifierName(library);

        var source = Path.Combine(library.LibraryDirectory, "verify", verifierName + ".c");
        var artifactDirectory = Path.Combine(library.RepositoryRoot, "out", "native-verify", library.Name, target.Value);
        var executable = Path.Combine(artifactDirectory, target.OperatingSystem == TargetOperatingSystem.Windows
            ? verifierName + ".exe"
            : verifierName);
        var report = Path.Combine(artifactDirectory, "report.json");
        var compileCommand = CompileCommand(target, source, executable, artifactDirectory);
        var libraryPath = library.OutputFile(target);
        return new(
            libraryPath,
            source,
            artifactDirectory,
            executable,
            report,
            compileCommand,
            RunCommand(target, executable, libraryPath, report, artifactDirectory, runPrefix));
    }

    /// <summary>Returns the verifier program basename for a supported native library.</summary>
    private static string VerifierName(LibraryBuildContext library) =>
        library.Name switch
        {
            "xxhash" or "fastnoise2" => "verify-" + library.Name,
            _ => throw new NotSupportedException("Native verification currently supports xxhash and fastnoise2 only.")
        };

    /// <summary>Creates a compiler invocation for the verifier source and target RID.</summary>
    private static CommandSpec CompileCommand(
        TargetRid target,
        string sourcePath,
        string executablePath,
        string artifactDirectory) =>
        target.OperatingSystem switch
        {
            TargetOperatingSystem.Windows => new(
                "cl",
                ["/nologo", "/O2", "/MT", "/Fe:" + executablePath, sourcePath],
                artifactDirectory,
                CreateWorkingDirectory: true),
            TargetOperatingSystem.Linux => new(
                target.LinuxCompiler,
                ["-O2", "-o", executablePath, sourcePath, "-ldl"],
                artifactDirectory,
                CreateWorkingDirectory: true),
            TargetOperatingSystem.MacOS => new(
                "clang",
                ["-O2", "-arch", target.MacArchitecture, "-mmacosx-version-min=11.0", "-o", executablePath, sourcePath],
                artifactDirectory,
                CreateWorkingDirectory: true),
            _ => throw new PlatformNotSupportedException()
        };

    /// <summary>Creates the command that executes the compiled verifier and writes its report.</summary>
    private static CommandSpec RunCommand(
        TargetRid target,
        string executablePath,
        string libraryPath,
        string reportPath,
        string artifactDirectory,
        NativeVerifyRunPrefix? runPrefix) =>
        runPrefix is null
            ? new(executablePath, [libraryPath, reportPath, target.Value], artifactDirectory)
            : new(
                runPrefix.FileName,
                [.. runPrefix.Arguments, executablePath, libraryPath, reportPath, target.Value],
                artifactDirectory);
}
