namespace AlvorKit.Script.Bindgen;

/// <summary>Writes the temporary C translation unit passed to Clang.</summary>
[ExcludeFromCodeCoverage]
public sealed class TranslationUnitWriter
{
    /// <summary>Writes a translation unit file and returns its path.</summary>
    public string Write(NativeLibraryBinding library)
    {
        var content = library.Config.ImplFile is not null
            ? ImplementationFileWithoutHeaderOnlySwitch(library)
            : string.Join('\n', library.Config.TuLines);

        var directory = Path.Combine(library.WorkRoot, "translation-units", $"{library.Name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "bindgen.c");
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>Deletes a generated translation unit directory after parsing is complete.</summary>
    public static void Delete(string translationUnitPath)
    {
        var directory = Path.GetDirectoryName(translationUnitPath);
        if (directory is not null && Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }

    /// <summary>Returns implementation-file content with header-only implementation switches removed.</summary>
    private static string ImplementationFileWithoutHeaderOnlySwitch(NativeLibraryBinding library) =>
        string.Join('\n', File.ReadLines(Path.Combine(library.Directory, library.Config.ImplFile!))
            .Where(line => !Regex.IsMatch(line, @"^#define\s+\w+_IMPLEMENTATION\s*$")));
}
