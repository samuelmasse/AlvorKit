using System.Collections.Immutable;

namespace AlvorKit.Mocking;

/// <summary>
/// Describes one executable parameter without retaining any invocation value.
/// </summary>
internal sealed class MockParameterShape : IEquatable<MockParameterShape>
{
    private readonly int declaredIndex;
    private readonly MockTypeIdentity type;
    private readonly MockPassingKind passing;
    private readonly bool isIn;
    private readonly bool isOut;
    private readonly bool isScoped;
    private readonly ImmutableArray<MockCustomModifier> requiredModifiers;
    private readonly ImmutableArray<MockCustomModifier> optionalModifiers;

    /// <summary>
    /// Creates an immutable parameter descriptor.
    /// </summary>
    internal MockParameterShape(
        int declaredIndex,
        MockTypeIdentity type,
        MockPassingKind passing,
        bool isIn,
        bool isOut,
        bool isScoped,
        ImmutableArray<MockCustomModifier> requiredModifiers,
        ImmutableArray<MockCustomModifier> optionalModifiers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(declaredIndex);
        this.declaredIndex = declaredIndex;
        this.type = type;
        this.passing = passing;
        this.isIn = isIn;
        this.isOut = isOut;
        this.isScoped = isScoped;
        this.requiredModifiers = requiredModifiers.IsDefault ? [] : requiredModifiers;
        this.optionalModifiers = optionalModifiers.IsDefault ? [] : optionalModifiers;
    }

    internal int DeclaredIndex => declaredIndex;
    internal MockTypeIdentity Type => type;
    internal MockPassingKind Passing => passing;
    internal bool IsIn => isIn;
    internal bool IsOut => isOut;
    internal bool IsScoped => isScoped;
    internal ImmutableArray<MockCustomModifier> RequiredModifiers => requiredModifiers;
    internal ImmutableArray<MockCustomModifier> OptionalModifiers => optionalModifiers;

    /// <inheritdoc />
    public bool Equals(MockParameterShape? other)
    {
        return other is not null
            && declaredIndex == other.declaredIndex
            && type == other.type
            && passing == other.passing
            && isIn == other.isIn
            && isOut == other.isOut
            && isScoped == other.isScoped
            && requiredModifiers.AsSpan().SequenceEqual(other.requiredModifiers.AsSpan())
            && optionalModifiers.AsSpan().SequenceEqual(other.optionalModifiers.AsSpan());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockParameterShape other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(declaredIndex);
        hash.Add(type);
        hash.Add(passing);
        hash.Add(isIn);
        hash.Add(isOut);
        hash.Add(isScoped);

        foreach (MockCustomModifier modifier in requiredModifiers)
            hash.Add(modifier);
        foreach (MockCustomModifier modifier in optionalModifiers)
            hash.Add(modifier);

        return hash.ToHashCode();
    }
}
