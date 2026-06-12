namespace AlvorKit.Script.Bindgen;

public sealed record NativeExportVerification(string LibraryPath, bool LibraryExists, List<BindingFunction> MissingFunctions)
{
    public bool AllExportsFound => LibraryExists && MissingFunctions.Count == 0;
}

public static class NativeExportVerifier
{
    public static NativeExportVerification Verify(string libraryPath, BindingModel model)
    {
        if (!File.Exists(libraryPath))
            return new(libraryPath, LibraryExists: false, MissingFunctions: []);

        var nativeLibrary = NativeLibrary.Load(libraryPath);
        try
        {
            var missing = model.Functions
                .Where(function => !NativeLibrary.TryGetExport(nativeLibrary, function.NativeName, out _))
                .ToList();
            return new(libraryPath, LibraryExists: true, missing);
        }
        finally
        {
            NativeLibrary.Free(nativeLibrary);
        }
    }
}
