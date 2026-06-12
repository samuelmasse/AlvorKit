namespace AlvorKit.Script.Bindgen;

public sealed class TranslationUnitWriter
{
    public string Write(NativeLibraryBinding library)
    {
        var content = library.Config.ImplFile is not null
            ? ImplementationFileWithoutHeaderOnlySwitch(library)
            : string.Join('\n', library.Config.TuLines);

        var path = Path.Combine(Path.GetTempPath(), $"{library.Name}-bindgen.c");
        File.WriteAllText(path, content);
        return path;
    }

    private static string ImplementationFileWithoutHeaderOnlySwitch(NativeLibraryBinding library) =>
        string.Join('\n', File.ReadLines(Path.Combine(library.Directory, library.Config.ImplFile!))
            .Where(line => !Regex.IsMatch(line, @"^#define\s+\w+_IMPLEMENTATION\s*$")));
}
