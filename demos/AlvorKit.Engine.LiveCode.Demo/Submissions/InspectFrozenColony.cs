namespace AgentSubmissions;

[Colony]
public sealed class InspectFrozenColony(
    ColonyScope scope,
    InjectorScopeGraph graph,
    ObservatoryFreeze freeze,
    UniverseClock clock,
    ColonyIdentity identity,
    ColonyGarden garden,
    ColonySky sky) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        output.WriteLine($"Inspected {identity.Name} while its game loop remained frozen.");
        output.Value("scopeId", graph.GetId(scope).Value);
        output.Value("inspectorThreadId", Environment.CurrentManagedThreadId);
        output.Value("frozenGameThreadId", freeze.GameThreadId);
        output.Value("engineTick", clock.Tick);
        output.Value("rotationSpeed", garden.RotationSpeed);
        output.Value("solarAngle", garden.SolarAngle);
        output.Value("sporeCount", garden.SporeCount);
        output.Value("weather", sky.Weather);
        output.Value("form", garden.Form);
    }
}
