namespace AlvorKit.Script.Bindgen;

public sealed class TranslationUnitWriter
{
    public string Write(NativeLibraryBinding library)
    {
        var content = library.Config.ImplFile is not null
            ? ImplementationFileWithoutHeaderOnlySwitch(library)
            : string.Join('\n', library.Config.TuLines);

        var directory = Path.Combine(Path.GetTempPath(), "AlvorKit.Bindgen", $"{library.Name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "bindgen.c");
        File.WriteAllText(path, content);
        return path;
    }

    private static string ImplementationFileWithoutHeaderOnlySwitch(NativeLibraryBinding library) =>
        string.Join('\n', File.ReadLines(Path.Combine(library.Directory, library.Config.ImplFile!))
            .Where(line => !Regex.IsMatch(line, @"^#define\s+\w+_IMPLEMENTATION\s*$")));
}
