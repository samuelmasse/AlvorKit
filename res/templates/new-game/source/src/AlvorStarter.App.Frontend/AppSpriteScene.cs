namespace AlvorStarter.App.Frontend;

/// <summary>Draws simple starter shapes directly with the root sprite batch.</summary>
[App]
public class AppSpriteScene(RootCanvas canvas, RootSprites sprites)
{
    private static readonly Vec4 HeaderColor = (0.09f, 0.095f, 0.09f, 0.78f);
    private static readonly Vec4 BarColor = (0.94f, 0.70f, 0.32f, 0.82f);
    private static readonly Vec4 PulseColor = (0.28f, 0.52f, 1.0f, 0.75f);

    private double seconds;

    /// <summary>Advances the small sprite-batch animation timer.</summary>
    public void Update(double delta) => seconds += delta;

    /// <summary>Appends shape commands to the root sprite batch.</summary>
    public void Draw()
    {
        sprites.Batch.Draw((24, 24), (410, 166), HeaderColor);
        sprites.Batch.Draw((24, 24), (4, 166), BarColor);
        sprites.Batch.Draw((48, 52), (310, 22), BarColor);
        sprites.Batch.Draw((48, 88), (230, 14), (0.28f, 0.52f, 1.0f, 0.72f));
        sprites.Batch.Draw((48, 122), (160, 14), (0.20f, 0.76f, 0.50f, 0.72f));

        var x = 48f + (MathF.Sin((float)seconds * 2.2f) * 0.5f + 0.5f) * 204f;
        sprites.Batch.Draw((x, 154), (28, 28), PulseColor);

        var footerY = MathF.Max(32f, canvas.Size.Y - 40f);
        sprites.Batch.Draw((34, footerY), (180, 6), (0.72f, 0.75f, 0.72f, 0.7f));
    }
}
