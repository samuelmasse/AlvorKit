[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $LibraryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$expectedExports = @(
    "DllCanUnloadNow",
    "DllGetClassObject",
    "alvorkit_interception_enqueue_generation",
    "alvorkit_interception_enqueue_install",
    "alvorkit_interception_enqueue_install_dispatch",
    "alvorkit_interception_enqueue_remove",
    "alvorkit_interception_get_abi_version",
    "alvorkit_interception_get_capabilities",
    "alvorkit_interception_get_completion",
    "alvorkit_interception_get_generation_completion",
    "alvorkit_interception_get_loaded_method_body",
    "alvorkit_interception_get_profiler_state",
    "alvorkit_interception_get_relocation_result"
)
$allowedDependencies = @(
    "KERNEL32.dll",
    "OLE32.dll"
)

function Enter-VisualStudioDevShell
{
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path -LiteralPath $vswhere))
    {
        throw "vswhere.exe was not found."
    }

    $installation = & $vswhere `
        -latest `
        -products * `
        -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
        -property installationPath
    if ([string]::IsNullOrWhiteSpace($installation))
    {
        throw "No Visual Studio installation with the x64 C++ toolset was found."
    }

    $devShell = Join-Path $installation "Common7\Tools\Launch-VsDevShell.ps1"
    & $devShell -Arch amd64 -SkipAutomaticLocation | Out-Null
}

function Read-DumpbinExports([string[]] $lines)
{
    $exports = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines)
    {
        if ($line -match "^\s+\d+\s+[0-9A-Fa-f]+\s+[0-9A-Fa-f]+\s+(\S+)\s*$")
        {
            $exports.Add($Matches[1])
        }
    }
    return [string[]] $exports
}

function Read-DumpbinDependencies([string[]] $lines)
{
    $dependencies = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines)
    {
        if ($line -match "^\s+([A-Za-z0-9._-]+\.dll)\s*$")
        {
            $dependencies.Add($Matches[1])
        }
    }
    return [string[]] $dependencies
}

$resolvedLibrary = [IO.Path]::GetFullPath($LibraryPath)
if (-not (Test-Path -LiteralPath $resolvedLibrary -PathType Leaf))
{
    throw "Profiler DLL was not found: $resolvedLibrary"
}

Enter-VisualStudioDevShell

$headers = [string[]] (& dumpbin /nologo /headers $resolvedLibrary)
if ($LASTEXITCODE -ne 0)
{
    throw "dumpbin /headers failed."
}
if (-not ($headers -match "8664 machine \(x64\)"))
{
    throw "Profiler DLL is not a Windows x64 PE image."
}

$exportOutput = [string[]] (& dumpbin /nologo /exports $resolvedLibrary)
if ($LASTEXITCODE -ne 0)
{
    throw "dumpbin /exports failed."
}
$actualExports = @(Read-DumpbinExports $exportOutput | Sort-Object -Unique)
$missingExports = @($expectedExports | Where-Object { $_ -notin $actualExports })
$unexpectedExports = @($actualExports | Where-Object { $_ -notin $expectedExports })
if ($missingExports.Count -gt 0)
{
    throw "Profiler DLL is missing exports: $($missingExports -join ', ')"
}
if ($unexpectedExports.Count -gt 0)
{
    throw "Profiler DLL has unexpected exports: $($unexpectedExports -join ', ')"
}

$dependencyOutput = [string[]] (& dumpbin /nologo /dependents $resolvedLibrary)
if ($LASTEXITCODE -ne 0)
{
    throw "dumpbin /dependents failed."
}
$actualDependencies = @(Read-DumpbinDependencies $dependencyOutput | Sort-Object -Unique)
$unexpectedDependencies = @($actualDependencies | Where-Object { $_ -notin $allowedDependencies })
if ($unexpectedDependencies.Count -gt 0)
{
    throw "Profiler DLL has unexpected dependencies: $($unexpectedDependencies -join ', ')"
}

$verifierSource = @"
using System;
using System.Runtime.InteropServices;

public static class AlvorKitProfilerArtifact
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint GetAbiVersion();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryW(string path);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr library, string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr library);

    public static uint ReadAbiVersion(string path)
    {
        IntPtr library = LoadLibraryW(path);
        if (library == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "LoadLibraryW failed with error " + Marshal.GetLastWin32Error());
        }

        try
        {
            IntPtr address = GetProcAddress(
                library,
                "alvorkit_interception_get_abi_version");
            if (address == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "GetProcAddress failed with error " + Marshal.GetLastWin32Error());
            }
            return Marshal.GetDelegateForFunctionPointer<GetAbiVersion>(address)();
        }
        finally
        {
            FreeLibrary(library);
        }
    }
}
"@
Add-Type -TypeDefinition $verifierSource
$abiVersion = [AlvorKitProfilerArtifact]::ReadAbiVersion($resolvedLibrary)
if ($abiVersion -ne 3)
{
    throw "Profiler DLL reports ABI version $abiVersion instead of 3."
}

Write-Output "Profiler artifact verified:"
Write-Output "  path: $resolvedLibrary"
Write-Output "  machine: x64"
Write-Output "  ABI: $abiVersion"
Write-Output "  exports: $($actualExports.Count)"
Write-Output "  dependencies: $($actualDependencies -join ', ')"
