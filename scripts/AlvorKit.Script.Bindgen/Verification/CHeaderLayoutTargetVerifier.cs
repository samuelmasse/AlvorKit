namespace AlvorKit.Script.Bindgen;

/// <summary>Validates that generated C header structs keep natural layout across supported targets.</summary>
internal static class CHeaderLayoutTargetVerifier
{
    /// <summary>Additional Clang targets used to catch platform-sensitive layout issues.</summary>
    private static readonly string[] AdditionalTargets =
    [
        "i686-pc-windows-msvc",
        "armv7-unknown-linux-gnueabihf",
        "aarch64-unknown-linux-gnu"
    ];

    /// <summary>Validates natural layout for every target used by the binding generator.</summary>
    public static void ValidateAllTargets(
        NativeLibraryBinding library,
        string translationUnitPath,
        BindingModel model)
    {
        var nativeStructNames = model.Structs.Select(structType => structType.NativeName).ToList();
        foreach (var target in AdditionalTargets)
        {
            CHeaderBindingParser.ValidateNaturalLayout(
                library.Config,
                translationUnitPath,
                library.IncludeDirectory,
                library.SourceDirectory,
                library.Directory,
                target,
                nativeStructNames);
        }

        Console.WriteLine($"Layout: natural on all {AdditionalTargets.Length + 1} validated targets");
    }
}
