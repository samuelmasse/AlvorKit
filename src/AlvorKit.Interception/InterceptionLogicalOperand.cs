namespace AlvorKit;

/// <summary>Logical operation identity used to detect cross-method consumer composition.</summary>
public readonly struct InterceptionLogicalOperand : IEquatable<InterceptionLogicalOperand>
{
    private InterceptionLogicalOperand(
        InterceptionLogicalOperandKind kind,
        InterceptionTarget target)
    {
        Kind = kind;
        Target = target;
    }

    /// <summary>Gets the operand category.</summary>
    public InterceptionLogicalOperandKind Kind { get; }

    /// <summary>Gets the exact logical target identity.</summary>
    public InterceptionTarget Target { get; }

    /// <summary>Creates a logical method operand from one exact loaded method.</summary>
    public static InterceptionLogicalOperand ForMethod(
        InterceptionTarget target)
    {
        if (!target.IsValid)
            throw new ArgumentException("A valid interception target is required.", nameof(target));
        return new(InterceptionLogicalOperandKind.Method, target);
    }

    internal bool IsValid =>
        Kind == InterceptionLogicalOperandKind.Method &&
        Target.IsValid;

    /// <inheritdoc />
    public bool Equals(InterceptionLogicalOperand other) =>
        Kind == other.Kind &&
        Target == other.Target;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is InterceptionLogicalOperand other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(Kind, Target);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Kind.ToString().ToLowerInvariant()}:{Target.DisplayName}";

    /// <summary>Tests exact logical-operand identity.</summary>
    public static bool operator ==(
        InterceptionLogicalOperand left,
        InterceptionLogicalOperand right) =>
        left.Equals(right);

    /// <summary>Tests exact logical-operand inequality.</summary>
    public static bool operator !=(
        InterceptionLogicalOperand left,
        InterceptionLogicalOperand right) =>
        !left.Equals(right);
}
