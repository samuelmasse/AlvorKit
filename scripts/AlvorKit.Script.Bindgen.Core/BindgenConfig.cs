namespace AlvorKit.Script.Bindgen;

/// <summary>Configuration for one native library binding, loaded from native/&lt;library&gt;/bindgen.json.</summary>
public class BindgenConfig
{
    public const string CHeaderKind = "c-header";
    public const string GlRegistryKind = "gl-registry";

    /// <summary>Selects the parser/emitter family used for this library.</summary>
    public string Kind { get; set; } = CHeaderKind;
    public required string Namespace { get; set; }
    public required string ApiClass { get; set; }
    public required string ApiSummary { get; set; }
    public required string BackendClass { get; set; }
    public string NativeClass { get; set; } = "";
    public string NativeLibrary { get; set; } = "";
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

    // C headers sometimes encode booleans as plain int. Listing the native functions here makes
    // the public return type bool while keeping the P/Invoke return as the raw integer.
    public string[] BoolReturns { get; set; } = [];

    // Same idea for parameters: public bool at the contract, raw integer at the native boundary.
    public Dictionary<string, string[]> BoolParams { get; set; } = [];

    // Pointer-plus-count returns can also expose a ReadOnlySpan<T> view. The raw entry point remains
    // available; this names the element type used by the convenience overload.
    public Dictionary<string, string> SpanReturns { get; set; } = [];

    // Macro constants have no native enum grouping, so groups that matter to users are supplied here.
    public Dictionary<string, EnumGroup> EnumGroups { get; set; } = [];

    // A plain int often represents an enum in C APIs. These rules add typed overloads when the C
    // signature does not carry enough information to infer them.
    public EnumOverloads? EnumOverloads { get; set; }

    // OpenGL callback typedefs are opt-in. Unlisted callback parameters stay raw nint; listed ones
    // get typed delegates and instance-rooted setter overloads. ParamGroups supplies enum typing
    // because the typedef text itself does not carry registry group attributes.
    public Dictionary<string, CallbackConfig> Callbacks { get; set; } = [];

    // Registry selection walks feature blocks for this API/profile/version and then applies the
    // explicitly requested extensions.
    public string GlApi { get; set; } = "gl";
    public string GlProfile { get; set; } = "core";
    public string? GlVersion { get; set; }
    public string[] GlExtensions { get; set; } = [];

    // Optional ES availability annotation. "gles2" covers ES 2.0 and later in the registry.
    public string GlEsApi { get; set; } = "gles2";

    // Optional Khronos reference-page docs. DocUrl points at an archive pinned by DOC_TAG; DocSubdir
    // names the directory inside that archive to parse.
    public string? DocUrl { get; set; }
    public string DocDir { get; set; } = "";
    public string DocSubdir { get; set; } = "";
}

/// <summary>Describes one callback typedef that should be surfaced as a typed managed delegate.</summary>
public class CallbackConfig
{
    /// <summary>Managed delegate type name, for example GLDEBUGPROC to GlDebugProc.</summary>
    public required string ManagedName { get; set; }

    /// <summary>
    /// Maps callback parameter names to registry enum groups. Parameters not listed here use the
    /// catch-all enum because callback typedefs do not include group attributes.
    /// </summary>
    public Dictionary<string, string> ParamGroups { get; set; } = [];
}

/// <summary>Rules for building a managed enum from macro constants.</summary>
public class EnumGroup
{
    /// <summary>Shared native prefix to collect and strip, unless <see cref="Members"/> is explicit.</summary>
    public string Prefix { get; set; } = "";

    /// <summary>Explicit native constant names for groups without a clean shared prefix.</summary>
    public string[]? Members { get; set; }

    /// <summary>Native constants to ignore when prefix collection would pull in unrelated names.</summary>
    public string[] Exclude { get; set; } = [];

    /// <summary>Native suffix to drop from member names, such as a repeated category suffix.</summary>
    public string Suffix { get; set; } = "";

    /// <summary>Whether the generated enum represents bit flags.</summary>
    public bool Flags { get; set; }

    /// <summary>Prefix for member names that would otherwise start with a digit.</summary>
    public string DigitPrefix { get; set; } = "Num";
}

/// <summary>Type hints for overloads that turn raw integer parameters into enums.</summary>
public class EnumOverloads
{
    /// <summary>Parameter names that always map to a given enum, across all functions.</summary>
    public Dictionary<string, string> ByParamName { get; set; } = [];

    /// <summary>Per-function overrides, including return values that have no parameter name.</summary>
    public Dictionary<string, FunctionEnums> Functions { get; set; } = [];
}

/// <summary>Enum typing rules for one native function.</summary>
public class FunctionEnums
{
    /// <summary>Enum represented by the raw integer return value, when any.</summary>
    public string? Return { get; set; }

    /// <summary>
    /// Per-parameter candidate types. One entry creates one typed overload; several entries create one
    /// overload per candidate. The literals <c>bool</c> and <c>int</c> are allowed beside enum names.
    /// </summary>
    public Dictionary<string, string[]> Params { get; set; } = [];
}
