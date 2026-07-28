[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Find-ClangDriver
{
    $pathCommand = Get-Command clang-cl.exe -ErrorAction SilentlyContinue
    if ($null -ne $pathCommand)
    {
        return $pathCommand.Source
    }

    $isWindows = [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [Runtime.InteropServices.OSPlatform]::Windows)
    if ($isWindows)
    {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (Test-Path -LiteralPath $vswhere)
        {
            $installations = & $vswhere -products * -requires Microsoft.VisualStudio.Component.VC.Llvm.Clang -property installationPath
            foreach ($installation in $installations)
            {
                $candidate = Join-Path $installation "VC\Tools\Llvm\x64\bin\clang-cl.exe"
                if (Test-Path -LiteralPath $candidate)
                {
                    return $candidate
                }
            }
        }

        $visualStudioRoot = Join-Path $env:ProgramFiles "Microsoft Visual Studio"
        if (Test-Path -LiteralPath $visualStudioRoot)
        {
            $candidate = Get-ChildItem -Path "$visualStudioRoot\*\*\VC\Tools\Llvm\x64\bin\clang-cl.exe" -File |
                Sort-Object FullName -Descending |
                Select-Object -First 1
            if ($null -ne $candidate)
            {
                return $candidate.FullName
            }
        }
    }

    $clangCommand = Get-Command clang++ -ErrorAction SilentlyContinue
    if ($null -ne $clangCommand)
    {
        return $clangCommand.Source
    }

    throw "clangd metadata needs clang-cl or clang++. Install the Visual Studio C++ Clang tools or put clang++ on PATH."
}

function Find-CoreClrSource([string] $repositoryRoot, [string] $tag)
{
    $candidates = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($env:ALVORKIT_CORECLR_SOURCE))
    {
        $candidates.Add($env:ALVORKIT_CORECLR_SOURCE)
    }

    $candidates.Add((Join-Path $repositoryRoot "out\upstream\dotnet-runtime"))
    $candidates.Add((Join-Path $repositoryRoot "out\upstream\dotnet-runtime-$tag"))
    $candidates.Add((Join-Path $repositoryRoot "out\upstream\dotnet-runtime-git-$tag"))
    foreach ($candidate in $candidates)
    {
        $corHeader = Join-Path $candidate "src\coreclr\inc\cor.h"
        $profilerHeader = Join-Path $candidate "src\coreclr\pal\prebuilt\inc\corprof.h"
        if ((Test-Path -LiteralPath $corHeader) -and (Test-Path -LiteralPath $profilerHeader))
        {
            return [IO.Path]::GetFullPath($candidate)
        }
    }

    throw "CoreCLR $tag headers were not found. Set ALVORKIT_CORECLR_SOURCE or populate out/upstream/dotnet-runtime-$tag."
}

function Find-WindowsSystemIncludes([string] $compiler)
{
    $llvmMarker = "\VC\Tools\Llvm\"
    $markerIndex = $compiler.IndexOf($llvmMarker, [StringComparison]::OrdinalIgnoreCase)
    if ($markerIndex -lt 0)
    {
        throw "Cannot derive the Visual Studio installation from $compiler."
    }

    $installation = $compiler.Substring(0, $markerIndex)
    $msvcInclude = Get-ChildItem -Path "$installation\VC\Tools\MSVC\*\include" -Directory -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1
    if ($null -eq $msvcInclude)
    {
        $visualStudioRoot = Join-Path $env:ProgramFiles "Microsoft Visual Studio"
        $msvcInclude = Get-ChildItem -Path "$visualStudioRoot\*\*\VC\Tools\MSVC\*\include" -Directory -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending |
            Select-Object -First 1
    }
    if ($null -eq $msvcInclude)
    {
        throw "No MSVC C++ headers were found in a Visual Studio installation."
    }

    $windowsSdkRoot = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\Include"
    $windowsSdkVersion = Get-ChildItem -LiteralPath $windowsSdkRoot -Directory |
        Sort-Object Name -Descending |
        Select-Object -First 1
    if ($null -eq $windowsSdkVersion)
    {
        throw "No Windows SDK headers were found under $windowsSdkRoot."
    }

    $candidates = @(
        $msvcInclude.FullName,
        (Join-Path $msvcInclude.Parent.FullName "atlmfc\include"),
        (Join-Path $windowsSdkVersion.FullName "ucrt"),
        (Join-Path $windowsSdkVersion.FullName "shared"),
        (Join-Path $windowsSdkVersion.FullName "um"),
        (Join-Path $windowsSdkVersion.FullName "winrt"),
        (Join-Path $windowsSdkVersion.FullName "cppwinrt"))
    return [string[]] ($candidates | Where-Object { Test-Path -LiteralPath $_ })
}

