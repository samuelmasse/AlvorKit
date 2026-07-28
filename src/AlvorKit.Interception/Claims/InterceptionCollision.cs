namespace AlvorKit.Interception;

/// <summary>Order-independent diagnostic describing two incompatible claims.</summary>
public sealed class InterceptionCollision
{
    private InterceptionCollision(
        InterceptionCollisionReason reason,
        InterceptionClaim first,
        InterceptionClaim second,
        string message)
    {
        Reason = reason;
        First = first;
        Second = second;
        Message = message;
    }

    /// <summary>Gets why the claims conflict.</summary>
    public InterceptionCollisionReason Reason { get; }

    /// <summary>Gets the first claim in stable diagnostic order.</summary>
    public InterceptionClaim First { get; }

    /// <summary>Gets the second claim in stable diagnostic order.</summary>
    public InterceptionClaim Second { get; }

    /// <summary>Gets the complete order-independent collision diagnostic.</summary>
    public string Message { get; }

    internal static InterceptionCollision Create(
        InterceptionCollisionReason reason,
        InterceptionClaim left,
        InterceptionClaim right)
    {
        var leftText = Describe(left);
        var rightText = Describe(right);
        var first = left;
        var second = right;
        if (string.CompareOrdinal(leftText, rightText) > 0)
        {
            (first, second) = (second, first);
            (leftText, rightText) = (rightText, leftText);
        }

        var reasonText = reason == InterceptionCollisionReason.PhysicalRegion
            ? "physical region"
            : "logical operand";
        return new(
            reason,
            first,
            second,
            $"Interception {reasonText} collision: {leftText}; {rightText}. " +
            "Explicit ordered composition is required.");
    }

    private static string Describe(InterceptionClaim claim)
    {
        var method = claim.Method;
        var logical = claim.LogicalOperand is { } operand
            ? $"{operand} [{operand.Target.ModuleMvid:D}/0x{operand.Target.MethodToken:X8}]"
            : "none";
        return
            $"owner='{claim.Owner.Consumer}', selector='{claim.Owner.Selector}', " +
            $"physical='{method.DisplayName} [{method.ModuleMvid:D}/0x{method.MethodToken:X8}] {claim.Region}', " +
            $"logical='{logical}'";
    }
}
