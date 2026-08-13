[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $LibraryPath,

    [Parameter(Mandatory)]
    [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-arm64")]
    [string] $RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$expectedExports = @(
    "DllCanUnloadNow",
    "DllGetClassObject",
    "alvorkit_interception_begin_allocation_capture",
    "alvorkit_interception_end_allocation_capture",
    "alvorkit_interception_enqueue_generation",
    "alvorkit_interception_enqueue_install",
    "alvorkit_interception_enqueue_install_dispatch",
    "alvorkit_interception_enqueue_remove",
    "alvorkit_interception_get_abi_version",
    "alvorkit_interception_get_allocation_sample",
    "alvorkit_interception_get_capabilities",
    "alvorkit_interception_get_completion",
    "alvorkit_interception_get_generation_completion",
    "alvorkit_interception_get_loaded_method_body",
    "alvorkit_interception_get_profiler_state",
    "alvorkit_interception_get_relocation_result",
    "alvorkit_interception_resolve_allocation_frame"
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

function Read-NmExports([string[]] $lines)
{
    $exports = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines)
    {
        if ($line -match "^(\S+)\s+[A-Za-z]\s+")
        {
            $exports.Add($Matches[1])
        }
    }
    return [string[]] $exports
}

function Read-ElfDependencies([string[]] $lines)
{
    $dependencies = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines)
    {
        if ($line -match "\(NEEDED\).*\[([^\]]+)\]")
        {
            $dependencies.Add($Matches[1])
        }
    }
    return [string[]] $dependencies
}

function Read-MacExports([string[]] $lines)
{
    $exports = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines)
    {
        $name = $line.Trim()
        if ($name.StartsWith("_", [StringComparison]::Ordinal))
        {
            $exports.Add($name.Substring(1))
        }
    }
    return [string[]] $exports
}

function Read-MacDependencies([string[]] $lines)
{
    $dependencies = [Collections.Generic.List[string]]::new()
    $readingLoadCommand = $false
    foreach ($line in $lines)
    {
        if ($line -match "^\s+cmd LC_(LOAD|LOAD_WEAK|REEXPORT|LOAD_UPWARD)_DYLIB\s*$")
        {
            $readingLoadCommand = $true
            continue
        }
        if ($readingLoadCommand -and $line -match "^\s+name (\S+) \(offset \d+\)\s*$")
        {
            $dependencies.Add($Matches[1])
            $readingLoadCommand = $false
        }
    }
    return [string[]] $dependencies
}

function Assert-ExactValues(
    [string] $kind,
    [string[]] $expected,
    [string[]] $actual)
{
    $missing = @($expected | Where-Object { $_ -notin $actual })
    $unexpected = @($actual | Where-Object { $_ -notin $expected })
    if ($missing.Count -gt 0)
    {
        throw "Profiler is missing ${kind}: $($missing -join ', ')"
    }
    if ($unexpected.Count -gt 0)
    {
        throw "Profiler has unexpected ${kind}: $($unexpected -join ', ')"
    }
}

function Assert-AllowedValues(
    [string] $kind,
    [string[]] $allowed,
    [string[]] $actual)
{
    $unexpected = @($actual | Where-Object { $_ -notin $allowed })
    if ($unexpected.Count -gt 0)
    {
        throw "Profiler has unexpected ${kind}: $($unexpected -join ', ')"
    }
}

function Read-WindowsArtifact([string] $path)
{
    Enter-VisualStudioDevShell

    $headers = [string[]] (& dumpbin /nologo /headers $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "dumpbin /headers failed."
    }
    if (-not ($headers -match "8664 machine \(x64\)"))
    {
        throw "Profiler DLL is not a Windows x64 PE image."
    }

    $exportOutput = [string[]] (& dumpbin /nologo /exports $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "dumpbin /exports failed."
    }
    $exports = @(Read-DumpbinExports $exportOutput | Sort-Object -Unique)

    $dependencyOutput = [string[]] (& dumpbin /nologo /dependents $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "dumpbin /dependents failed."
    }
    $dependencies = @(Read-DumpbinDependencies $dependencyOutput | Sort-Object -Unique)
    $allowed = @("KERNEL32.dll", "OLE32.dll")
    Assert-ExactValues "exports" $expectedExports $exports
    Assert-AllowedValues "dependencies" $allowed $dependencies

    return [pscustomobject] @{
        Machine = "Windows x64"
        Exports = $exports
        Dependencies = $dependencies
    }
}

