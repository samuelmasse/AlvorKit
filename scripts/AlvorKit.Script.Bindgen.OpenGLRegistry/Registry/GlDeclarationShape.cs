namespace AlvorKit.Script.Bindgen;

/// <summary>Mapped declaration shape with pointer and callback metadata.</summary>
/// <param name="Name">Native declaration name.</param>
/// <param name="Type">Managed and interop type pair.</param>
/// <param name="PointerDepth">Native pointer depth.</param>
/// <param name="PointeeType">Managed pointee type for typed one-level pointers.</param>
/// <param name="PointeeIsConst">Whether the pointee is const.</param>
/// <param name="PointeeIsChar">Whether the pointee is GLchar.</param>
/// <param name="CallbackType">Managed callback delegate type for configured callbacks.</param>
internal sealed record GlDeclarationShape(
    string Name,
    GlTypeMapping Type,
    int PointerDepth,
    string? PointeeType,
    bool PointeeIsConst,
    bool PointeeIsChar,
    string? CallbackType);
