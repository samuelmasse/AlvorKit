[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$WindowTitle,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [double]$DelaySeconds = 2,

    [int]$TimeoutSeconds = 15,

    [string]$FilePath,

    [string[]]$ArgumentList = @(),

    [switch]$ExactTitle,

    [switch]$StopProcess
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

if (-not ("CaptureWindowNative" -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class CaptureWindowNative
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO info);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
}
"@
}

function Resolve-OutputPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Find-Window {
    param(
        [string]$Title,
        [bool]$RequireExactTitle
    )

    $script:foundWindow = [IntPtr]::Zero
    $callback = [CaptureWindowNative+EnumWindowsProc] {
        param([IntPtr]$window, [IntPtr]$state)

        if (-not [CaptureWindowNative]::IsWindowVisible($window)) {
            return $true
        }

        $titleBuffer = [System.Text.StringBuilder]::new(512)
        [void][CaptureWindowNative]::GetWindowText($window, $titleBuffer, $titleBuffer.Capacity)
        $candidate = $titleBuffer.ToString()
        $matches = if ($RequireExactTitle) {
            $candidate -eq $Title
        } else {
            $candidate.Contains($Title, [StringComparison]::OrdinalIgnoreCase)
        }

        if ($matches) {
            $script:foundWindow = $window
            return $false
        }

        return $true
    }

    [void][CaptureWindowNative]::EnumWindows($callback, [IntPtr]::Zero)
    return $script:foundWindow
}

function Wait-ForWindow {
    param(
        [string]$Title,
        [bool]$RequireExactTitle,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTimeOffset]::Now.AddSeconds($TimeoutSeconds)
    do {
        $window = Find-Window -Title $Title -RequireExactTitle $RequireExactTitle
        if ($window -ne [IntPtr]::Zero) {
            return $window
        }

        Start-Sleep -Milliseconds 100
    } while ([DateTimeOffset]::Now -lt $deadline)

    throw "Timed out waiting for a visible window matching '$Title'."
}

function Move-WindowIntoView {
    param([IntPtr]$Window)

    $rect = [CaptureWindowNative+RECT]::new()
    if (-not [CaptureWindowNative]::GetWindowRect($Window, [ref]$rect)) {
        throw "Could not read the target window bounds."
    }

    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    $monitor = [CaptureWindowNative]::MonitorFromWindow($Window, 2)
    $monitorInfo = [CaptureWindowNative+MONITORINFO]::new()
    $monitorInfo.cbSize = [Runtime.InteropServices.Marshal]::SizeOf($monitorInfo)
    if (-not [CaptureWindowNative]::GetMonitorInfo($monitor, [ref]$monitorInfo)) {
        throw "Could not read the target monitor bounds."
    }

    $x = $monitorInfo.rcWork.Left + 24
    $y = $monitorInfo.rcWork.Top + 24
    [void][CaptureWindowNative]::SetWindowPos($Window, [IntPtr](-1), $x, $y, $width, $height, 0x0040)
    [void][CaptureWindowNative]::SetForegroundWindow($Window)
}

function Save-WindowBitmap {
    param(
        [IntPtr]$Window,
        [string]$Path
    )

    $rect = [CaptureWindowNative+RECT]::new()
    if (-not [CaptureWindowNative]::GetWindowRect($Window, [ref]$rect)) {
        throw "Could not read the target window bounds."
    }

    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    if ($width -le 0 -or $height -le 0) {
        throw "The target window has invalid bounds: ${width}x${height}."
    }

    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $size = [System.Drawing.Size]::new($width, $height)
        $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $size)
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

[void][CaptureWindowNative]::SetProcessDPIAware()
$resolvedOutputPath = Resolve-OutputPath -Path $OutputPath
$outputDirectory = Split-Path -Parent $resolvedOutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$startedProcess = $null
$window = [IntPtr]::Zero
try {
    if ($FilePath) {
        $startedProcess = Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -PassThru
    }

    $window = Wait-ForWindow -Title $WindowTitle -RequireExactTitle $ExactTitle.IsPresent -TimeoutSeconds $TimeoutSeconds
    [void][CaptureWindowNative]::ShowWindow($window, 9)
    Move-WindowIntoView -Window $window
    Start-Sleep -Milliseconds ([int]($DelaySeconds * 1000))
    Save-WindowBitmap -Window $window -Path $resolvedOutputPath
    Write-Output "Captured '$WindowTitle' to $resolvedOutputPath"
} finally {
    if ($window -and $window -ne [IntPtr]::Zero) {
        [void][CaptureWindowNative]::SetWindowPos($window, [IntPtr](-2), 0, 0, 0, 0, 0x0013)
    }

    if ($StopProcess -and $startedProcess -and -not $startedProcess.HasExited) {
        Stop-Process -Id $startedProcess.Id -Force
        Wait-Process -Id $startedProcess.Id -ErrorAction SilentlyContinue
    }
}
