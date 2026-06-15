namespace AlvorKit.Script.Bindgen;

/// <summary>Registry token after naming and availability mapping.</summary>
/// <param name="ManagedName">Generated C# enum member name.</param>
/// <param name="NativeName">Native registry token name.</param>
/// <param name="Value">Unsigned token value.</param>
/// <param name="Availability">Registry availability for generated documentation.</param>
/// <param name="Groups">Generated typed enum groups that also contain this token.</param>
public sealed record GlEnumMember(
    string ManagedName,
    string NativeName,
    ulong Value,
    GlAvailability Availability,
    IReadOnlyList<string> Groups);
