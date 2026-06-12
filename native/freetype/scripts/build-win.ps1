# Builds the FreeType win-x64 binary for AlvorKit.FreeType.Native, at the
# version pinned in native/freetype/TAG (upstream tag VER-x-y-z). Requires
# Visual Studio with the C++ toolset and CMake.
# Output: native/freetype/runtimes/win-<arch>/native/freetype.dll

param([ValidateSet('x64', 'x86', 'arm64')][string]$Arch = 'x64')

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Version = (Get-Content "$ScriptDir\..\TAG" -Raw).Trim()
$UpstreamTag = 'VER-' + $Version.Replace('.', '-')
$WorkDir = "$env:USERPROFILE\freetype-build"
$SrcDir = "$WorkDir\freetype-$UpstreamTag"
$OutDir = "$ScriptDir\..\runtimes\win-$Arch\native"

# Enter the MSVC x64 dev environment (vswhere on PATH keeps Launch-VsDevShell quiet).
$env:PATH = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer;$env:PATH"
$VcToolset = if ($Arch -eq 'arm64') { 'Microsoft.VisualStudio.Component.VC.Tools.ARM64' } else { 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' }
$DevArch = switch ($Arch) { 'x64' { 'amd64' } 'x86' { 'x86' } 'arm64' { 'arm64' } }
$VsPath = & vswhere.exe -latest -products * -requires $VcToolset -property installationPath
if (-not $VsPath) { throw "No Visual Studio with the $Arch C++ toolset found." }
& "$VsPath\Common7\Tools\Launch-VsDevShell.ps1" -Arch $DevArch -SkipAutomaticLocation | Out-Null

# Fetch the pinned source from upstream GitLab.
New-Item -ItemType Directory -Force $WorkDir | Out-Null
$OutDir = (New-Item -ItemType Directory -Force $OutDir).FullName
Set-Location $WorkDir
if (-not (Test-Path $SrcDir)) {
    curl.exe -fsSL -o "freetype-$UpstreamTag.tar.gz" "https://gitlab.freedesktop.org/freetype/freetype/-/archive/$UpstreamTag/freetype-$UpstreamTag.tar.gz"
    tar.exe xf "freetype-$UpstreamTag.tar.gz"
}

# Build. Internal zlib, optional deps off, static CRT so the DLL needs no
# VC++ Redistributable. Only the DLL ships.
# NMake generator: nmake ships with the VC++ toolset itself, so this works on
# any Visual Studio version — no VS-generator version coupling.
cmake --fresh -S $SrcDir -B "$WorkDir\build-win-$Arch" -G 'NMake Makefiles' `
    -DCMAKE_BUILD_TYPE=Release `
    -DCMAKE_POLICY_DEFAULT_CMP0091=NEW `
    -DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded `
    -DBUILD_SHARED_LIBS=ON `
    -DFT_DISABLE_ZLIB=ON -DFT_DISABLE_BZIP2=ON -DFT_DISABLE_PNG=ON `
    -DFT_DISABLE_HARFBUZZ=ON -DFT_DISABLE_BROTLI=ON
if ($LASTEXITCODE -ne 0) { throw 'cmake configure failed.' }
cmake --build "$WorkDir\build-win-$Arch"
if ($LASTEXITCODE -ne 0) { throw 'cmake build failed.' }
Copy-Item "$WorkDir\build-win-$Arch\freetype.dll" "$OutDir\freetype.dll" -Force

# Verify: imports must be Windows system DLLs only.
$Deps = & dumpbin /nologo /dependents "$OutDir\freetype.dll" | Select-String '\.dll'
$Deps | ForEach-Object { Write-Host $_.Line.Trim() }
if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime — static CRT did not take.' }
Write-Host "OK $OutDir\freetype.dll"
