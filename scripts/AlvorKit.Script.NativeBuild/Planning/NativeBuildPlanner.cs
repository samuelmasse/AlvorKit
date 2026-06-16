namespace AlvorKit.Script.NativeBuild;

/// <summary>Creates side-effect-free command specs and scripts for native builds.</summary>
internal static class NativeBuildPlanner
{
    /// <summary>Plans Linux package installation commands.</summary>
    public static IReadOnlyList<CommandSpec> LinuxInstallCommands(TargetRid target, PlatformBuildConfig platform)
    {
        var packages = platform.LinuxPackages(target).ToArray();
        if (packages.Length == 0)
            return [];

        var addArchitecture = target.Architecture == TargetArchitecture.Arm && platform.ArmArchitecture is { Length: > 0 }
            ? [new CommandSpec("sudo", ["dpkg", "--add-architecture", platform.ArmArchitecture])]
            : Array.Empty<CommandSpec>();

        return [
            .. addArchitecture,
            new("sudo", ["apt-get", "update", "-qq"]),
            new("sudo", ["apt-get", "install", "-y", "-qq", .. packages])
        ];
    }

    /// <summary>Plans commands for a direct Linux C compiler build.</summary>
    public static IReadOnlyList<CommandSpec> SingleCLinuxCommands(
        LibraryBuildContext library,
        TargetRid target,
        PlatformBuildConfig platform)
    {
        var implFile = RequiredImplFile(library);
        return [
            new(target.LinuxCompiler, [
                "-shared", "-fPIC", "-O2", "-Wl,--no-undefined",
                "-I", library.SourceDirectory,
                "-o", library.OutputFile(target),
                implFile,
                .. platform.LinkLibraries.Select(link => "-l" + link)
            ], library.BuildDirectory(target), CreateWorkingDirectory: true),
            new(target.LinuxStrip, [library.OutputFile(target)])
        ];
    }

    /// <summary>Plans commands for a direct macOS clang build.</summary>
    public static IReadOnlyList<CommandSpec> SingleCMacCommands(
        LibraryBuildContext library,
        TargetRid target,
        PlatformBuildConfig platform)
    {
        var outputFile = library.OutputFile(target);
        return [
            new("clang", [
                "-dynamiclib", "-O2", "-arch", target.MacArchitecture,
                "-mmacosx-version-min=11.0",
                "-install_name", "@rpath/" + Path.GetFileName(outputFile),
                "-I", library.SourceDirectory,
                "-o", outputFile,
                RequiredImplFile(library),
                .. platform.LinkLibraries.Select(link => "-l" + link)
            ], library.BuildDirectory(target), CreateWorkingDirectory: true),
            new("strip", ["-x", outputFile])
        ];
    }

    /// <summary>Plans commands for a Linux CMake build.</summary>
    public static IReadOnlyList<CommandSpec> CMakeLinuxCommands(
        LibraryBuildContext library,
        TargetRid target,
        PlatformBuildConfig platform) =>
        [
            new("cmake", [
                "--fresh", "-S", library.SourceDirectory,
                "-B", library.BuildDirectory(target),
                "-DCMAKE_C_COMPILER=" + target.LinuxCompiler,
                "-DCMAKE_BUILD_TYPE=Release",
                .. platform.CMakeOptions
            ]),
            new("cmake", ["--build", library.BuildDirectory(target), "-j"]),
            new(target.LinuxStrip, [library.OutputFile(target)])
        ];

    /// <summary>Plans commands for a macOS CMake build.</summary>
    public static IReadOnlyList<CommandSpec> CMakeMacCommands(
        LibraryBuildContext library,
        TargetRid target,
        PlatformBuildConfig platform) =>
        [
            new("cmake", [
                "--fresh", "-S", library.SourceDirectory,
                "-B", library.BuildDirectory(target),
                "-DCMAKE_BUILD_TYPE=Release",
                "-DCMAKE_OSX_ARCHITECTURES=" + target.MacArchitecture,
                "-DCMAKE_OSX_DEPLOYMENT_TARGET=11.0",
                .. platform.CMakeOptions
            ]),
            new("cmake", ["--build", library.BuildDirectory(target), "-j"]),
            new("strip", ["-x", library.OutputFile(target)])
        ];

    /// <summary>Returns the local implementation file required by single-C builds.</summary>
    public static string RequiredImplFile(LibraryBuildContext library) =>
        library.Metadata.ImplFile is { Length: > 0 } implFile
            ? Path.Combine(library.LibraryDirectory, implFile)
            : throw new InvalidOperationException($"{library.Name}: single-c builds require implFile in conf/bindgen.yml.");

    /// <summary>Returns the configured CMake output or throws a manifest error.</summary>
    public static string RequiredCMakeOutput(LibraryBuildContext library, PlatformBuildConfig platform) =>
        platform.CMakeOutput is { Length: > 0 } output
            ? output
            : throw new InvalidOperationException($"{library.Name}: cmake builds require cmakeOutput.");
}
