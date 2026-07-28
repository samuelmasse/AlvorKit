using AlvorKit.Engine.LiveCode.Demo;
using AlvorKit.LivePatch;

/// <summary>Atomic second version used to demonstrate replacement without another ReJIT.</summary>
public sealed class ReverseOrbit(ColonySky sky)
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        receiver.Phase -= delta * 5.5;
        receiver.SolarAngle -= delta * 1.15;
        receiver.SolarRadius =
            0.34f + MathF.Sin((float)receiver.Phase * 0.41f) * 0.04f;
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
        receiver.Primary = (0.05f, 0.68f, 1f, 1f);
        receiver.Secondary = (1f, 0.2f, 0.62f, 1f);
        receiver.OrbitRadius = 142f;
        receiver.SporeCount = 38;
        receiver.Form = "atomic reverse orbit";
        sky.Warp = 0.78f;
        sky.Weather = "reversed temporal tide";
    }
}
