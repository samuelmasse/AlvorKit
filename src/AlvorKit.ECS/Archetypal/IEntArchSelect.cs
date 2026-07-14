namespace AlvorKit.ECS;

/// <summary>Describes a compile-time archetypal component selection.</summary>
public interface IEntArchSelect<A>
{
    /// <summary>Returns whether one alloc-local arch contains every selected component.</summary>
    static abstract bool Matches(int allocId, int archId);
}
