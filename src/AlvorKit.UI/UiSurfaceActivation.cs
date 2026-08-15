namespace AlvorKit;

/// <summary>Activates and restores the scale and viewport shared by one UI surface callback.</summary>
internal sealed class UiSurfaceActivation(
    RootUiScale scale,
    RootUiContext context)
{
    /// <summary>Activates a surface and returns the context needed to restore the previous one.</summary>
    internal UiSurfaceActiveState Activate(UiSurface surface)
    {
        var contextState = context.Activate(surface.ResolveViewport());
        try
        {
            var scaleState = scale.Activate(surface.ResolveScale());
            return new(scaleState, contextState);
        }
        catch
        {
            context.Restore(contextState);
            throw;
        }
    }

    /// <summary>Restores the scale and viewport active before the current surface.</summary>
    internal void Restore(UiSurfaceActiveState state)
    {
        scale.Restore(state.Scale);
        context.Restore(state.Context);
    }
}

/// <summary>Captures the scale and viewport active before a UI surface callback.</summary>
internal readonly record struct UiSurfaceActiveState(
    RootUiScale.ActiveState Scale,
    RootUiContext.ActiveState Context);
