namespace AlvorKit.Script.Bindgen;

/// <summary>Describes one managed enum member emitted from a native enum or macro group.</summary>
/// <param name="NativeName">Native enum member or macro constant name.</param>
/// <param name="ManagedName">Managed C# member name.</param>
/// <param name="Value">Integral native value.</param>
/// <param name="Documentation">Optional XML documentation text.</param>
public record BindingEnumMember(string NativeName, string ManagedName, long Value, string? Documentation);
