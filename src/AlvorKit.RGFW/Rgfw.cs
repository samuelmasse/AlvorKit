namespace AlvorKit.RGFW;

/// <summary>Raw bindings over the RGFW shared library (AlvorKit.RGFW.Native).</summary>
public static partial class Rgfw
{
    private const string Lib = "RGFW";

    public const int EventNoWait = 0;
    public const int EventWaitNext = -1;

    [LibraryImport(Lib, EntryPoint = "RGFW_createWindow", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint CreateWindow(string name, int x, int y, int w, int h, RgfwWindowFlags flags);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_close")]
    public static partial void WindowClose(nint window);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_shouldClose")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool WindowShouldClose(nint window);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_setShouldClose")]
    public static partial void WindowSetShouldClose(nint window, [MarshalAs(UnmanagedType.U1)] bool shouldClose);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_checkEvent")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool WindowCheckEvent(nint window, out RgfwEvent ev);

    [LibraryImport(Lib, EntryPoint = "RGFW_pollEvents")]
    public static partial void PollEvents();

    [LibraryImport(Lib, EntryPoint = "RGFW_waitForEvent")]
    public static partial void WaitForEvent(int waitMs);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_getSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool WindowGetSize(nint window, out int w, out int h);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_getPosition")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool WindowGetPosition(nint window, out int x, out int y);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_setName", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void WindowSetName(nint window, string name);

    [LibraryImport(Lib, EntryPoint = "RGFW_window_setExitKey")]
    public static partial void WindowSetExitKey(nint window, RgfwKey key);
}
