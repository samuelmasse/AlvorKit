namespace AlvorKit.Script.Bindgen;

/// <summary>Token too wide for uint-backed enums, emitted as a standalone constant.</summary>
/// <param name="ManagedName">Generated C# constant name.</param>
/// <param name="NativeName">Native registry token name.</param>
/// <param name="Value">Unsigned token value.</param>
/// <param name="Availability">Registry availability for generated documentation.</param>
public sealed record GlConstant(
    string ManagedName,
    string NativeName,
    ulong Value,
    GlAvailability Availability);
