namespace AlvorKit.Engine.SourceUpdate.Demo;

/// <summary>Runs two pre-existing service instances through the normal engine loop.</summary>
[Root]
internal sealed class SourceUpdateDemoState(
    Log log,
    Injector injector,
    RootScope root,
    RootScripts scripts,
    RootScreen screen,
    RootInput input,
    RootBackbuffer backbuffer,
    PulsePalette palette,
    PulseClock clock) : State
{
    private RootLiveCode liveCode = null!;
    private SourceUpdateModuleLedger sourceUpdates = null!;
    private PulseService left = null!;
    private PulseService right = null!;
    private PulseReading leftReading;
    private PulseReading rightReading;

    /// <inheritdoc />
    public override void Load()
    {
        input.Track = true;
        liveCode = new(injector, root, scripts, new("source-update-demo"));
        _ = liveCode.Enable();
        sourceUpdates = new RootSourceUpdate(
            liveCode.Bridges,
            SourceUpdateHostOptions.FromEnvironment(
                typeof(SourceUpdateDemoState).Assembly)).Enable();

        left = new(palette, clock, 0f);
        right = new(palette, clock, MathF.PI);
        screen.Title = "Source Update — ORIGINAL METHOD — generation 0";
        screen.IsVisible = true;

        log.Info("SOURCE UPDATE DEMO");
        log.Info("LiveCode session: {0}", liveCode.Session.SessionId);
        log.Info("Edit PulseService.Step in the original file and apply its immutable diff.");
        log.Info("Both already-created PulseService instances will immediately execute the new IL.");
    }

    /// <inheritdoc />
    public override void Update(double delta)
    {
        leftReading = left.Step(delta);
        rightReading = right.Step(delta);
        if (leftReading.Updates % 20 == 0)
        {
            var generation = sourceUpdates.Capabilities().Modules.Single().Generation;
            screen.Title =
                $"Source Update — {leftReading.Label} — generation {generation} — " +
                $"instances {leftReading.Updates}/{rightReading.Updates}";
        }
    }

    /// <inheritdoc />
    public override void Render()
    {
        var blend = (leftReading.Color + rightReading.Color) * 0.5f;
        backbuffer.Clear((blend.X, blend.Y, blend.Z, 1f));
    }
}
