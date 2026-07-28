namespace AgentSubmissions;

[Root]
public sealed class RetireColony(
    InjectorScopeGraph graph,
    UniverseColonies universe) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        var before = graph.Snapshot();
        universe.Close("Tide Archive");
        var after = graph.Snapshot(includeEnded: true);

        output.WriteLine("Retired Tide Archive through the scope lifecycle layer.");
        output.Value("revisionBefore", before.Revision);
        output.Value("revisionAfter", after.Revision);
        output.Value("graphNodesIncludingTombstones", after.Nodes.Length);
    }
}
