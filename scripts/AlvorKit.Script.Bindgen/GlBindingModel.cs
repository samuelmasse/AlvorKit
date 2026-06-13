namespace AlvorKit.Script.Bindgen;

public record GlEnumMember(string ManagedName, string NativeName, ulong Value, string Since);

public record GlEnumGroup(string NativeName, string ManagedName, bool IsFlags, List<GlEnumMember> Members);

/// <summary>
/// A command parameter. <see cref="ManagedType"/> and <see cref="InteropType"/> differ when a
/// cast sits between the typed surface and the GL entry point (enum groups over GLenum/GLbitfield
/// and grouped GLint, bool over GLboolean). Pointers are <c>nint</c> with the depth-one pointee
/// recorded for overload generation; <see cref="PointeeIsChar"/> marks GLchar at any depth.
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
    bool PointeeIsChar);

public record GlCommand(
    string NativeName,
    string ManagedName,
    string ReturnType,
    string ReturnInteropType,
    List<GlParameter> Parameters,
    string Since);

/// <summary>A token too wide for the uint-backed enums (GL_TIMEOUT_IGNORED), emitted as a constant.</summary>
public record GlConstant(string ManagedName, string NativeName, ulong Value, string Since);

public record GlBindingModel(
    List<GlEnumGroup> Groups,
    GlEnumGroup AllTokens,
    List<GlCommand> Commands,
    List<GlConstant> WideConstants,
    List<string> UngroupedEnumUses,
    List<string> SkippedCommands);