function Read-LinuxArtifact(
    [string] $path,
    [string] $runtimeIdentifier)
{
    $expectedMachine = switch ($runtimeIdentifier)
    {
        "linux-x64" { "Advanced Micro Devices X86-64" }
        "linux-arm64" { "AArch64" }
        default { throw "Unsupported Linux profiler RID: $runtimeIdentifier" }
    }
    $machineLabel = switch ($runtimeIdentifier)
    {
        "linux-x64" { "Linux x64" }
        "linux-arm64" { "Linux Arm64" }
    }
    $headers = [string[]] (& readelf -h $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "readelf -h failed."
    }
    if (-not ($headers -match "Class:\s+ELF64") -or
        -not ($headers -match "Machine:\s+$([regex]::Escape($expectedMachine))"))
    {
        throw "Profiler shared library is not a $machineLabel ELF image."
    }

    $exportOutput = [string[]] (& nm --dynamic --defined-only --format=posix $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "nm export inspection failed."
    }
    $exports = @(Read-NmExports $exportOutput | Sort-Object -Unique)

    $dependencyOutput = [string[]] (& readelf -d $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "readelf -d failed."
    }
    $dependencies = @(Read-ElfDependencies $dependencyOutput | Sort-Object -Unique)
    $allowed = @(
        "libc.so.6",
        "libdl.so.2",
        "libgcc_s.so.1",
        "libm.so.6",
        "libpthread.so.0",
        "libstdc++.so.6"
    )
    if ($runtimeIdentifier -eq "linux-arm64")
    {
        $allowed += "ld-linux-aarch64.so.1"
    }
    Assert-ExactValues "exports" $expectedExports $exports
    Assert-AllowedValues "dependencies" $allowed $dependencies

    return [pscustomobject] @{
        Machine = $machineLabel
        Exports = $exports
        Dependencies = $dependencies
    }
}

function Read-MacArtifact([string] $path)
{
    $architectures = [string[]] (& lipo -archs $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "lipo architecture inspection failed."
    }
    if (($architectures -join " ").Trim() -ne "arm64")
    {
        throw "Profiler dynamic library is not an exact macOS Arm64 image."
    }

    $fileOutput = [string[]] (& file $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "file architecture inspection failed."
    }
    if (-not ($fileOutput -match "Mach-O 64-bit dynamically linked shared library arm64"))
    {
        throw "Profiler dynamic library is not a 64-bit macOS Arm64 Mach-O shared library."
    }

    $exportOutput = [string[]] (& nm -gUj $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "nm export inspection failed."
    }
    $exports = @(Read-MacExports $exportOutput | Sort-Object -Unique)

    $dependencyOutput = [string[]] (& otool -l $path)
    if ($LASTEXITCODE -ne 0)
    {
        throw "otool dependency inspection failed."
    }
    $dependencies = @(Read-MacDependencies $dependencyOutput | Sort-Object -Unique)
    $allowed = @(
        "/usr/lib/libSystem.B.dylib",
        "/usr/lib/libc++.1.dylib"
    )
    Assert-ExactValues "exports" $expectedExports $exports
    Assert-AllowedValues "dependencies" $allowed $dependencies

    return [pscustomobject] @{
        Machine = "macOS Arm64"
        Exports = $exports
        Dependencies = $dependencies
    }
}

$resolvedLibrary = [IO.Path]::GetFullPath($LibraryPath)
if (-not (Test-Path -LiteralPath $resolvedLibrary -PathType Leaf))
{
    throw "Profiler library was not found: $resolvedLibrary"
}

