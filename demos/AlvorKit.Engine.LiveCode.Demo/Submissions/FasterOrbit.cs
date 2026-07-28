using AlvorKit.Engine.LiveCode.Demo;
using AlvorKit.LivePatch;
using AlvorKit.Maths;

/// <summary>
/// Exact replacement for ColonyGarden.Update, constructed inside the selected
/// colony scope so it can use that colony's own atmosphere dependency.
/// </summary>
public sealed class FasterOrbit(ColonySky sky)
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        receiver.Phase += delta * 8.5;
        receiver.SolarAngle += delta * 1.65;
        receiver.SolarRadius =
            0.27f + MathF.Sin((float)receiver.Phase * 0.23f) * 0.055f;
        receiver.Anchor =
        (
            0.5f + MathF.Cos((float)receiver.SolarAngle) * receiver.SolarRadius,
            0.5f + MathF.Sin((float)receiver.SolarAngle) *
                receiver.SolarRadius *
                ColonyGarden.SolarVerticalScale
        );
        receiver.SolarAngle = Math.Atan2(
            (receiver.Anchor.Y - 0.5f) / ColonyGarden.SolarVerticalScale,
            receiver.Anchor.X - 0.5f);
        receiver.Primary =
        (
            0.55f + MathF.Sin((float)receiver.Phase) * 0.35f,
            0.18f,
            1f,
            1f
        );
        receiver.Secondary = (0.08f, 1f, 0.86f, 1f);
        receiver.OrbitRadius =
            108f + MathF.Sin((float)receiver.Phase * 0.7f) * 28f;
        receiver.SporeCount = 58;
        receiver.Form = "live-patched solar helix";
        sky.Warp =
            0.42f + MathF.Sin((float)receiver.Phase * 0.31f) * 0.32f;
        sky.Weather = "agent-authored chromatic storm";
    }
}
