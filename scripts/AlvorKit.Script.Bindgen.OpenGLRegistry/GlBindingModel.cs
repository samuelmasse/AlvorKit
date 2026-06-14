namespace AlvorKit.Script.Bindgen;

/// <summary>
/// Where an item was introduced. <see cref="Gl"/> is a desktop GL version ("4.6") or, for an
/// extension-sourced item, the extension name. <see cref="GlEs"/> is the OpenGL ES version, or
/// null when the item is desktop-only.
/// </summary>
public record GlAvailability(string Gl, string? GlEs);

/// <summary>
/// A token in an enum group. <see cref="Groups"/> lists the managed names of every typed enum the
/// token also belongs to; the catch-all enum emits it so a token found by its C name points to the
/// typed enums to prefer.
/// </summary>
public record GlEnumMember(string ManagedName, string NativeName, ulong Value, GlAvailability Availability, IReadOnlyList<string> Groups);

public record GlEnumGroup(string NativeName, string ManagedName, bool IsFlags, List<GlEnumMember> Members);

/// <summary>
/// A command parameter. <see cref="ManagedType"/> and <see cref="InteropType"/> differ when a
/// cast sits between the typed surface and the GL entry point (enum groups over GLenum/GLbitfield
/// and grouped GLint, bool over GLboolean). Pointers are <c>nint</c> with the depth-one pointee
/// recorded for overload generation; <see cref="PointeeIsChar"/> marks GLchar at any depth.
/// <see cref="CallbackType"/>, when set, is the managed delegate a function-pointer parameter
/// accepts: the raw method keeps the <c>nint</c>, and a typed setter overload is derived from it.
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

/// <summary>
/// A function-pointer typedef (GLDEBUGPROC) surfaced as a managed delegate. The raw entry point
/// keeps the callback parameter as <c>nint</c>; an instance-rooted setter overload takes this.
/// </summary>
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

/// <summary>A token too wide for the uint-backed enums (GL_TIMEOUT_IGNORED), emitted as a constant.</summary>
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
