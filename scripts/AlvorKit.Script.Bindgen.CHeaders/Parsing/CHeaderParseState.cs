using ClangSharp;

namespace AlvorKit.Script.Bindgen;

/// <summary>Mutable parse state shared by C-header discovery passes.</summary>
internal sealed class CHeaderParseState
{
    /// <summary>Enums keyed by native spelling and useful aliases.</summary>
    public Dictionary<string, BindingEnum> EnumByNativeName { get; } = [];

    /// <summary>Structs keyed by native spelling.</summary>
    public Dictionary<string, BindingStruct> StructByNativeName { get; } = [];

    /// <summary>Opaque handles keyed by native pointee spelling.</summary>
    public Dictionary<string, string> HandlesByNativeName { get; } = [];

    /// <summary>Callback delegates keyed by native typedef spelling.</summary>
    public Dictionary<string, BindingDelegate> DelegatesByNativeName { get; } = [];

    /// <summary>Callback typedefs that appear in emitted public APIs.</summary>
    public HashSet<string> UsedCallbackTypedefs { get; } = [];

    /// <summary>Native struct names that could not be mapped safely.</summary>
    public HashSet<string> FailedStructs { get; } = [];

    /// <summary>Native records keyed by record or typedef spelling.</summary>
    public Dictionary<string, RecordDecl> RecordByNativeName { get; } = [];

    /// <summary>Public record spellings preferred when a definition also has a private tag name.</summary>
    public List<string> PublicRecordNames { get; } = [];

    /// <summary>Functions selected for the managed API.</summary>
    public List<BindingFunction> Functions { get; } = [];

    /// <summary>Constants selected for the managed API.</summary>
    public List<BindingConstant> Constants { get; } = [];

    /// <summary>Macro values keyed by native spelling.</summary>
    public Dictionary<string, long> ValuesByNativeName { get; } = [];

    /// <summary>Native constant names in discovery order.</summary>
    public List<string> NativeNamesInOrder { get; } = [];

    /// <summary>Native functions skipped with reasons.</summary>
    public List<string> SkippedFunctions { get; } = [];

    /// <summary>Native types needing runtime sizeof shims.</summary>
    public SortedSet<string> SizeofTypes { get; } = [];

    /// <summary>Freezes the accumulated state into the public binding model.</summary>
    public BindingModel ToModel() => new(
        [.. EnumByNativeName.Values.DistinctBy(type => type.NativeName)],
        [.. StructByNativeName.Values],
        [.. HandlesByNativeName.Select(handle => new BindingHandle(handle.Key, handle.Value))],
        [.. DelegatesByNativeName.Where(entry => UsedCallbackTypedefs.Contains(entry.Key)).Select(entry => entry.Value)],
        Functions,
        Constants,
        SkippedFunctions,
        [.. SizeofTypes]);
}
