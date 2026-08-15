namespace AlvorKit;

/// <summary>Opaque identity and diagnostic name for one interception consumer.</summary>
public sealed class InterceptionClaimConsumer
{
    /// <summary>Creates one distinct consumer identity.</summary>
    public InterceptionClaimConsumer(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>Gets the diagnostic consumer name.</summary>
    public string Name { get; }

    /// <inheritdoc />
    public override string ToString() => Name;
}
