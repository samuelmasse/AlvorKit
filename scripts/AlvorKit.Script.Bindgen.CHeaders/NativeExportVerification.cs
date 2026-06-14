namespace AlvorKit.Script.Bindgen;

/// <summary>Result of comparing generated binding functions with a native library's exports.</summary>
/// <param name="LibraryPath">Native library path that was checked.</param>
/// <param name="LibraryExists">Whether the native library exists.</param>
/// <param name="MissingFunctions">Generated functions missing from the native library.</param>
public sealed record NativeExportVerification(string LibraryPath, bool LibraryExists, List<BindingFunction> MissingFunctions)
{
    /// <summary>Whether the library exists and every generated function export was found.</summary>
    public bool AllExportsFound => LibraryExists && MissingFunctions.Count == 0;
}
