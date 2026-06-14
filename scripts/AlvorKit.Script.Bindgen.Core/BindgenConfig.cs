namespace AlvorKit.Script.Bindgen;

/// <summary>Configuration for one native library binding, loaded from native/&lt;library&gt;/conf/bindgen.json.</summary>
public sealed partial class BindgenConfig
{
    /// <summary>Serializer options used by hand-authored bindgen.json files.</summary>
    private static readonly JsonSerializerOptions ConfigJsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Bindgen kind used for C header parsing and emission.</summary>
    public const string CHeaderKind = "c-header";

    /// <summary>Bindgen kind used for Khronos OpenGL registry parsing and emission.</summary>
    public const string GlRegistryKind = "gl-registry";

    /// <summary>Selects the parser/emitter family used for this library.</summary>
    public string Kind { get; set; } = CHeaderKind;

    /// <summary>Managed namespace for generated public API types.</summary>
    public required string Namespace { get; set; }

    /// <summary>Generated public API class name.</summary>
    public required string ApiClass { get; set; }

    /// <summary>XML documentation summary emitted for the public API class.</summary>
    public required string ApiSummary { get; set; }

    /// <summary>Generated backend class name.</summary>
    public required string BackendClass { get; set; }

    /// <summary>Native symbol prefix stripped from generated managed member names.</summary>
    public required string Prefix { get; set; }

    /// <summary>Managed prefix applied to identifiers that would otherwise start with a digit.</summary>
    public string DigitNamePrefix { get; set; } = "Num";

    /// <summary>Workspace directory under the current user's profile for downloaded sources.</summary>
    public required string WorkDir { get; set; }

    /// <summary>Source directory path relative to <see cref="WorkDir"/> after version token replacement.</summary>
    public required string SourceDir { get; set; }

    /// <summary>Optional source tag or commit used for version token replacement instead of version/TAG.</summary>
    public string? SourceTag { get; set; }

    /// <summary>Optional URL for downloading the source archive or registry file.</summary>
    public string? SourceUrl { get; set; }

    /// <summary>Primary header or registry path relative to <see cref="SourceDir"/>.</summary>
    public required string Header { get; set; }

    /// <summary>Output path for the generated public API project.</summary>
    public required string ApiProject { get; set; }

    /// <summary>Output path for the generated backend project.</summary>
    public required string BackendProject { get; set; }

    /// <summary>Native declarations skipped during generation with reasons for maintainers.</summary>
    public Dictionary<string, string> Skip { get; set; } = [];

    /// <summary>Native constants skipped during generation with reasons for maintainers.</summary>
    public Dictionary<string, string> SkipConstants { get; set; } = [];

    /// <summary>Reads and deserializes a case-insensitive conf/bindgen.json file.</summary>
    public static BindgenConfig Load(string libraryDirectory, string libraryName) =>
        JsonSerializer.Deserialize<BindgenConfig>(
            File.ReadAllText(Path.Combine(libraryDirectory, "conf", "bindgen.json")),
            ConfigJsonOptions)
        ?? throw new InvalidOperationException($"Could not read bindgen config for {libraryName}.");
}
