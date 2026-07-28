namespace AgentSubmissions;

[Root]
public sealed class ReleaseFrozenGame(
    ObservatoryFreeze freeze,
    UniverseClock clock) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        output.WriteLine("Releasing the deliberately frozen game loop.");
        output.Value("frozenGameThreadId", freeze.GameThreadId);
        output.Value("frozenAtTick", clock.Tick);
        output.Value("inspectorThreadId", Environment.CurrentManagedThreadId);
        freeze.Release();
    }
}
