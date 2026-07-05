namespace AlvorKit.Script.Bindgen;

/// <summary>Formats attributes shared by generated API, backend, wrapper, and noop methods.</summary>
internal static class BindingMethodAttributes
{
    /// <summary>Returns attribute source for primary generated methods: platform support plus advanced de-emphasis.</summary>
    public static string ForFunction(BindingFunction function) => Platform(function) + Advanced(function);

    /// <summary>Returns platform-support attribute source alone, for convenience overloads and raw native imports.</summary>
    public static string PlatformOnly(BindingFunction function) => Platform(function);

    /// <summary>Returns the platform-support attribute line when the function is platform-specific.</summary>
    private static string Platform(BindingFunction function) =>
        function.Platform is { } platform
            ? $"    [global::System.Runtime.Versioning.SupportedOSPlatform(\"{platform}\")]" + Environment.NewLine
            : "";

    /// <summary>Returns attribute source that de-emphasizes advanced native-shaped methods in IntelliSense.</summary>
    private static string Advanced(BindingFunction function) =>
        function.IsAdvanced
            ? "    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]"
                + Environment.NewLine
            : "";
}
