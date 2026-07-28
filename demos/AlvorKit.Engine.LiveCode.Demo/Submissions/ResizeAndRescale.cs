namespace AgentSubmissions;

using AlvorKit.Engine.Loop;
using AlvorKit.Graphics2D;
using AlvorKit.Graphics2D.Fonts;

[Root]
public sealed class ResizeAndRescale(
    RootScope root,
    RootScripts scripts,
    RootScreen screen,
    RootCanvas canvas,
    RootScale scale,
    UniverseColonies universe) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        var previousSize = canvas.Size;
        var previousScale = scale.Scale;

        screen.Size = (1680u, 1050u);
        scale.Numerator = 3;
        scale.Denominator = 2;

        universe.NetworkColor = (0.18f, 1f, 0.76f, 0.9f);
        universe.NetworkIntensity = 1f;
        universe.LastIntervention = "Agent resized the live window and raised UI scale to 150%.";
        foreach (var colony in universe.Span)
            colony.Garden.Burst(2.2f);

        var overlayPresent = false;
        foreach (var script in scripts.Span)
        {
            if (script.GetType().FullName == typeof(LiveScaleOverlay).FullName)
                overlayPresent = true;
        }

        if (!overlayPresent)
            scripts.Add(root.New<LiveScaleOverlay>());

        output.WriteLine("Resized and rescaled the currently running engine window.");
        output.Value("previousSize", previousSize);
        output.Value("requestedSize", "1680 x 1050");
        output.Value("previousUiScale", previousScale);
        output.Value("newUiScale", scale.Scale);
        output.Value("liveOverlayAdded", !overlayPresent);
    }
}

[Root]
public sealed class LiveScaleOverlay(
    RootCanvas canvas,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale) : Script
{
    private double time;

    public override float Order => 10_000f;

    public override void Update(double delta) => time += delta;

    public override void Draw()
    {
        Vec2 size = canvas.Size;
        var ui = scale.Scale;
        var pulse = 0.72f + MathF.Sin((float)time * 4f) * 0.18f;
        var sweep = (float)(time * 0.16 % 1d) * size.X;
        var border = 4f * ui;
        var bannerHeight = 108f * ui;

        sprites.Batch.Draw((0f, 0f), (size.X, bannerHeight), (0.015f, 0.025f, 0.07f, 0.94f));
        sprites.Batch.Draw((0f, 0f), (size.X, border), (0.14f, 1f, 0.74f, pulse));
        sprites.Batch.Draw((0f, size.Y - border), (size.X, border), (1f, 0.18f, 0.76f, pulse));
        sprites.Batch.Draw((0f, 0f), (border, size.Y), (0.14f, 1f, 0.74f, pulse));
        sprites.Batch.Draw((size.X - border, 0f), (border, size.Y), (1f, 0.18f, 0.76f, pulse));
        sprites.Batch.Draw((sweep, 0f), (8f * ui, size.Y), (0.2f, 0.95f, 1f, 0.12f));

        var titleFont = roboto[scale[27]];
        var detailFont = roboto[scale[14]];
        sprites.Batch.Write(
            titleFont,
            "LIVE CODE REWIRED THIS RUNNING WINDOW",
            (28f * ui, 19f * ui),
            (0.86f, 1f, 0.96f, 1f));
        sprites.Batch.Write(
            detailFont,
            text.Format(
                "{0:0} x {1:0} physical pixels   /   UI scale {2:0.00}   /   zero restarts",
                size.X,
                size.Y,
                scale.Scale),
            (31f * ui, 63f * ui),
            (0.24f, 1f, 0.75f, 1f));
        sprites.Batch.Write(
            detailFont,
            "This overlay class was compiled, injected, and attached to RootScripts while the game was running.",
            (31f * ui, 84f * ui),
            (0.66f, 0.74f, 0.9f, 1f));
    }
}
