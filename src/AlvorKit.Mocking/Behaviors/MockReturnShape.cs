using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Describes an executable return shape without retaining any returned value.
/// </summary>
internal sealed class MockReturnShape : IEquatable<MockReturnShape>
{
    private readonly MockTypeIdentity type;
    private readonly MockReturnKind kind;
    private readonly ImmutableArray<MockCustomModifier> requiredModifiers;
    private readonly ImmutableArray<MockCustomModifier> optionalModifiers;

    /// <summary>
    /// Creates an immutable return descriptor.
    /// </summary>
    internal MockReturnShape(
        MockTypeIdentity type,
        MockReturnKind kind,
        ImmutableArray<MockCustomModifier> requiredModifiers,
        ImmutableArray<MockCustomModifier> optionalModifiers)
    {
        this.type = type;
        this.kind = kind;
        this.requiredModifiers = requiredModifiers.IsDefault ? [] : requiredModifiers;
        this.optionalModifiers = optionalModifiers.IsDefault ? [] : optionalModifiers;
    }

    internal MockTypeIdentity Type => type;
    internal MockReturnKind Kind => kind;
    internal ImmutableArray<MockCustomModifier> RequiredModifiers => requiredModifiers;
    internal ImmutableArray<MockCustomModifier> OptionalModifiers => optionalModifiers;

    /// <inheritdoc />
    public bool Equals(MockReturnShape? other)
    {
        return other is not null
            && type == other.type
            && kind == other.kind
            && requiredModifiers.AsSpan().SequenceEqual(other.requiredModifiers.AsSpan())
            && optionalModifiers.AsSpan().SequenceEqual(other.optionalModifiers.AsSpan());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockReturnShape other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(type);
        hash.Add(kind);

        foreach (MockCustomModifier modifier in requiredModifiers)
            hash.Add(modifier);
        foreach (MockCustomModifier modifier in optionalModifiers)
            hash.Add(modifier);

        return hash.ToHashCode();
    }
}
