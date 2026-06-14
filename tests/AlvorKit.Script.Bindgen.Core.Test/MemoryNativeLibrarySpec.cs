using AlvorKit.Script.Bindgen;

namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>In-memory native library spec used by binding path tests.</summary>
internal sealed class MemoryNativeLibrarySpec(string name, BindgenConfig config) : INativeLibrarySpec
{
    /// <summary>Gets the native library directory name.</summary>
    public string Name { get; } = name;

    /// <summary>Returns the prebuilt config supplied by the test.</summary>
    public BindgenConfig LoadConfig(string libraryDirectory) => config;
}
