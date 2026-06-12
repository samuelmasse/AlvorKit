namespace AlvorKit.Script.Bindgen;

/// <summary>Per-library generator configuration, loaded from native/&lt;lib&gt;/bindgen.json.</summary>
public class BindgenConfig
{
    public required string Namespace { get; set; }
    public required string ApiClass { get; set; }
    public required string ApiSummary { get; set; }
    public required string BackendClass { get; set; }
    public required string NativeClass { get; set; }
    public required string NativeLibrary { get; set; }
    public required string Prefix { get; set; }
    public string DigitNamePrefix { get; set; } = "Num";
    public required string WorkDir { get; set; }
    public required string SourceDir { get; set; }
    public string? SourceUrl { get; set; }
    public required string Header { get; set; }
    public string? ImplFile { get; set; }
    public string[] TuLines { get; set; } = [];
    public string IncludeSubdir { get; set; } = "";
    public Dictionary<string, int> Constants { get; set; } = [];
    public string[] BoolTypes { get; set; } = [];
    public string[] FlagsEnums { get; set; } = [];
    public required string ApiProject { get; set; }
    public required string BackendProject { get; set; }
    public string[] ExtraDefines { get; set; } = [];
    public string[] TransparentStructs { get; set; } = [];
    public Dictionary<string, string[]> OutParams { get; set; } = [];
    public Dictionary<string, string[]> InParams { get; set; } = [];
    public string[] ExtraPrefixes { get; set; } = [];
    public Dictionary<string, string> TypeRenames { get; set; } = [];
    public string? SizeofShim { get; set; }
    public string ShimExport { get; set; } = "";
    public Dictionary<string, string> Skip { get; set; } = [];
    public Dictionary<string, string> SkipConstants { get; set; } = [];
    public bool SpanExtensions { get; set; }
    public Dictionary<string, string> SpanSkip { get; set; } = [];
    public Dictionary<string, string[]> SpanParams { get; set; } = [];
}
