namespace AlvorKit;

/// <summary>
/// Identifies one required or optional custom modifier in declared metadata order.
/// </summary>
internal readonly record struct MockCustomModifier
{
    private readonly MockTypeIdentity type;

    /// <summary>
    /// Creates a custom-modifier descriptor.
    /// </summary>
    internal MockCustomModifier(Type type)
    {
        this.type = new MockTypeIdentity(type);
    }

    /// <summary>
    /// Gets the modifier type.
    /// </summary>
    internal MockTypeIdentity Type => type;

    /// <inheritdoc />
    public override string ToString() => type.ToString();
}
