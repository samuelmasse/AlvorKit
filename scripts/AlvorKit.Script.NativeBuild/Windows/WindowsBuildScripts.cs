namespace AlvorKit.Script.NativeBuild;

/// <summary>Generates Windows PowerShell build fragments for MSVC-based builds.</summary>
internal static class WindowsBuildScripts
{
    /// <summary>Visual Studio component ID for ClangCL support.</summary>
    private const string ClangClComponent = "Microsoft.VisualStudio.Component.VC.Llvm.Clang";

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

    /// <summary>Generates the Windows CMake PowerShell script.</summary>
    public static string CMake(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var cmakeOutput = NativeBuildPlanner.RequiredCMakeOutput(library, platform);
        var sourceDirectory = CommandText.PowerShellQuote(library.SourceDirectory);
        var buildDirectory = CommandText.PowerShellQuote(library.BuildDirectory(target));
        return Templates.Render(
            "cmake.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target, RequiresClangCl(platform))),
            ("SourceDirectory", sourceDirectory),
            ("BuildDirectory", buildDirectory),
            ("CMakeOptions", CommandText.PowerShellArgs(platform.CMakeOptions)),
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
            ("ClangClCheck", requiresClangCl ? ClangClCheck() : ""),
            ("WindowsArchitecture", target.WindowsArchitecture),
            ("VisualStudioArchitecture", CommandText.PowerShellQuote(target.VisualStudioArchitecture)));
    }

    /// <summary>Returns true when CMake options request ClangCL explicitly.</summary>
    private static bool RequiresClangCl(PlatformBuildConfig platform) =>
        platform.CMakeOptions.Any(option => option.Contains("clang-cl", StringComparison.OrdinalIgnoreCase));

    /// <summary>Generates the post-dev-shell ClangCL availability check.</summary>
    private static string ClangClCheck() =>
        "if (-not (Get-Command clang-cl -ErrorAction SilentlyContinue)) { "
        + "throw \"clang-cl was requested but was not found after launching the Visual Studio developer shell.\" }"
        + Environment.NewLine;
}
