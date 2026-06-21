namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a managed delegate emitted from a native function-pointer typedef.</summary>
/// <param name="ManagedName">Managed C# delegate name.</param>
/// <param name="ReturnType">Managed return type.</param>
/// <param name="Parameters">Managed delegate parameters.</param>
/// <param name="NativeName">Native function-pointer typedef name.</param>
/// <param name="Documentation">Parsed upstream typedef documentation.</param>
public record BindingDelegate(
    string ManagedName,
    string ReturnType,
    List<BindingParameter> Parameters,
    string? NativeName = null,
    XmlDocComment? Documentation = null);
