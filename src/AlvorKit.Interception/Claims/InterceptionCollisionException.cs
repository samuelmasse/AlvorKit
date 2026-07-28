namespace AlvorKit.Interception;

/// <summary>Rejects ambiguous physical or logical interception ownership.</summary>
public sealed class InterceptionCollisionException(
    InterceptionCollision collision)
    : InvalidOperationException(collision.Message)
{
    /// <summary>Gets the structured order-independent collision.</summary>
    public InterceptionCollision Collision { get; } = collision;
}
