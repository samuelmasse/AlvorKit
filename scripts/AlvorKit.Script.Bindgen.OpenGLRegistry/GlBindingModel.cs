namespace AlvorKit.Script.Bindgen;

/// <summary>Availability attached to generated docs: desktop GL version/extension and optional ES version.</summary>
public record GlAvailability(string Gl, string? GlEs);

/// <summary>
/// Registry token after naming. The catch-all enum keeps <see cref="Groups"/> so users can discover
/// the more precise typed enum when they start from a raw GL token.
/// </summary>
public record GlEnumMember(string ManagedName, string NativeName, ulong Value, GlAvailability Availability, IReadOnlyList<string> Groups);

public record GlEnumGroup(string NativeName, string ManagedName, bool IsFlags, List<GlEnumMember> Members);

/// <summary>
/// Command parameter after registry typing. Managed and interop types differ for typed enum groups,
/// GLboolean, handles, and callbacks; pointer metadata is kept so overload generation can add spans,
/// strings, or typed callback setters while the core command stays raw.
/// </summary>
public record GlParameter(
    string NativeName,
    string ManagedName,
    string ManagedType,
    string InteropType,
    string? Len,
    int PointerDepth,
    string? PointeeType,
    bool PointeeIsConst,
    bool PointeeIsChar,
    string? CallbackType = null);

/// <summary>Function-pointer typedef surfaced as a managed delegate for a rooted setter overload.</summary>
public record GlDelegate(string NativeName, string ManagedName, string ReturnType, List<GlParameter> Parameters);

public record GlCommand(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string ReturnInteropType,
    List<GlParameter> Parameters,
    GlAvailability Availability,
    XmlDocComment? Documentation,
    bool ReturnsCString);

/// <summary>Token too wide for uint-backed enums, emitted as a standalone constant.</summary>
public record GlConstant(string ManagedName, string NativeName, ulong Value, GlAvailability Availability);

public record GlBindingModel(
    List<GlEnumGroup> Groups,
    GlEnumGroup AllTokens,
    List<GlCommand> Commands,
    List<GlConstant> WideConstants,
    List<string> UngroupedEnumUses,
    List<string> SkippedCommands,
    List<string> HandleTypes,
    List<GlDelegate> Delegates);
