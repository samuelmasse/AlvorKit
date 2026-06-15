namespace AlvorKit.Script.Bindgen;

/// <summary>Managed and raw interop type pair for a mapped declaration.</summary>
/// <param name="Managed">Public managed type.</param>
/// <param name="Interop">Raw function-pointer type.</param>
internal sealed record GlTypeMapping(string Managed, string Interop);
