namespace AlvorKit.Script.Bindgen;

/// <summary>Function-pointer typedef surfaced as a managed delegate for a rooted setter overload.</summary>
/// <param name="NativeName">Native callback typedef name.</param>
/// <param name="ManagedName">Generated managed delegate type name.</param>
/// <param name="ReturnType">Managed delegate return type.</param>
/// <param name="Parameters">Managed delegate parameters.</param>
public sealed record GlDelegate(
    string NativeName,
    string ManagedName,
    string ReturnType,
    IReadOnlyList<GlParameter> Parameters);
