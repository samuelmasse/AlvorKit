namespace AlvorKit.UI.Blend;

/// <summary>Builds rounded Blend control geometry and resolves its reactive colors.</summary>
internal sealed class BlendStyleControlSurface
{
    /// <summary>Style façade supplying current palette, metrics, and text recipes.</summary>
    private readonly BlendStyle style;

    /// <summary>Generated rounded-cap texture cache.</summary>
    private readonly BlendControlChrome chrome;

    /// <summary>Creates rounded control rendering over the owning style and cap cache.</summary>
    internal BlendStyleControlSurface(BlendStyle style, BlendControlChrome chrome)
    {
        this.style = style;
        this.chrome = chrome;
    }

    /// <summary>Adds the rounded surface and reactive label to an existing control frame.</summary>
    internal void Apply(EntMut ent, Vec2 size, int fontSize, bool active)
    {
        RoundedControlSurface(ent, size, active);
        ControlLabel(ent, fontSize, active);
    }

    /// <summary>Resolves the control fill from disabled, active, pressed, and hovered state.</summary>
    private Vec4 ButtonFill(EntMut ent, bool active)
    {
        if (ent.IsInputDisabledFV.Resolve())
            return style.Palette.AppBackground;
        if (active)
            return style.Palette.ActiveSurface;
        if (ent.IsPressedR)
            return style.Palette.Selection;
        if (ent.IsHoveredR)
            return style.Palette.Hover;
        return style.Palette.Panel;
    }

    /// <summary>Resolves the control border from disabled, active, hover, and focus state.</summary>
    private Vec4 ButtonBorder(EntMut ent, bool active)
    {
        if (ent.IsInputDisabledFV.Resolve())
            return style.Palette.Border;
        if (active)
            return style.Palette.Accent;
        if (ent.IsHoveredR || ent.IsFocusedR)
            return style.Palette.StrongBorder;
        return style.Palette.Border;
    }

    /// <summary>Adds left and right caps plus the three-piece center fill.</summary>
    private void RoundedControlSurface(EntMut ent, Vec2 size, bool active)
    {
        var capWidth = style.Metrics.ControlRadius;
        var borderWidth = style.Metrics.ControlBorderWidth;
        var middleWidth = -(capWidth * 2f);

        Node(ent)
            .IsFloatingV(true)
            .SizeRelativeV((0, 0))
            .SizeV((capWidth, size.Y))
            .TextureF(() => ControlCap(ent, size.Y, active));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .SizeRelativeV((0, 0))
            .SizeV((capWidth, size.Y))
            .TextureF(() => ControlCap(ent, size.Y, active))
            .TextureFlipV(SpriteBatchFlip.Horizontal);

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active));

        Node(ent)
            .IsFloatingV(true)
            .OffsetV((capWidth, borderWidth))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, size.Y - (borderWidth * 2f)))
            .ColorF(() => ButtonFill(ent, active));

        Node(ent)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .OffsetV((capWidth, 0))
            .SizeRelativeV((1, 0))
            .SizeV((middleWidth, borderWidth))
            .ColorF(() => ButtonBorder(ent, active));
    }

    /// <summary>Gets the cached cap texture matching the control's current state.</summary>
    private Texture2D ControlCap(EntMut ent, float height, bool active) =>
        chrome.Cap(
            height,
            style.Metrics.ControlRadius,
            style.Metrics.ControlBorderWidth,
            ButtonFill(ent, active),
            ButtonBorder(ent, active));

    /// <summary>Adds a centered label that respects disabled and caller-supplied text colors.</summary>
    private void ControlLabel(EntMut ent, int fontSize, bool active) =>
        Node(ent)
            .IsFloatingV(true)
            .SizeRelativeV((1, 1))
            .Mutate(style.CenterText)
            .FontSizeV(fontSize)
            .TextColorF(() =>
            {
                if (ent.IsInputDisabledFV.Resolve())
                    return style.Palette.WithAlpha(style.Palette.MutedText, 0.45f);

                var requested = ent.TextColorFV.Resolve();
                if (requested.W > 0f)
                    return requested;

                return active
                    ? style.Palette.Text
                    : style.Palette.MutedText;
            })
            .TextF(() => ent.TextFV.Resolve());
}
