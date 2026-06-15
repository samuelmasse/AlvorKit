namespace AlvorKit.Script.Bindgen;

/// <summary>Formats attributes shared by generated API, backend, wrapper, and noop methods.</summary>
internal static class BindingMethodAttributes
{
    /// <summary>Returns attribute source that de-emphasizes advanced native-shaped methods in IntelliSense.</summary>
    public static string ForFunction(BindingFunction function) =>
        function.IsAdvanced
            ? "    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]"
                + Environment.NewLine
            : "";
}
