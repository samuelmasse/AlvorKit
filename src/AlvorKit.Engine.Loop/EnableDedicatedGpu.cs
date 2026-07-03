namespace AlvorKit.Engine.Loop;

internal static class EnableDedicatedGpu
{
    /// <summary>Attempts the vendor-library probe on Windows and does nothing on other platforms.</summary>
    internal static void Run()
    {
        if (!OperatingSystem.IsWindows())
            return;

        TryAny(
            "nvapi64.dll",
            "nvapi.dll",
            "atiadlxx.dll",
            "atiadlxy.dll");
    }

    private static void TryAny(params string[] libraries)
    {
        foreach (var library in libraries)
        {
            try
            {
                if (NativeLibrary.TryLoad(library, out _))
                    return;
            }
            catch { }
        }
    }
}
