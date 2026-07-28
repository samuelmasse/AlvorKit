namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Root-owned handle for one tracked colony and its cached hot-loop services.</summary>
public sealed class UniverseColony(
    InjectorScopeId id,
    ColonyScope scope,
    ColonyIdentity identity,
    ColonyGarden garden,
    ColonySky sky,
    ColonySimulation simulation)
{
    /// <summary>Gets the stable scope graph identifier.</summary>
    public InjectorScopeId Id { get; } = id;

    /// <summary>Gets the exact injector scope targeted by LiveCode.</summary>
    public ColonyScope Scope { get; } = scope;

    /// <summary>Gets the cached identity service.</summary>
    public ColonyIdentity Identity { get; } = identity;

    /// <summary>Gets the cached mutable visual state.</summary>
    public ColonyGarden Garden { get; } = garden;

    /// <summary>Gets the cached atmosphere state.</summary>
    public ColonySky Sky { get; } = sky;

    /// <summary>Gets the cached local simulation.</summary>
    public ColonySimulation Simulation { get; } = simulation;

    /// <summary>Gets the label used by the graph and client selector.</summary>
    public string Name => Identity.Name;
}
