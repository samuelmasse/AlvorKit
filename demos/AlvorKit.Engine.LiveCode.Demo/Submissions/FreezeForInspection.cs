namespace AgentSubmissions;

[Root]
public sealed class FreezeForInspection(
    ObservatoryFreeze freeze,
    UniverseClock clock) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        freeze.Request(clock.Tick);
        output.WriteLine("The game will freeze at the end of its current update.");
        output.Value("requestedAtTick", clock.Tick);
    }
}
