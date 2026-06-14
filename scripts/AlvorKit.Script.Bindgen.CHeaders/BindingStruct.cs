namespace AlvorKit.Script.Bindgen;

/// <summary>Describes a managed struct emitted from a native struct or union.</summary>
/// <param name="NativeName">Native struct, union, or typedef name.</param>
/// <param name="ManagedName">Managed C# struct name.</param>
/// <param name="IsUnion">Whether the native record is a union.</param>
/// <param name="Size">Native record size in bytes.</param>
/// <param name="Fields">Managed fields emitted for the record.</param>
/// <param name="NestedBuffers">Inline buffer helper types required by array fields.</param>
/// <param name="Documentation">Optional XML documentation text.</param>
public record BindingStruct(
    string NativeName,
    string ManagedName,
    bool IsUnion,
    int Size,
    List<BindingField> Fields,
    List<InlineBufferDefinition> NestedBuffers,
    string? Documentation);
