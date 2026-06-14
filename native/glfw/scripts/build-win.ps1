# Builds the GLFW win-x64 binary for AlvorKit.GLFW.Native, at the version pinned
# in native/glfw/TAG. Requires Visual Studio with the C++ toolset and CMake.
# Output: native/glfw/runtimes/win-<arch>/native/glfw3.dll

param([ValidateSet('x64', 'x86', 'arm64')][string]$Arch = 'x64')

$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Version = (Get-Content "$ScriptDir\..\TAG" -Raw).Trim()
$WorkDir = "$env:USERPROFILE\glfw-build"
$SrcDir = "$WorkDir\glfw-$Version"
$OutDir = "$ScriptDir\..\runtimes\win-$Arch\native"

# Enter the MSVC dev environment (vswhere on PATH keeps Launch-VsDevShell quiet).
$env:PATH = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer;$env:PATH"
$VcToolset = if ($Arch -eq 'arm64') { 'Microsoft.VisualStudio.Component.VC.Tools.ARM64' } else { 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' }
$DevArch = switch ($Arch) { 'x64' { 'amd64' } 'x86' { 'x86' } 'arm64' { 'arm64' } }
$VsPath = & vswhere.exe -latest -products * -requires $VcToolset -property installationPath
if (-not $VsPath) { throw "No Visual Studio with the $Arch C++ toolset found." }
& "$VsPath\Common7\Tools\Launch-VsDevShell.ps1" -Arch $DevArch -SkipAutomaticLocation | Out-Null

# Fetch the pinned source.
New-Item -ItemType Directory -Force $WorkDir | Out-Null
$OutDir = (New-Item -ItemType Directory -Force $OutDir).FullName
Set-Location $WorkDir
if (-not (Test-Path $SrcDir)) {
    curl.exe -fsSL -o "glfw-$Version.tar.gz" "https://github.com/glfw/glfw/archive/refs/tags/$Version.tar.gz"
    tar.exe xf "glfw-$Version.tar.gz"
}

# Build. Shared library, no examples/tests/docs, static CRT so the DLL needs no
# VC++ Redistributable. NMake generator: nmake ships with the VC++ toolset
# itself, so this works on any Visual Studio version - no VS-generator version
# coupling. Only the DLL ships - the import .lib/.exp stay in the build dir.
cmake --fresh -S $SrcDir -B "$WorkDir\build-win-$Arch" -G 'NMake Makefiles' `
    -DCMAKE_BUILD_TYPE=Release `
    -DCMAKE_POLICY_DEFAULT_CMP0091=NEW `
    -DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded `
    -DBUILD_SHARED_LIBS=ON `
    -DGLFW_BUILD_EXAMPLES=OFF -DGLFW_BUILD_TESTS=OFF -DGLFW_BUILD_DOCS=OFF
if ($LASTEXITCODE -ne 0) { throw 'cmake configure failed.' }
cmake --build "$WorkDir\build-win-$Arch"
if ($LASTEXITCODE -ne 0) { throw 'cmake build failed.' }
Copy-Item "$WorkDir\build-win-$Arch\src\glfw3.dll" "$OutDir\glfw3.dll" -Force

# Verify: imports must be Windows system DLLs only.
$Deps = & dumpbin /nologo /dependents "$OutDir\glfw3.dll" | Select-String '\.dll'
$Deps | ForEach-Object { Write-Host $_.Line.Trim() }
if ($Deps -match 'VCRUNTIME|MSVCP') { throw 'DLL depends on the VC++ runtime - static CRT did not take.' }
Write-Host "OK $OutDir\glfw3.dll"
