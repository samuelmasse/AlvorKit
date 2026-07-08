namespace AlvorKit.ECS;

public readonly record struct EntComponent(Type ValueType, Type NameType)
{
    public override string ToString() =>
    $"{ValueType.Name} {NameType.Name}";
}
