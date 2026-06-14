namespace AlvorKit.Script.Bindgen;

/// <summary>Describes an opaque native pointer represented as a managed handle struct.</summary>
/// <param name="NativeName">Native pointee type name.</param>
/// <param name="ManagedName">Managed handle type name.</param>
public record BindingHandle(string NativeName, string ManagedName);