function Add-CompileEntry(
    [System.Collections.Generic.List[object]] $entries,
    [string] $repositoryRoot,
    [string] $compiler,
    [string] $source,
    [string[]] $arguments)
{
    $sourcePath = [IO.Path]::GetFullPath($source)
    $commandArguments = [System.Collections.Generic.List[string]]::new()
    $commandArguments.Add($compiler)
    $commandArguments.AddRange($arguments)
    $commandArguments.Add($sourcePath)
    $entries.Add(
        [ordered]@{
            directory = $repositoryRoot
            file = $sourcePath
            arguments = $commandArguments
        })
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$profilerRoot = Join-Path $repositoryRoot "native\interception-profiler"
$coreClrTag = (Get-Content -LiteralPath (Join-Path $profilerRoot "version\CORECLR_TAG") -Raw).Trim()
$coreClrSource = Find-CoreClrSource $repositoryRoot $coreClrTag
$compiler = Find-ClangDriver
$usingClangCl = [IO.Path]::GetFileName($compiler).Equals("clang-cl.exe", [StringComparison]::OrdinalIgnoreCase)
$isLinux = [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [Runtime.InteropServices.OSPlatform]::Linux)
$includePrefix = if ($usingClangCl) { "/I" } else { "-I" }

$coreClrInclude = Join-Path $coreClrSource "src\coreclr\inc"
$coreClrProfilerInclude = Join-Path $coreClrSource "src\coreclr\pal\prebuilt\inc"
$coreClrPalInclude = Join-Path $coreClrSource "src\coreclr\pal\inc"
$coreClrPalRuntimeInclude = Join-Path $coreClrPalInclude "rt"
$coreClrNativeInclude = Join-Path $coreClrSource "src\native"
$profilerInclude = Join-Path $profilerRoot "include"

if ($usingClangCl)
{
    $systemArguments = [string[]] ((Find-WindowsSystemIncludes $compiler) | ForEach-Object { "/imsvc$_" })
    $cppArguments = [string[]] (@(
        "/nologo",
        "/std:c++20",
        "/EHsc",
        "/clang:-Wno-pragma-pack",
        "/DUNICODE",
        "/D_UNICODE",
        "/DNOMINMAX",
        "/I$profilerInclude",
        "/imsvc$coreClrInclude",
        "/imsvc$coreClrProfilerInclude") + $systemArguments)
    $cArguments = [string[]] (@("/nologo", "/TC", "/std:c17", "/D_CRT_SECURE_NO_WARNINGS") + $systemArguments)
}
elseif ($isLinux)
{
    $cppArguments = @(
        "-std=c++20",
        "-Wno-pragma-pack",
        "-DHOST_64BIT=1",
        "-I$profilerInclude",
        "-isystem$coreClrInclude",
        "-isystem$coreClrProfilerInclude",
        "-isystem$coreClrPalRuntimeInclude",
        "-isystem$coreClrPalInclude",
        "-isystem$coreClrNativeInclude")
    $cArguments = @("-x", "c", "-std=c17")
}
else
{
    throw "The profiler compile database supports clang-cl on Windows and clang++ on Linux."
}

$entries = [System.Collections.Generic.List[object]]::new()
Get-ChildItem -LiteralPath (Join-Path $profilerRoot "src") -Filter *.cpp | Sort-Object FullName | ForEach-Object {
    Add-CompileEntry $entries $repositoryRoot $compiler $_.FullName $cppArguments
}

$cSourcePatterns = @(
    "native\fastnoise2\verify\*.c",
    "native\miniaudio\src\*.c",
    "native\xxhash\src\*.c",
    "native\xxhash\verify\*.c")
foreach ($pattern in $cSourcePatterns)
{
    Get-ChildItem -Path (Join-Path $repositoryRoot $pattern) | Sort-Object FullName | ForEach-Object {
        $sourceArguments = [System.Collections.Generic.List[string]]::new()
        $sourceArguments.AddRange([string[]] $cArguments)
        if ($_.DirectoryName -like "*\native\miniaudio\src")
        {
            $tag = (Get-Content -LiteralPath (Join-Path $repositoryRoot "native\miniaudio\version\TAG") -Raw).Trim()
            $sourceArguments.Add("$includePrefix$(Join-Path $repositoryRoot "out\native-work\miniaudio-build\miniaudio-$tag")")
        }
        elseif ($_.DirectoryName -like "*\native\xxhash\src")
        {
            $tag = (Get-Content -LiteralPath (Join-Path $repositoryRoot "native\xxhash\version\TAG") -Raw).Trim()
            $sourceArguments.Add("$includePrefix$(Join-Path $repositoryRoot "out\native-work\xxhash-build\xxHash-$tag")")
        }

        Add-CompileEntry $entries $repositoryRoot $compiler $_.FullName $sourceArguments
    }
}

$outputDirectory = Join-Path $repositoryRoot "out\clangd"
[IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$outputPath = Join-Path $outputDirectory "compile_commands.json"
$json = ConvertTo-Json -InputObject ([object[]] $entries) -Depth 5
[IO.File]::WriteAllText($outputPath, $json, [Text.UTF8Encoding]::new($false))
Write-Output "Wrote $($entries.Count) clangd compile commands to $outputPath"
