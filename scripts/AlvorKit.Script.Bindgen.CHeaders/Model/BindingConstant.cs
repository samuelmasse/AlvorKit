namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a native macro or configured value promoted into the catch-all enum.</summary>
/// <param name="NativeName">Native macro name, or managed configured name when no native macro exists.</param>
/// <param name="ManagedName">Managed C# enum member name.</param>
/// <param name="Value">Integral constant value.</param>
/// <param name="Documentation">Optional upstream documentation text.</param>
public record BindingConstant(string NativeName, string ManagedName, long Value, string? Documentation);
