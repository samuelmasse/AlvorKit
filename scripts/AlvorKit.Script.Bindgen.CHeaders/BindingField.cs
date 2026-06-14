namespace AlvorKit.Script.Bindgen;

/// <summary>Describes one managed field emitted for a native struct or union field.</summary>
/// <param name="ManagedName">Managed C# field name.</param>
/// <param name="ManagedType">Managed C# field type.</param>
/// <param name="Offset">Native field offset in bytes.</param>
/// <param name="Documentation">Optional XML documentation text.</param>
public record BindingField(string ManagedName, string ManagedType, int Offset, string? Documentation);
