namespace AlvorKit;

/// <summary>Reason two neutral interception claims cannot coexist.</summary>
public enum InterceptionCollisionReason
{
    /// <summary>The physical baseline IL regions overlap.</summary>
    PhysicalRegion,

    /// <summary>Different consumers claim the same logical operation operand.</summary>
    LogicalOperand
}
