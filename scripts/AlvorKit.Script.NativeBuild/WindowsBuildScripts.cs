namespace AlvorKit.Script.NativeBuild;

/// <summary>Generates Windows PowerShell build fragments for MSVC-based builds.</summary>
internal static class WindowsBuildScripts
{
    /// <summary>Generates the Windows direct C compiler PowerShell script.</summary>
    public static string SingleC(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var outputFile = library.OutputFile(target);
        return $$"""
            {{VisualStudioDevShell(target)}}
            New-Item -ItemType Directory -Force {{CommandText.PowerShellQuote(library.BuildDirectory(target))}} | Out-Null
            Set-Location {{CommandText.PowerShellQuote(library.BuildDirectory(target))}}
            & cl /nologo /O2 /MT /LD /I {{CommandText.PowerShellQuote(library.SourceDirectory)}} {{CommandText.PowerShellQuote("/Fe:" + outputFile)}} {{CommandText.PowerShellQuote(NativeBuildPlanner.RequiredImplFile(library))}} {{CommandText.PowerShellArgs(platform.LinkLibraries.Select(link => link + ".lib"))}}
            if ($LASTEXITCODE -ne 0) { throw "cl failed with exit code $LASTEXITCODE." }
            """;
    }

    /// <summary>Generates the Windows CMake PowerShell script.</summary>
    public static string CMake(LibraryBuildContext library, TargetRid target, PlatformBuildConfig platform)
    {
        var cmakeOutput = NativeBuildPlanner.RequiredCMakeOutput(library, platform);
        return $$"""
            {{VisualStudioDevShell(target)}}
            & cmake --fresh -S {{CommandText.PowerShellQuote(library.SourceDirectory)}} -B {{CommandText.PowerShellQuote(library.BuildDirectory(target))}} -G 'NMake Makefiles' `
                -DCMAKE_BUILD_TYPE=Release `
                -DCMAKE_POLICY_DEFAULT_CMP0091=NEW `
                -DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded `
                {{CommandText.PowerShellArgs(platform.CMakeOptions)}}
            if ($LASTEXITCODE -ne 0) { throw "cmake configure failed." }
            & cmake --build {{CommandText.PowerShellQuote(library.BuildDirectory(target))}}
            if ($LASTEXITCODE -ne 0) { throw "cmake build failed." }
            Copy-Item {{CommandText.PowerShellQuote(library.BuildFile(target, cmakeOutput))}} {{CommandText.PowerShellQuote(library.OutputFile(target))}} -Force
            """;
    }

    /// <summary>Generates the Visual Studio developer shell setup fragment.</summary>
    private static string VisualStudioDevShell(TargetRid target)
    {
        var toolset = target.Architecture == TargetArchitecture.Arm64
            ? "Microsoft.VisualStudio.Component.VC.Tools.ARM64"
            : "Microsoft.VisualStudio.Component.VC.Tools.x86.x64";
        return $$"""
            $env:PATH = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer;$env:PATH"
            $VsPath = & vswhere.exe -latest -products * -requires {{CommandText.PowerShellQuote(toolset)}} -property installationPath
            if (-not $VsPath) { throw "No Visual Studio with the {{target.WindowsArchitecture}} C++ toolset found." }
            & "$VsPath\Common7\Tools\Launch-VsDevShell.ps1" -Arch {{CommandText.PowerShellQuote(target.VisualStudioArchitecture)}} -SkipAutomaticLocation | Out-Null
            """;
    }
}
