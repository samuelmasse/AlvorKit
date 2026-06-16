namespace AlvorKit.Script.AlvorEye;

/// <summary>Win32 thread enumeration and suspension imports.</summary>
[ExcludeFromCodeCoverage]
internal static partial class WindowsThreadNative
{
    /// <summary>Thread snapshot entry structure.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadEntry32
    {
        /// <summary>Structure size in bytes.</summary>
        public uint Size;

        /// <summary>Unused reference count.</summary>
        public uint Usage;

        /// <summary>Thread id.</summary>
        public uint ThreadId;

        /// <summary>Owning process id.</summary>
        public uint OwnerProcessId;

        /// <summary>Base priority.</summary>
        public int BasePriority;

        /// <summary>Delta priority.</summary>
        public int DeltaPriority;

        /// <summary>Thread flags.</summary>
        public uint Flags;
    }

    /// <summary>Creates a Toolhelp snapshot.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint CreateToolhelp32Snapshot(uint flags, uint processId);

    /// <summary>Reads the first thread entry.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool Thread32First(nint snapshot, ref ThreadEntry32 entry);

    /// <summary>Reads the next thread entry.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool Thread32Next(nint snapshot, ref ThreadEntry32 entry);

    /// <summary>Opens a thread handle.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint OpenThread(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint threadId);

    /// <summary>Suspends a thread.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint SuspendThread(nint thread);

    /// <summary>Resumes a thread.</summary>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint ResumeThread(nint thread);

    /// <summary>Closes a native handle.</summary>
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(nint handle);
}
