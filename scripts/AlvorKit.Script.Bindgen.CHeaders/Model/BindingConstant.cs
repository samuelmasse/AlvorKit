namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a managed constant emitted from a native macro or configured value.</summary>
/// <param name="ManagedName">Managed C# constant name.</param>
/// <param name="Value">Integral constant value.</param>
public record BindingConstant(string ManagedName, long Value);
