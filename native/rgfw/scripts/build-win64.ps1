# Builds the RGFW win-x64 binary for AlvorKit.RGFW.Native, at the tag pinned in
# native/rgfw/TAG. Requires Visual Studio with the C++ toolset.
# Output: native/rgfw/runtimes/win-x64/native/RGFW.dll

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Version = (Get-Content "$ScriptDir\..\TAG" -Raw).Trim()
$ImplFile = (Resolve-Path "$ScriptDir\..\rgfw.c").Path
$WorkDir = "$env:USERPROFILE\rgfw-build"
$SrcDir = "$WorkDir\RGFW-$Version"
$OutDir = "$ScriptDir\..\runtimes\win-x64\native"

# Enter the MSVC x64 dev environment (vswhere on PATH keeps Launch-VsDevShell quiet).
$env:PATH = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer;$env:PATH"
$VsPath = & vswhere.exe -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $VsPath) { throw 'No Visual Studio with the C++ toolset found.' }
& "$VsPath\Common7\Tools\Launch-VsDevShell.ps1" -Arch amd64 -SkipAutomaticLocation | Out-Null

# Fetch the pinned source.
New-Item -ItemType Directory -Force $WorkDir | Out-Null
$OutDir = (New-Item -ItemType Directory -Force $OutDir).FullName
Set-Location $WorkDir
if (-not (Test-Path $SrcDir)) {
    curl.exe -fsSL -o "RGFW-$Version.tar.gz" "https://github.com/ColleagueRiley/RGFW/archive/refs/tags/$Version.tar.gz"
    tar.exe xf "RGFW-$Version.tar.gz"
}

# Build. Link line per upstream's Makefile; /MT so the DLL needs no VC++ Redistributable.
# Only the DLL ships — the import .lib/.exp stay in the build dir.
Set-Location (New-Item -ItemType Directory -Force "$WorkDir\build-win64")
cl /nologo /O2 /MT /LD /I $SrcDir /Fe:RGFW.dll $ImplFile gdi32.lib user32.lib shell32.lib opengl32.lib winmm.lib
if ($LASTEXITCODE -ne 0) { throw "cl failed with exit code $LASTEXITCODE." }
Copy-Item RGFW.dll "$OutDir\RGFW.dll" -Force

# Verify: imports must be Windows system DLLs only.
$Deps = & dumpbin /nologo /dependents "$OutDir\RGFW.dll" | Select-String '\.dll'
$Deps | ForEach-Object { Write-Host $_.Line.Trim() }
if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime — /MT did not take.' }
Write-Host "OK $OutDir\RGFW.dll"
