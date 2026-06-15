namespace AlvorKit.Script.Bindgen;

/// <summary>Registry token after filtering and C# name mapping.</summary>
/// <param name="NativeName">Native registry token name.</param>
/// <param name="ManagedName">Generated C# token name.</param>
/// <param name="Value">Unsigned token value.</param>
/// <param name="Groups">Native registry groups containing this token.</param>
/// <param name="IsBitmask">Whether the token came from a bitmask enum block.</param>
/// <param name="Availability">Registry availability for generated documentation.</param>
internal sealed record GlRegistryToken(
    string NativeName,
    string ManagedName,
    ulong Value,
    string[] Groups,
    bool IsBitmask,
    GlAvailability Availability);
