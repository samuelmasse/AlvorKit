namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Adapts visualizer UI scale commands to the engine UI scale roots.</summary>
[App]
public class AppUiScale(RootScale rootScale, RootUiScale uiScale)
{
    public float Scale => uiScale.Scale;

    public void ScaleUp()
    {
        const int maximumScaleMultiplier = 4;
        rootScale.Numerator = Math.Min(rootScale.Denominator * maximumScaleMultiplier, rootScale.Numerator + 1);
        uiScale.Scale = rootScale.Scale;
    }

    public void ScaleDown()
    {
        rootScale.Numerator = Math.Max(1, rootScale.Numerator - 1);
        uiScale.Scale = rootScale.Scale;
    }
}
