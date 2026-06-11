# Builds the miniaudio win-x64 binary for AlvorKit.MiniAudio.Native, at the tag
# pinned in native/miniaudio/TAG. Requires Visual Studio with the C++ toolset.
# Output: native/miniaudio/runtimes/win-x64/native/miniaudio.dll

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Version = (Get-Content "$ScriptDir\..\TAG" -Raw).Trim()
$ImplFile = (Resolve-Path "$ScriptDir\..\miniaudio.c").Path
$WorkDir = "$env:USERPROFILE\miniaudio-build"
$SrcDir = "$WorkDir\miniaudio-$Version"
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
    curl.exe -fsSL -o "miniaudio-$Version.tar.gz" "https://github.com/mackron/miniaudio/archive/refs/tags/$Version.tar.gz"
    tar.exe xf "miniaudio-$Version.tar.gz"
}

# Build. miniaudio loads its backends at runtime, so no extra link libraries;
# /MT so the DLL needs no VC++ Redistributable. Only the DLL ships.
Set-Location (New-Item -ItemType Directory -Force "$WorkDir\build-win64")
cl /nologo /O2 /MT /LD /I $SrcDir /Fe:miniaudio.dll $ImplFile
if ($LASTEXITCODE -ne 0) { throw "cl failed with exit code $LASTEXITCODE." }
Copy-Item miniaudio.dll "$OutDir\miniaudio.dll" -Force

# Verify: imports must be Windows system DLLs only.
$Deps = & dumpbin /nologo /dependents "$OutDir\miniaudio.dll" | Select-String '\.dll'
$Deps | ForEach-Object { Write-Host $_.Line.Trim() }
if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime — /MT did not take.' }
Write-Host "OK $OutDir\miniaudio.dll"
