namespace AlvorKit.Interception;

/// <summary>One physical interception ownership request plus logical and consumer metadata.</summary>
public sealed class InterceptionClaim
{
    /// <summary>Creates one validated physical and optional logical ownership claim.</summary>
    public InterceptionClaim(
        InterceptionTarget method,
        InterceptionPhysicalRegion region,
        InterceptionClaimOwner owner,
        InterceptionLogicalOperand? logicalOperand = null)
    {
        if (!method.IsValid)
            throw new ArgumentException("A valid interception target is required.", nameof(method));
        if (!region.IsValid)
            throw new ArgumentException("A valid physical region is required.", nameof(region));
        ArgumentNullException.ThrowIfNull(owner);
        if (logicalOperand is { IsValid: false })
            throw new ArgumentException("A valid logical operand is required.", nameof(logicalOperand));

        Method = method;
        Region = region;
        Owner = owner;
        LogicalOperand = logicalOperand;
    }

    /// <summary>Gets the loaded method containing the physical region.</summary>
    public InterceptionTarget Method { get; }

    /// <summary>Gets the exact physical region claimed in that method.</summary>
    public InterceptionPhysicalRegion Region { get; }

    /// <summary>Gets the consumer and selector metadata, which is not part of physical identity.</summary>
    public InterceptionClaimOwner Owner { get; }

    /// <summary>Gets the optional logical operand used for cross-method collision detection.</summary>
    public InterceptionLogicalOperand? LogicalOperand { get; }
}
