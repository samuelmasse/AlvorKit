namespace AlvorKit.Interception;

/// <summary>Consumer and selector metadata attached to one physical claim.</summary>
public sealed class InterceptionClaimOwner
{
    /// <summary>Creates owner metadata without making it part of physical identity.</summary>
    public InterceptionClaimOwner(
        InterceptionClaimConsumer consumer,
        string selector)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        if (string.IsNullOrWhiteSpace(selector))
            throw new ArgumentException("An interception selector description is required.", nameof(selector));
        Consumer = consumer;
        Selector = selector;
    }

    /// <summary>Gets the consumer owning this claim.</summary>
    public InterceptionClaimConsumer Consumer { get; }

    /// <summary>Gets the consumer-specific selector diagnostic.</summary>
    public string Selector { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Consumer} selector '{Selector}'";
}
