namespace AlvorKit.UI;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiScale scale, RootUiFocus focus, RootUiClipping clipping)
{
    private const long DoubleClickMs = 500;

    private Vec2 position;
    private EntMut prevHovered;
    private EntMut pressedMain;
    private EntMut pressedSecondary;
    private bool prevMainDown;
    private bool prevSecondaryDown;
    private EntMut lastClickTarget;
    private long lastClickTicks;

    public Vec2 Position => position;
    public EntMut Hovered => prevHovered;

    internal void Hover(EntMut n)
    {
        position = mouse.Position / scale.Scale;

        if (CursorGrabbed())
        {
            ClearHovered();
            return;
        }

        var hovered = FindHovered(null, n, false);
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

    internal void Update(EntMut n)
    {
        if (CursorGrabbed())
        {
            ClearPressed();
            prevMainDown = mouse.IsMainDown();
            prevSecondaryDown = mouse.IsSecondaryDown();
            return;
        }

        var scrolled = FindScrolled(null, n, false);
        if (mouse.Wheel != default)
            scrolled.OnScrollFV.Resolve()?.Invoke(mouse.Wheel);

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

    private EntMut FindHovered(Box2? clip, EntMut n, bool inputDisabled)
    {
        var box = clipping.IntersectClips(clip, new Box2(n.PositionR, n.PositionR + n.SizeR));
        inputDisabled |= n.IsInputDisabledFV.Resolve();

        EntMut hovered = default;

        if (!inputDisabled && box.ContainsInclusive(position) && n.IsSelectableFV.Resolve())
            hovered = n;

        foreach (var c in n.NodesR.Span)
        {
            var child = FindHovered(box, c, inputDisabled);
            if (child != default)
                hovered = child;
        }

        return hovered;
    }

    private EntMut FindScrolled(Box2? clip, EntMut n, bool inputDisabled)
    {
        var box = clipping.IntersectClips(clip, new Box2(n.PositionR, n.PositionR + n.SizeR));
        inputDisabled |= n.IsInputDisabledFV.Resolve();

        EntMut scrolled = default;

        if (!inputDisabled && box.ContainsInclusive(position) && n.IsScrollableFV.Resolve())
            scrolled = n;

        foreach (var c in n.NodesR.Span)
        {
            var child = FindScrolled(box, c, inputDisabled);
            if (child != default)
                scrolled = child;
        }

        return scrolled;
    }

    private bool InputEnabled(EntMut n) => !n.IsInputDisabledFV.Resolve();

    private bool CursorGrabbed() => mouse.CursorMode is CursorMode.Disabled or CursorMode.Captured;

    private void ClearHovered()
    {
        prevHovered.IsHoveredR = false;
        prevHovered = default;
    }

    private void ClearPressed()
    {
        pressedMain.IsPressedR = false;
        pressedMain = default;
        pressedSecondary.IsSecondaryPressedR = false;
        pressedSecondary = default;
    }
}
