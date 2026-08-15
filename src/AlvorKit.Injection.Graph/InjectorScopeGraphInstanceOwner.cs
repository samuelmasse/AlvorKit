namespace AlvorKit;

/// <summary>Weak-table value that records the graph node owning one injected reference instance.</summary>
/// <param name="id">Owning graph-node identifier.</param>
internal sealed class InjectorScopeGraphInstanceOwner(InjectorScopeId id)
{
    /// <summary>Gets the owning graph-node identifier.</summary>
    internal InjectorScopeId Id { get; } = id;
}
