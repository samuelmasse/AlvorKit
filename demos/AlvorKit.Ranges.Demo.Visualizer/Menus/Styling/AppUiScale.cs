namespace AlvorKit;

/// <summary>Adapts visualizer UI scale commands to the engine UI scale roots.</summary>
[App]
public class AppUiScale(RootScale rootScale, RootUi ui)
{
    public float Scale => ui.Scale.Scale;

    public void ScaleUp()
    {
        const int maximumScaleMultiplier = 4;
        rootScale.Numerator = Math.Min(rootScale.Denominator * maximumScaleMultiplier, rootScale.Numerator + 1);
        ui.Scale.Scale = rootScale.Scale;
    }

    public void ScaleDown()
    {
        rootScale.Numerator = Math.Max(1, rootScale.Numerator - 1);
        ui.Scale.Scale = rootScale.Scale;
    }
}
