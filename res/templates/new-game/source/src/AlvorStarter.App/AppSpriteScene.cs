namespace AlvorStarter.App;

/// <summary>Draws starter text and simple shapes directly with the root sprite batch.</summary>
[App]
public class AppSpriteScene(RootCanvas canvas, RootSprites sprites, RootRoboto roboto)
{
    private static readonly Vec4 TextColor = (0.94f, 0.95f, 0.92f, 1f);
    private static readonly Vec4 MutedTextColor = (0.72f, 0.75f, 0.72f, 1f);
    private static readonly Vec4 HeaderColor = (0.09f, 0.095f, 0.09f, 0.78f);
    private static readonly Vec4 BarColor = (0.94f, 0.70f, 0.32f, 0.82f);
    private static readonly Vec4 PulseColor = (0.28f, 0.52f, 1.0f, 0.75f);

    private double seconds;

    /// <summary>Advances the small sprite-batch animation timer.</summary>
    public void Update(double delta) => seconds += delta;

    /// <summary>Appends text and shape commands to the root sprite batch.</summary>
    public void Draw()
    {
        var font = roboto[30];
        var small = roboto[16];
        sprites.Batch.Draw((24, 24), (410, 166), HeaderColor);
        sprites.Batch.Draw((24, 24), (4, 166), BarColor);

        sprites.Batch.Write(font, "HELLO ALVOR", (46, 42), TextColor);
        sprites.Batch.Write(small, "raw GL triangle | RootSprites batch | Blend UI", (48, 82), MutedTextColor);

        sprites.Batch.Draw((48, 116), (240, 8), BarColor);
        sprites.Batch.Draw((48, 132), (164, 8), (0.28f, 0.52f, 1.0f, 0.72f));

        var x = 48f + (MathF.Sin((float)seconds * 2.2f) * 0.5f + 0.5f) * 204f;
        sprites.Batch.Draw((x, 154), (28, 28), PulseColor);

        var footerY = MathF.Max(32f, canvas.Size.Y - 40f);
        sprites.Batch.Write(small, "Esc closes the window", (34, footerY), MutedTextColor);
    }
}
