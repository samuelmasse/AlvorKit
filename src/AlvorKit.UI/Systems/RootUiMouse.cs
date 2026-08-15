namespace AlvorKit;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiFocus focus, RootUiClipping clipping)
{
    private const long DoubleClickMs = 500;

    private readonly UiMouseHitSearch hitSearch = new(clipping);
    private Vec2 position;
    private EntMut prevHovered;
    private EntMut pressedMain;
    private EntMut pressedSecondary;
    private UiSurface? hoveredSurface;
    private bool prevMainDown;
    private bool prevSecondaryDown;
    private EntMut lastClickTarget;
    private long lastClickTicks;

    public Vec2 Position => position;
    public EntMut Hovered => prevHovered;

    internal void Hover(RootUiSurfaces surfaces)
    {
        if (CursorGrabbed())
        {
            ClearHovered();
            return;
        }

        var hovered = default(EntMut);
        UiSurface? surface = null;
        var span = surfaces.Span;
        for (var index = span.Length - 1; index >= 0; index--)
        {
            var candidate = span[index];
            var active = surfaces.Activate(candidate);
            try
            {
                position = LocalPosition(candidate);
                hovered = hitSearch.Hovered(position, candidate.Root);
                if (hovered != default)
                {
                    surface = candidate;
                    break;
                }
            }
            finally
            {
                surfaces.Restore(active);
            }
        }

        hoveredSurface = surface;
        if (surface is null)
            position = LocalPosition(surfaces.Default);

        if (hovered != prevHovered)
        {
            prevHovered.IsHoveredR = false;
            hovered.IsHoveredR = true;
            prevHovered = hovered;
        }
    }

    internal void Draw()
    {
        mouse.CursorShape = prevHovered != default
            ? (prevHovered.CursorFV.Resolve() ?? CursorShape.Default)
            : CursorShape.Default;
    }

    /// <summary>Clears transient hover and press state and releases the hardware cursor shape.</summary>
    internal void Unload()
    {
        ClearHovered();
        ClearPressed();
        mouse.CursorShape = CursorShape.Default;
    }

    internal void Update(RootUiSurfaces surfaces)
    {
        if (CursorGrabbed())
        {
            ClearPressed();
            prevMainDown = mouse.IsMainDown();
            prevSecondaryDown = mouse.IsSecondaryDown();
            return;
        }

        DispatchScroll(surfaces);

        var active = hoveredSurface is null
            ? default(UiSurfaceActiveState?)
            : surfaces.Activate(hoveredSurface);
        try
        {
            if (mouse.IsMainDown())
            {
                if (!prevMainDown)
                {
                    pressedMain = prevHovered;
                    if (pressedMain != default)
                        OnLeftPress(pressedMain);
                }

                prevMainDown = true;
            }
            else
            {
                if (prevMainDown && pressedMain != default && pressedMain == prevHovered)
                    OnLeftClick(pressedMain);

                pressedMain.IsPressedR = false;
                pressedMain = default;
                prevMainDown = false;
            }

            if (mouse.IsSecondaryDown())
            {
                if (!prevSecondaryDown)
                {
                    pressedSecondary = prevHovered;
                    if (pressedSecondary != default)
                        OnRightPress(pressedSecondary);
                }

                prevSecondaryDown = true;
            }
            else
            {
                if (prevSecondaryDown && pressedSecondary != default && pressedSecondary == prevHovered)
                    OnRightClick(pressedSecondary);

                pressedSecondary.IsSecondaryPressedR = false;
                pressedSecondary = default;
                prevSecondaryDown = false;
            }
        }
        finally
        {
            if (active.HasValue)
                surfaces.Restore(active.GetValueOrDefault());
        }
    }

    private void DispatchScroll(RootUiSurfaces surfaces)
    {
        if (mouse.Wheel == default)
            return;

        var hoverPosition = position;
        var span = surfaces.Span;
        for (var index = span.Length - 1; index >= 0; index--)
        {
            var surface = span[index];
            var active = surfaces.Activate(surface);
            try
            {
                position = LocalPosition(surface);
                var scrolled = hitSearch.Scrolled(position, surface.Root);
                if (scrolled == default)
                    continue;

                scrolled.OnScrollFV.Resolve()?.Invoke(mouse.Wheel);
                return;
            }
            finally
            {
                surfaces.Restore(active);
                position = hoverPosition;
            }
        }
    }

    private Vec2 LocalPosition(UiSurface surface) =>
        (mouse.Position - surface.CurrentViewport.Min)
        / surface.CurrentScale;

    private void OnLeftPress(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        if (e.IsFocusableFV.Resolve() || e.IsSilentFocusableFV.Resolve())
            focus.Focus(e, false);

        e.IsPressedR = true;
        e.OnPressFV.Resolve()?.Invoke();
    }

    private void OnLeftClick(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        var now = Environment.TickCount64;

        if (lastClickTarget == e && now - lastClickTicks <= DoubleClickMs && e.OnDoubleClickFV.Resolve() != null)
        {
            e.OnDoubleClickFV.Resolve()?.Invoke();
            lastClickTarget = default;
            lastClickTicks = 0;
        }
        else
        {
            e.OnClickFV.Resolve()?.Invoke();
            lastClickTarget = e;
            lastClickTicks = now;
        }
    }

    private void OnRightPress(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        e.IsSecondaryPressedR = true;
        e.OnSecondaryPressFV.Resolve()?.Invoke();
    }

    private void OnRightClick(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryClickFV.Resolve()?.Invoke();
    }

    private bool InputEnabled(EntMut n) => !n.IsInputDisabledFV.Resolve();

    private bool CursorGrabbed() => mouse.CursorMode is CursorMode.Disabled or CursorMode.Captured;

    private void ClearHovered()
    {
        prevHovered.IsHoveredR = false;
        prevHovered = default;
        hoveredSurface = null;
    }

    private void ClearPressed()
    {
        pressedMain.IsPressedR = false;
        pressedMain = default;
        pressedSecondary.IsSecondaryPressedR = false;
        pressedSecondary = default;
    }
}
