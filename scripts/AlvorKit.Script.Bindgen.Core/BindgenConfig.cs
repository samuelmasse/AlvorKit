namespace AlvorKit.Script.Bindgen;

/// <summary>Per-library generator configuration, loaded from native/&lt;lib&gt;/bindgen.json.</summary>
public class BindgenConfig
{
    public const string CHeaderKind = "c-header";
    public const string GlRegistryKind = "gl-registry";

    /// <summary>The generator pipeline: libclang over a C header, or the Khronos gl.xml registry.</summary>
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

    // c-header: functions whose int return is really a boolean (a header that uses plain int for
    // GLFW_TRUE/FALSE carries no bool type, so the int-bool functions are named here). The generated
    // method returns bool, marshalled at the P/Invoke boundary - there is no int-returning version.
    public string[] BoolReturns { get; set; } = [];

    // c-header: int parameters that are really booleans, keyed by native function name. The generated
    // parameter is bool, marshalled at the boundary.
    public Dictionary<string, string[]> BoolParams { get; set; } = [];

    // c-header: functions that return a pointer plus an out-count (the trailing out parameter), to
    // surface as a garbage-free ReadOnlySpan<T> view over the native memory. Maps the native function
    // name to the span's managed element type. The raw pointer+count method is kept.
    public Dictionary<string, string> SpanReturns { get; set; } = [];

    // Synthesized enums grouped from #define constants (a C header that uses macros, not C enums,
    // carries no grouping, so it is supplied here). Keyed by the managed enum name.
    public Dictionary<string, EnumGroup> EnumGroups { get; set; } = [];

    // Typed convenience overloads: which int parameters/returns of which functions stand for which
    // enums. Not derivable from the C types (everything is int), so it is supplied here.
    public EnumOverloads? EnumOverloads { get; set; }

    // gl-registry: function-pointer typedefs (GLDEBUGPROC) to surface as typed delegates with
    // instance-rooted callback-setter overloads. Keyed by the native typedef name. Opt-in: a
    // callback parameter stays a raw nint unless its typedef is listed here. The GLenum parameters
    // it types come from ParamGroups, since the typedef text carries no group attributes.
    public Dictionary<string, CallbackConfig> Callbacks { get; set; } = [];

    // gl-registry options: the feature walk selects everything in <feature api="glApi"> blocks
    // up to glVersion for glProfile, plus the opted-in extensions.
    public string GlApi { get; set; } = "gl";
    public string GlProfile { get; set; } = "core";
    public string? GlVersion { get; set; }
    public string[] GlExtensions { get; set; } = [];

    // The OpenGL ES api whose introduction version is recorded alongside the desktop version in
    // each member's docs ("gles2" spans ES 2.0-3.2). Empty to omit the ES annotation.
    public string GlEsApi { get; set; } = "gles2";

    // The OpenGL reference pages (DocBook), pinned by the DOC_TAG file, supplying the command and
    // parameter doc comments. DocUrl is a repository tarball; DocSubdir is the directory within it
    // to read ("gl4"). Leave DocUrl null to generate without reference-page docs.
    public string? DocUrl { get; set; }
    public string DocDir { get; set; } = "";
    public string DocSubdir { get; set; } = "";
}

/// <summary>One function-pointer typedef surfaced as a typed, instance-rooted callback.</summary>
public class CallbackConfig
{
    /// <summary>The managed delegate type name (GLDEBUGPROC becomes GlDebugProc). Required.</summary>
    public required string ManagedName { get; set; }

    /// <summary>
    /// A callback parameter name to the native enum group it belongs to (source to DebugSource).
    /// The typedef text carries no group attributes, so the typing is supplied here; parameters
    /// without an entry fall back to the catch-all enum.
    /// </summary>
    public Dictionary<string, string> ParamGroups { get; set; } = [];
}

/// <summary>One synthesized enum: how to gather its constants and how to name the members.</summary>
public class EnumGroup
{
    /// <summary>Native prefix the members share; collected by it (unless <see cref="Members"/> is set) and stripped for naming.</summary>
    public string Prefix { get; set; } = "";

    /// <summary>Explicit native constant names, for groups that have no clean shared prefix. Overrides prefix collection.</summary>
    public string[]? Members { get; set; }

    /// <summary>Native names to skip when collecting by prefix (e.g. an unrelated constant sharing the prefix).</summary>
    public string[] Exclude { get; set; } = [];

    /// <summary>Native suffix to drop from member names (e.g. the trailing <c>_CURSOR</c> on cursor shapes).</summary>
    public string Suffix { get; set; } = "";

    /// <summary>Emit with <c>[Flags]</c>.</summary>
    public bool Flags { get; set; }

    /// <summary>Prefix for members whose stripped name would start with a digit (<c>GLFW_KEY_0</c> to <c>D0</c>).</summary>
    public string DigitPrefix { get; set; } = "Num";
}

/// <summary>Where typed enum overloads of the API functions come from.</summary>
public class EnumOverloads
{
    /// <summary>A parameter name to the enum it always stands for (<c>key</c> to <c>GlfwKey</c>), applied across all functions.</summary>
    public Dictionary<string, string> ByParamName { get; set; } = [];

    /// <summary>Per-function typing, for returns (which have no name) and the rest, keyed by the native function name.</summary>
    public Dictionary<string, FunctionEnums> Functions { get; set; } = [];
}

/// <summary>Per-function enum typing.</summary>
public class FunctionEnums
{
    /// <summary>The enum the <c>int</c> return value stands for, or null.</summary>
    public string? Return { get; set; }

    /// <summary>
    /// A parameter name to the candidate types it may take (overrides <see cref="EnumOverloads.ByParamName"/>).
    /// One entry is a fixed typing; several emit one overload each (e.g. a window-hint value can be a bool or
    /// any of the value enums). The literal <c>bool</c>/<c>int</c> are allowed alongside enum names.
    /// </summary>
    public Dictionary<string, string[]> Params { get; set; } = [];
}
