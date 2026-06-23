namespace AlvorKit.Script.NativeBuild;

/// <summary>Generates Windows PowerShell build fragments for MSVC-based builds.</summary>
internal static class WindowsBuildScripts
{
    /// <summary>Visual Studio component ID for ClangCL support.</summary>
    private const string ClangClComponent = "Microsoft.VisualStudio.Component.VC.Llvm.Clang";

    /// <summary>CMake option requesting the C compiler through ClangCL.</summary>
    private const string ClangClCCompilerOption = "-DCMAKE_C_COMPILER=clang-cl";

    /// <summary>CMake option requesting the C++ compiler through ClangCL.</summary>
    private const string ClangClCxxCompilerOption = "-DCMAKE_CXX_COMPILER=clang-cl";

    /// <summary>Template files for Windows native build scripts.</summary>
    private static readonly RepositoryTemplateSet Templates = RepositoryTemplates.ForArea(typeof(WindowsBuildScripts), "native-build/windows");

    /// <summary>Generates the Windows direct C compiler PowerShell script.</summary>
    public static string SingleC(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var outputFile = library.OutputFile(target);
        var sourceDirectory = CommandText.PowerShellQuote(library.SourceDirectory);
        var outputArgument = CommandText.PowerShellQuote("/Fe:" + outputFile);
        var implFile = CommandText.PowerShellQuote(NativeBuildPlanner.RequiredImplFile(library));
        var linkLibraries = CommandText.PowerShellArgs(platform.LinkLibraries.Select(link => link + ".lib"));
        var buildDirectory = CommandText.PowerShellQuote(library.BuildDirectory(target));
        return Templates.Render(
            "single-c.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target, requiresClangCl: false)),
            ("BuildDirectory", buildDirectory),
            ("SourceDirectory", sourceDirectory),
            ("OutputArgument", outputArgument),
            ("ImplFile", implFile),
            ("LinkLibraries", linkLibraries));
    }

    /// <summary>Generates the Windows verifier compiler PowerShell script.</summary>
    public static string Verify(NativeVerifyPlan plan, TargetRid target) =>
        Templates.Render(
            "verify-xxhash.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target, requiresClangCl: false)),
            ("ArtifactDirectory", CommandText.PowerShellQuote(plan.ArtifactDirectory)),
            ("OutputArgument", CommandText.PowerShellQuote("/Fe:" + plan.ExecutablePath)),
            ("SourcePath", CommandText.PowerShellQuote(plan.SourcePath)));

    /// <summary>Generates the Windows CMake PowerShell script.</summary>
    public static string CMake(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var cmakeOutput = NativeBuildPlanner.RequiredCMakeOutput(library, platform);
        var requiresClangCl = RequiresClangCl(platform, target);
        var sourceDirectory = CommandText.PowerShellQuote(library.SourceDirectory);
        var buildDirectory = CommandText.PowerShellQuote(library.BuildDirectory(target));
        return Templates.Render(
            "cmake.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target, requiresClangCl)),
            ("SourceDirectory", sourceDirectory),
            ("BuildDirectory", buildDirectory),
            ("CMakeOptions", CMakeOptions(platform, target, requiresClangCl)),
            ("CMakeOutputFile", CommandText.PowerShellQuote(library.BuildFile(target, cmakeOutput))),
            ("OutputFile", CommandText.PowerShellQuote(library.OutputFile(target))));
    }

    /// <summary>Generates the Visual Studio developer shell setup fragment.</summary>
    public static string VisualStudioDevShell(TargetRid target) =>
        VisualStudioDevShell(target, requiresClangCl: false);