$artifact = switch ($RuntimeIdentifier)
{
    "win-x64" { Read-WindowsArtifact $resolvedLibrary }
    "osx-arm64" { Read-MacArtifact $resolvedLibrary }
    default { Read-LinuxArtifact $resolvedLibrary $RuntimeIdentifier }
}

$loaderSource = if ($RuntimeIdentifier -eq "win-x64")
{
@"
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
            throw new InvalidOperationException(
                "LoadLibraryW failed with error " + Marshal.GetLastWin32Error());

        try
        {
            IntPtr address = GetProcAddress(
                library,
                "alvorkit_interception_get_abi_version");
            if (address == IntPtr.Zero)
                throw new InvalidOperationException(
                    "GetProcAddress failed with error " + Marshal.GetLastWin32Error());
            return Marshal.GetDelegateForFunctionPointer<GetAbiVersion>(address)();
        }
        finally
        {
            FreeLibrary(library);
        }
    }
"@
}
elseif ($RuntimeIdentifier -eq "osx-arm64")
{
@"
    private const int RtldNow = 2;

    [DllImport("libSystem.B.dylib", CharSet = CharSet.Ansi)]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("libSystem.B.dylib", CharSet = CharSet.Ansi)]
    private static extern IntPtr dlsym(IntPtr library, string name);

    [DllImport("libSystem.B.dylib")]
    private static extern int dlclose(IntPtr library);

    [DllImport("libSystem.B.dylib")]
    private static extern IntPtr dlerror();

    public static uint ReadAbiVersion(string path)
    {
        IntPtr library = dlopen(path, RtldNow);
        if (library == IntPtr.Zero)
            throw new InvalidOperationException(
                "dlopen failed: " + Marshal.PtrToStringAnsi(dlerror()));

        try
        {
            IntPtr address = dlsym(
                library,
                "alvorkit_interception_get_abi_version");
            if (address == IntPtr.Zero)
                throw new InvalidOperationException(
                    "dlsym failed: " + Marshal.PtrToStringAnsi(dlerror()));
            return Marshal.GetDelegateForFunctionPointer<GetAbiVersion>(address)();
        }
        finally
        {
            dlclose(library);
        }
    }
"@
}
else
{
@"
    private const int RtldNow = 2;

    [DllImport("libdl.so.2", CharSet = CharSet.Ansi)]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("libdl.so.2", CharSet = CharSet.Ansi)]
    private static extern IntPtr dlsym(IntPtr library, string name);

    [DllImport("libdl.so.2")]
    private static extern int dlclose(IntPtr library);

    [DllImport("libdl.so.2")]
    private static extern IntPtr dlerror();

    public static uint ReadAbiVersion(string path)
    {
        IntPtr library = dlopen(path, RtldNow);
        if (library == IntPtr.Zero)
            throw new InvalidOperationException(
                "dlopen failed: " + Marshal.PtrToStringAnsi(dlerror()));

        try
        {
            IntPtr address = dlsym(
                library,
                "alvorkit_interception_get_abi_version");
            if (address == IntPtr.Zero)
                throw new InvalidOperationException(
                    "dlsym failed: " + Marshal.PtrToStringAnsi(dlerror()));
            return Marshal.GetDelegateForFunctionPointer<GetAbiVersion>(address)();
        }
        finally
        {
            dlclose(library);
        }
    }
"@
}

$verifierSource = @"
using System;
using System.Runtime.InteropServices;

public static class AlvorKitProfilerArtifact
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint GetAbiVersion();

$loaderSource
}
"@
Add-Type -TypeDefinition $verifierSource
$abiVersion = [AlvorKitProfilerArtifact]::ReadAbiVersion($resolvedLibrary)
if ($abiVersion -ne 3)
{
    throw "Profiler library reports ABI version $abiVersion instead of 3."
}

Write-Output "Profiler artifact verified:"
Write-Output "  path: $resolvedLibrary"
Write-Output "  machine: $($artifact.Machine)"
Write-Output "  ABI: $abiVersion"
Write-Output "  exports: $($artifact.Exports.Count)"
Write-Output "  dependencies: $($artifact.Dependencies -join ', ')"
