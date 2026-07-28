namespace AgentSubmissions;

[Colony]
public sealed class InspectAndAwaken(
    ColonyScope scope,
    InjectorScopeGraph graph,
    UniverseClock clock,
    UniverseColonies universe,
    ColonyIdentity identity,
    ColonyGarden garden,
    ColonySky sky) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        output.WriteLine($"Entered the exact live scope for {identity.Name} at engine tick {clock.Tick}.");
        output.Value("scopeId", graph.GetId(scope).Value);
        output.Value("organismsBefore", garden.SporeCount);
        output.Value("weatherBefore", sky.Weather);

        graph.Run<ProbeScope>(
            scope,
            probe => output.Value("nestedProbe", probe.Get<ProbeTelemetry>().Read()),
            "Agent diagnostic probe");

        garden.Primary = (1f, 0.16f, 0.82f, 1f);
        garden.Secondary = (0.15f, 1f, 0.88f, 1f);
        garden.Radius = 88f;
        garden.OrbitRadius = 148f;
        garden.SporeCount = 58;
        garden.RotationSpeed = -1.8f;
        garden.Form = "agent singularity";
        garden.Burst(2.8f);
        sky.Halo = (0.7f, 0.18f, 1f, 0.86f);
        sky.Warp = 0.78f;
        sky.Weather = "impossible magenta storm";

        universe.NetworkColor = (1f, 0.18f, 0.72f, 0.72f);
        universe.NetworkIntensity = 0.9f;
        universe.LastIntervention = $"Agent rewrote {identity.Name} inside scope #{graph.GetId(scope).Value}.";

        output.WriteLine("Rewrote morphology, motion, palette, atmosphere, and network energy.");
        output.Value("organismsAfter", garden.SporeCount);
        output.Value("graphRevision", graph.Revision);
    }
}