    /// <summary>Generates the Visual Studio developer shell setup fragment with optional ClangCL support.</summary>
    private static string VisualStudioDevShell(TargetRid target, bool requiresClangCl)
    {
        var toolset = target.Architecture == TargetArchitecture.Arm64
            ? "Microsoft.VisualStudio.Component.VC.Tools.ARM64"
            : "Microsoft.VisualStudio.Component.VC.Tools.x86.x64";
        var components = requiresClangCl ? [toolset, ClangClComponent] : new[] { toolset };
        var requiredComponents = CommandText.PowerShellArgs(components);
        var missing = requiresClangCl
            ? $"No Visual Studio with the {target.WindowsArchitecture} C++ toolset and ClangCL component found."
            : $"No Visual Studio with the {target.WindowsArchitecture} C++ toolset found.";
        return Templates.Render(
            "dev-shell.ps1.tmpl",
            ("RequiredComponents", requiredComponents),
            ("MissingMessage", CommandText.PowerShellQuote(missing)),
            ("ClangClCheck", requiresClangCl ? ClangClCheck(target) : ""),
            ("WindowsArchitecture", target.WindowsArchitecture),
            ("VisualStudioArchitecture", CommandText.PowerShellQuote(target.VisualStudioArchitecture)));
    }

    /// <summary>Returns true when CMake options request ClangCL explicitly.</summary>
    private static bool RequiresClangCl(PlatformBuildConfig platform, TargetRid target) =>
        platform.CMakeOptionsFor(target).Any(option => option.Contains("clang-cl", StringComparison.OrdinalIgnoreCase));

    /// <summary>Generates the post-dev-shell ClangCL availability check.</summary>
    private static string ClangClCheck(TargetRid target) =>
        "$ClangCl = Join-Path $env:VCINSTALLDIR "
        + CommandText.PowerShellQuote(ClangClRelativePath(target.Architecture))
        + Environment.NewLine
        + "if (-not (Test-Path $ClangCl)) { "
        + "throw \"clang-cl was requested but was not found in the Visual Studio LLVM toolset.\" }"
        + Environment.NewLine;

    /// <summary>Generates PowerShell CMake option arguments, replacing clang-cl with the Visual Studio compiler path.</summary>
    private static string CMakeOptions(PlatformBuildConfig platform, TargetRid target, bool requiresClangCl)
    {
        if (!requiresClangCl)
            return CommandText.PowerShellArgs(platform.CMakeOptionsFor(target));

        var options = platform.CMakeOptionsFor(target)
            .Where(option => !IsClangClCompilerOption(option))
            .Select(CommandText.PowerShellQuote)
            .Concat(ClangClTargetOptions(target).Select(CommandText.PowerShellQuote))
            .Append("\"-DCMAKE_C_COMPILER=$ClangCl\"")
            .Append("\"-DCMAKE_CXX_COMPILER=$ClangCl\"");
        return string.Join(" ", options);
    }

    /// <summary>Returns explicit compiler target options needed when the host ClangCL binary does not imply the target architecture.</summary>
    private static IEnumerable<string> ClangClTargetOptions(TargetRid target) =>
        target.Architecture == TargetArchitecture.X86
            ? ["-DCMAKE_C_COMPILER_TARGET=i686-pc-windows-msvc", "-DCMAKE_CXX_COMPILER_TARGET=i686-pc-windows-msvc"]
            : [];

    /// <summary>Returns whether an option is replaced by the generated Visual Studio ClangCL compiler path.</summary>
    private static bool IsClangClCompilerOption(string option) =>
        string.Equals(option, ClangClCCompilerOption, StringComparison.OrdinalIgnoreCase)
        || string.Equals(option, ClangClCxxCompilerOption, StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns the Visual Studio LLVM compiler path relative to <c>VCINSTALLDIR</c>.</summary>
    private static string ClangClRelativePath(TargetArchitecture architecture) =>
        architecture switch
        {
            TargetArchitecture.X86 => @"Tools\Llvm\x64\bin\clang-cl.exe",
            TargetArchitecture.X64 => @"Tools\Llvm\x64\bin\clang-cl.exe",
            TargetArchitecture.Arm64 => @"Tools\Llvm\ARM64\bin\clang-cl.exe",
            _ => throw new PlatformNotSupportedException($"{architecture} is not a Windows architecture.")
        };
}
