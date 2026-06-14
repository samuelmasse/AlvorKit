namespace AlvorKit.Script.Bindgen;

/// <summary>Describes one managed enum member emitted from a native enum or macro group.</summary>
/// <param name="ManagedName">Managed C# member name.</param>
/// <param name="Value">Integral native value.</param>
/// <param name="Documentation">Optional XML documentation text.</param>
public record BindingEnumMember(string ManagedName, long Value, string? Documentation);
