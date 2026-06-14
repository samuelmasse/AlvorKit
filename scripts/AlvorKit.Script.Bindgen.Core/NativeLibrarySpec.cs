namespace AlvorKit.Script.Bindgen;

public interface INativeLibrarySpec
{
    string Name { get; }
    BindgenConfig LoadConfig(string libraryDirectory);
}

public abstract class JsonNativeLibrarySpec(string name) : INativeLibrarySpec
{
    public string Name { get; } = name;

    public BindgenConfig LoadConfig(string libraryDirectory) =>
        JsonSerializer.Deserialize<BindgenConfig>(
            File.ReadAllText(Path.Combine(libraryDirectory, "bindgen.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException($"Could not read bindgen config for {Name}.");
}
