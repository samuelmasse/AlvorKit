namespace AlvorKit.UI;

[Root]
public class RootUiScale(RootScale rscale)
{
    private float scale = rscale.Scale;
    private float activeScale;
    private bool isActive;

    /// <summary>
    /// Gets the scale of the UI surface currently being processed, or the configurable
    /// default-surface scale outside UI processing. Setting it changes the default surface.
    /// </summary>
    public float Scale
    {
        get => isActive ? activeScale : scale;
        set => scale = value;
    }

    internal float DefaultScale => scale;

    internal ActiveState Activate(float value)
    {
        var previous = new ActiveState(isActive, activeScale);
        isActive = true;
        activeScale = value;
        return previous;
    }

    internal void Restore(ActiveState state)
    {
        isActive = state.IsActive;
        activeScale = state.Scale;
    }

    internal readonly record struct ActiveState(bool IsActive, float Scale);
}
