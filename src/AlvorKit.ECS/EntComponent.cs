namespace AlvorKit.ECS;

/// <summary>Describes a component value type and its generated marker type.</summary>
/// <param name="ValueType">The stored value type.</param>
/// <param name="NameType">The marker type that names the component slot.</param>
public readonly record struct EntComponent(Type ValueType, Type NameType)
{
    /// <summary>Formats the component metadata for diagnostics.</summary>
    public override string ToString() =>
        $"{ValueType.Name} {NameType.Name}";
}
