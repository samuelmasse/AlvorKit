namespace AlvorKit.Engine.Loop;

/// <summary>Tries to load vendor GPU libraries so Windows laptops prefer the dedicated graphics device.</summary>
[ExcludeFromCodeCoverage(Justification = "Probes optional native GPU vendor libraries through OS loader state.")]
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
                if (System.Runtime.InteropServices.NativeLibrary.TryLoad(library, out _))
                    return;
            }
            catch
            {
            }
        }
    }
}
