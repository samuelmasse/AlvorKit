namespace AlvorKit;

/// <summary>Describes a component's value, marker, and whether it uses unobserved archetypal storage.</summary>
public readonly record struct EntComponent(Type ValueType, Type NameType, bool IsArchetypal)
{
    public override string ToString() =>
    $"{ValueType.Name} {NameType.Name}";
}
