namespace AlvorKit.Script.Bindgen;

/// <summary>Command parameter after registry typing.</summary>
/// <param name="NativeName">Native registry parameter name.</param>
/// <param name="ManagedName">Generated C# parameter name.</param>
/// <param name="ManagedType">Public managed parameter type.</param>
/// <param name="InteropType">Raw function-pointer parameter type.</param>
/// <param name="Len">Registry length expression for pointer parameters.</param>
/// <param name="PointerDepth">Number of pointer indirections in the native declaration.</param>
/// <param name="PointeeType">Managed element type for one-level typed pointers, when known.</param>
/// <param name="PointeeIsConst">Whether the pointed-to value is const.</param>
/// <param name="PointeeIsChar">Whether the pointed-to value is a GL character.</param>
/// <param name="CallbackType">Managed callback delegate type when the parameter is a configured callback.</param>
public sealed record GlParameter(
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
