namespace AlvorKit.Script.Bindgen;

/// <summary>C-header-specific configuration for one native library binding.</summary>
public sealed partial class BindgenConfig
{
    /// <summary>Generated native P/Invoke class name for C header bindings.</summary>
    public string NativeClass { get; set; } = "";

    /// <summary>Native library base name loaded by generated P/Invoke entry points.</summary>
    public string NativeLibrary { get; set; } = "";

    /// <summary>Optional native implementation file compiled by generated verification shims.</summary>
    public string? ImplFile { get; set; }

    /// <summary>Extra lines written into the temporary translation unit.</summary>
    public string[] TuLines { get; set; } = [];

    /// <summary>Include subdirectory under <see cref="SourceDir"/> used for generated include paths.</summary>
    public string IncludeSubdir { get; set; } = "";

    /// <summary>Additional constants supplied when headers do not expose parseable values.</summary>
    public Dictionary<string, int> Constants { get; set; } = [];

    /// <summary>Native scalar types that should be surfaced as managed booleans.</summary>
    public string[] BoolTypes { get; set; } = [];

    /// <summary>Native enum names that should be generated with the Flags attribute.</summary>
    public string[] FlagsEnums { get; set; } = [];

    /// <summary>Additional preprocessor defines passed to native header parsing.</summary>
    public string[] ExtraDefines { get; set; } = [];

    /// <summary>Native struct names emitted as transparent managed structs.</summary>
    public string[] TransparentStructs { get; set; } = [];

    /// <summary>Maps native functions to parameters treated as public out parameters.</summary>
    public Dictionary<string, string[]> OutParams { get; set; } = [];

    /// <summary>Maps native functions to pointer parameters treated as public input spans.</summary>
    public Dictionary<string, string[]> InParams { get; set; } = [];

    /// <summary>Additional native prefixes stripped when generating managed identifiers.</summary>
    public string[] ExtraPrefixes { get; set; } = [];

    /// <summary>Explicit native-to-managed type rename overrides.</summary>
    public Dictionary<string, string> TypeRenames { get; set; } = [];

    /// <summary>Native types projected directly as existing managed types instead of generated structs.</summary>
    public Dictionary<string, string> TypeAliases { get; set; } = [];

    /// <summary>Native types that keep a generated interop struct while the public API uses a type alias.</summary>
    public Dictionary<string, string> InteropTypeAliases { get; set; } = [];

    /// <summary>Native pointer pointee types projected as opaque handles, even when record definitions are visible.</summary>
    public Dictionary<string, string> OpaqueTypes { get; set; } = [];

    /// <summary>Explicit native-to-managed function rename overrides.</summary>
    public Dictionary<string, string> FunctionRenames { get; set; } = [];

    /// <summary>Optional size verification shim source file relative to the native directory.</summary>
    public string? SizeofShim { get; set; }

    /// <summary>Native export name used by the size verification shim.</summary>
    public string ShimExport { get; set; } = "";

    /// <summary>Native functions whose integer return values should be projected as public booleans.</summary>
    public string[] BoolReturns { get; set; } = [];

    /// <summary>Native functions kept public but de-emphasized as advanced raw binding members.</summary>
    public string[] AdvancedFunctions { get; set; } = [];

    /// <summary>Native functions whose pointer parameters must not receive managed string convenience overloads.</summary>
    public Dictionary<string, string> StringSkip { get; set; } = [];

    /// <summary>Maps OS platform names to native functions exported only by that platform's native library builds.</summary>
    public Dictionary<string, string[]> PlatformFunctions { get; set; } = [];

    /// <summary>Native function parameters projected as public booleans over raw integer values.</summary>
    public Dictionary<string, string[]> BoolParams { get; set; } = [];

    /// <summary>Macro constant groups promoted into generated managed enums.</summary>
    public Dictionary<string, EnumGroup> EnumGroups { get; set; } = [];

    /// <summary>Rules that add typed enum overloads for raw integer C signatures.</summary>
    public EnumOverloads? EnumOverloads { get; set; }

    /// <summary>Callback typedefs promoted into typed managed delegates and setter overloads.</summary>
    public Dictionary<string, CallbackConfig> Callbacks { get; set; } = [];

    /// <summary>Whether to emit FreeType-specific convenience members over the raw freetype.h surface.</summary>
    public bool FreeTypeConvenience { get; set; }

    /// <summary>Whether to emit xxHash-specific convenience members over the raw xxhash.h surface.</summary>
    public bool XxHashConvenience { get; set; }

    /// <summary>Whether to emit FastNoise2-specific span convenience members over the raw FastNoise2 C API.</summary>
    public bool FastNoise2Convenience { get; set; }
}
