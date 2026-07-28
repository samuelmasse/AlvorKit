namespace AlvorKit.Mocking;

/// <summary>
/// Carries exact reflection metadata for one field with a statically checked
/// value type.
/// </summary>
public sealed class MockField<TValue>
    where TValue : allows ref struct
{
    /// <summary>Creates one validated field handle.</summary>
    internal MockField(FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (field.DeclaringType is null)
            throw new MockException("A mock field must have a declaring type.");
        if (field.IsLiteral)
        {
            throw new MockException(
                $"Literal field '{field.DeclaringType.FullName}.{field.Name}' " +
                "has no runtime field-read or field-write opcode to intercept.");
        }
        if (field.FieldType != typeof(TValue))
        {
            throw new MockException(
                $"Field '{field.DeclaringType.FullName}.{field.Name}' has value " +
                $"type '{field.FieldType}', not '{typeof(TValue)}'.");
        }

        Metadata = field;
    }

    /// <summary>Gets the exact field metadata represented by this handle.</summary>
    public FieldInfo Metadata { get; }

    /// <summary>Gets whether the represented field is static.</summary>
    public bool IsStatic => Metadata.IsStatic;

    /// <inheritdoc />
    public override string ToString() =>
        $"{Metadata.DeclaringType!.FullName}.{Metadata.Name}";
}
