namespace AgentSubmissions;

[Root]
public sealed class CreateColony(
    InjectorScopeGraph graph,
    UniverseColonies universe) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        var colony = universe.OpenAgentColony();
        universe.NetworkIntensity = 0.95f;
        universe.NetworkColor = (0.22f, 1f, 0.76f, 0.8f);
        universe.LastIntervention = $"Agent created {colony.Name} as a new sibling executor.";

        output.WriteLine($"Opened and selected the new sibling scope '{colony.Name}'.");
        output.Value("scopeId", graph.GetId(colony.Scope).Value);
        output.Value("activeColonies", universe.Span.Length);
        output.Value("graphRevision", graph.Revision);
    }
}
