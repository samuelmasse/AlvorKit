namespace AlvorKit.Script.NativeBuild;

/// <summary>Generates Windows PowerShell build fragments for MSVC-based builds.</summary>
internal static class WindowsBuildScripts
{
    /// <summary>Generates the Windows direct C compiler PowerShell script.</summary>
    public static string SingleC(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var outputFile = library.OutputFile(target);
        var sourceDirectory = CommandText.PowerShellQuote(library.SourceDirectory);
        var outputArgument = CommandText.PowerShellQuote("/Fe:" + outputFile);
        var implFile = CommandText.PowerShellQuote(NativeBuildPlanner.RequiredImplFile(library));
        var linkLibraries = CommandText.PowerShellArgs(platform.LinkLibraries.Select(link => link + ".lib"));
        var buildDirectory = CommandText.PowerShellQuote(library.BuildDirectory(target));
        return TemplateResource.Render(
            "res/templates/native-build/windows/single-c.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target)),
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
        return TemplateResource.Render(
            "res/templates/native-build/windows/cmake.ps1.tmpl",
            ("VisualStudioDevShell", VisualStudioDevShell(target)),
            ("SourceDirectory", sourceDirectory),
            ("BuildDirectory", buildDirectory),
            ("CMakeOptions", CommandText.PowerShellArgs(platform.CMakeOptions)),
            ("CMakeOutputFile", CommandText.PowerShellQuote(library.BuildFile(target, cmakeOutput))),
            ("OutputFile", CommandText.PowerShellQuote(library.OutputFile(target))));
    }

    /// <summary>Generates the Visual Studio developer shell setup fragment.</summary>
    public static string VisualStudioDevShell(TargetRid target)
    {
        var toolset = target.Architecture == TargetArchitecture.Arm64
            ? "Microsoft.VisualStudio.Component.VC.Tools.ARM64"
            : "Microsoft.VisualStudio.Component.VC.Tools.x86.x64";
        return TemplateResource.Render(
            "res/templates/native-build/windows/dev-shell.ps1.tmpl",
            ("Toolset", CommandText.PowerShellQuote(toolset)),
            ("WindowsArchitecture", target.WindowsArchitecture),
            ("VisualStudioArchitecture", CommandText.PowerShellQuote(target.VisualStudioArchitecture)));
    }
}
