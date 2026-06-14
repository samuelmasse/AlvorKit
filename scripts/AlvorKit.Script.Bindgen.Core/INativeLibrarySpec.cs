namespace AlvorKit.Script.Bindgen;

/// <summary>Loads the bindgen configuration for one native library family.</summary>
public interface INativeLibrarySpec
{
    /// <summary>Gets the native directory name under the repository's native folder.</summary>
    string Name { get; }

    /// <summary>Reads the library-specific bindgen configuration from a native library directory.</summary>
    BindgenConfig LoadConfig(string libraryDirectory);
}
