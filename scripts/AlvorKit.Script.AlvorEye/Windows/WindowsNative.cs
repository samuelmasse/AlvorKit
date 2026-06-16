namespace AlvorKit.Script.AlvorEye;

/// <summary>Win32 imports used by the Windows AlvorEye adapter.</summary>
[ExcludeFromCodeCoverage]
internal static partial class WindowsNative
{
    /// <summary>Callback used by <see cref="EnumWindows"/>.</summary>
    public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    /// <summary>Structure containing native rectangle coordinates.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        /// <summary>Left coordinate.</summary>
        public int Left;

        /// <summary>Top coordinate.</summary>
        public int Top;

        /// <summary>Right coordinate.</summary>
        public int Right;

        /// <summary>Bottom coordinate.</summary>
        public int Bottom;
    }

    /// <summary>Structure containing monitor bounds.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfo
    {
        /// <summary>Structure size in bytes.</summary>
        public int Size;

        /// <summary>Full monitor rectangle.</summary>
        public Rect Monitor;

        /// <summary>Work-area rectangle.</summary>
        public Rect Work;

        /// <summary>Monitor flags.</summary>
        public uint Flags;
    }

    /// <summary>Enumerates top-level windows.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnumWindows(EnumWindowsProc callback, nint lParam);

    /// <summary>Reads a window title.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(nint hWnd, StringBuilder text, int maxCount);

    /// <summary>Checks whether a window is visible.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsWindowVisible(nint hWnd);

    /// <summary>Reads the owning process id for a window.</summary>
    [LibraryImport("user32.dll")]
    public static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    /// <summary>Reads a window rectangle.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(nint hWnd, out Rect rect);

    /// <summary>Shows or restores a window.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int command);

    /// <summary>Brings a window to the foreground.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(nint hWnd);

    /// <summary>Moves, resizes, or reorders a window.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(nint hWnd, nint insertAfter, int x, int y, int cx, int cy, uint flags);

    /// <summary>Makes the current process DPI-aware.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetProcessDPIAware();

    /// <summary>Finds the nearest monitor for a window.</summary>
    [LibraryImport("user32.dll")]
    public static partial nint MonitorFromWindow(nint hWnd, uint flags);

    /// <summary>Reads monitor bounds.</summary>
    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    /// <summary>Moves the cursor to an absolute screen position.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetCursorPos(int x, int y);
}
