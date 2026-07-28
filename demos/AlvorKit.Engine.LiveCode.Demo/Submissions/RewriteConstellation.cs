namespace AgentSubmissions;

[Root]
public sealed class RewriteConstellation(
    InjectorScopeGraph graph,
    UniverseColonies universe) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        var colonies = universe.Snapshot();
        for (var index = 0; index < colonies.Length; index++)
        {
            var angle = index / (float)Math.Max(1, colonies.Length) * MathF.Tau - MathF.PI * 0.5f;
            var garden = colonies[index].Garden;
            garden.Anchor = (0.5f + MathF.Cos(angle) * 0.3f, 0.5f + MathF.Sin(angle) * 0.32f);
            garden.SporeCount = 34 + index * 7;
            garden.OrbitRadius = 104f + index * 9f;
            garden.RotationSpeed = index % 2 == 0 ? 1.35f : -1.35f;
            garden.Form = "synchronized lattice";
            garden.Burst(2f);
        }

        var authored = universe.OpenAgentColony();
        authored.Garden.Anchor = (0.5f, 0.5f);
        universe.NetworkColor = (0.22f, 1f, 0.74f, 0.9f);
        universe.NetworkIntensity = 1f;
        universe.LastIntervention = "Agent composed a synchronized four-scope lattice.";

        output.WriteLine("Repositioned every active scope and authored a new central executor.");
        output.Value("activeColonies", universe.Span.Length);
        output.Value("newScopeId", graph.GetId(authored.Scope).Value);
        output.Value("graphRevision", graph.Revision);
    }
}
