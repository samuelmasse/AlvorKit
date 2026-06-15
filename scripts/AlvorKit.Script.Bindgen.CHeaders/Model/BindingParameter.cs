namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a function parameter after C has been reduced to managed and interop types.</summary>
/// <param name="ManagedName">Managed C# parameter name.</param>
/// <param name="ManagedType">Public managed parameter type.</param>
/// <param name="InteropType">Raw native interop parameter type.</param>
/// <param name="Modifier">C# parameter modifier such as <c>in</c> or <c>out</c>.</param>
/// <param name="HasStringConvenience">Whether a string overload should be emitted.</param>
/// <param name="IsUntypedPointer">Whether the parameter is an untyped pointer candidate.</param>
/// <param name="IsConstPointee">Whether the pointed-to value is const.</param>
/// <param name="IsSizeT">Whether the native spelling is <c>size_t</c>.</param>
/// <param name="CallbackType">Managed delegate type name when this parameter is a callback.</param>
public record BindingParameter(
    string ManagedName,
    string ManagedType,
    string InteropType,
    string Modifier,
    bool HasStringConvenience,
    bool IsUntypedPointer = false,
    bool IsConstPointee = false,
    bool IsSizeT = false,
    string? CallbackType = null);
