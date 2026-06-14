namespace AlvorKit.Script.Bindgen;

/// <summary>Base implementation for library specs stored as native/&lt;library&gt;/bindgen.json.</summary>
public abstract class JsonNativeLibrarySpec(string name) : INativeLibrarySpec
{
    /// <summary>Gets the native directory name under the repository's native folder.</summary>
    public string Name { get; } = name;

    /// <summary>Reads and deserializes a case-insensitive bindgen.json file.</summary>
    public BindgenConfig LoadConfig(string libraryDirectory) =>
        JsonSerializer.Deserialize<BindgenConfig>(
            File.ReadAllText(Path.Combine(libraryDirectory, "bindgen.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException($"Could not read bindgen config for {Name}.");
}
