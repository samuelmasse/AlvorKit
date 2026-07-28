namespace AlvorKit.UI.Blend;

/// <summary>Implements Blend button, chip, field, and keyboard-activation recipes.</summary>
internal sealed class BlendStyleControls
{
    /// <summary>Style façade supplying current palette, metrics, and shared recipes.</summary>
    private readonly BlendStyle style;

    /// <summary>Regular Inter face used to measure control labels.</summary>
    private readonly Font font;

    /// <summary>Keyboard root used for focus activation.</summary>
    private readonly RootKeyboard keyboard;

    /// <summary>Rounded surface renderer shared by every interactive control.</summary>
    private readonly BlendStyleControlSurface surface;

    /// <summary>Creates control recipes over the owning style and runtime collaborators.</summary>
    internal BlendStyleControls(BlendStyle style, Font font, GlLayer gl, RootUiScale scale, RootKeyboard keyboard)
    {
        this.style = style;
        this.font = font;
        this.keyboard = keyboard;
        surface = new(style, new(gl, scale));
    }

    /// <summary>Builds a compact rounded button using the standard Blend button font size.</summary>
    internal void Button(EntMut ent) =>
        Button(ent, style.Metrics.ButtonHeight, style.Metrics.ButtonFontSize, style.Metrics.ButtonTextPadding, false);

    /// <summary>Builds an active compact rounded button using the standard Blend button font size.</summary>
    internal void ActiveButton(EntMut ent) =>
        Button(ent, style.Metrics.ButtonHeight, style.Metrics.ButtonFontSize, style.Metrics.ButtonTextPadding, true);

    /// <summary>Builds a compact rounded button sized for title rows and toolbar strips.</summary>
    internal void ToolbarButton(EntMut ent)
    {
        Button(ent, style.Metrics.ToolbarButtonHeight, style.Metrics.ButtonFontSize, style.Metrics.ButtonTextPadding, false);
        ent.Mutate().OffsetV((0, -style.Metrics.Hairline));
    }

    /// <summary>Builds an active compact rounded button sized for title rows and toolbar strips.</summary>
    internal void ActiveToolbarButton(EntMut ent)
    {
        Button(ent, style.Metrics.ToolbarButtonHeight, style.Metrics.ButtonFontSize, style.Metrics.ButtonTextPadding, true);
        ent.Mutate().OffsetV((0, -style.Metrics.Hairline));
    }

    /// <summary>Builds a compact square button using the standard Blend square-button font size.</summary>
    internal void SquareButton(EntMut ent) =>
        FixedButton(ent, (style.Metrics.SquareButtonSize, style.Metrics.SquareButtonSize), style.Metrics.SquareButtonFontSize, false);

    /// <summary>Builds an active compact square button using the standard Blend square-button font size.</summary>
    internal void ActiveSquareButton(EntMut ent) =>
        FixedButton(ent, (style.Metrics.SquareButtonSize, style.Metrics.SquareButtonSize), style.Metrics.SquareButtonFontSize, true);

    /// <summary>Applies a smaller toolbar chip.</summary>
    internal void Chip(EntMut ent) =>
        Button(ent, style.Metrics.ChipHeight, style.Metrics.ChipFontSize, style.Metrics.ChipTextPadding, false);

    /// <summary>Applies a non-interactive readout chip that remains hoverable for tooltips.</summary>
    internal void ReadoutChip(EntMut ent) => ent.Mutate()
        .Mutate(style.Board)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, style.Metrics.ChipHeight))
        .FontV(font)
        .FontSizeV(style.Metrics.ChipFontSize)
        .TextPaddingV((style.Metrics.ChipTextPadding, 0, style.Metrics.ChipTextPadding, 0))
        .TextAlignmentV(Alignment.Center)
        .TextColorV(style.Palette.MutedText)
        .ColorV(style.Palette.Panel)
        .IsSelectableV(true)
        .Mutate(style.Border);

    /// <summary>Applies a static field-like surface.</summary>
    internal void Field(EntMut ent) => ent.Mutate()
        .Mutate(style.Text)
        .SizeRelativeV((1, 0))
        .SizeV((0, style.Metrics.FieldHeight))
        .TextPaddingV((style.Metrics.FieldTextPadding, 0, style.Metrics.FieldTextPadding, 0))
        .TextColorV(style.Palette.MutedText)
        .ColorV(style.Palette.AppBackground)
        .Mutate(style.Border);

    /// <summary>Runs the node's click or press callback when it is focused and Enter is pressed.</summary>
    internal void ActivateOnEnter(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate()
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                {
                    var click = ent.OnClickFV.Resolve();
                    if (click != null)
                        click();
                    else
                        ent.OnPressFV.Resolve()?.Invoke();
                }

                enterWasDown = enterDown;
            });
    }

    /// <summary>Builds a fixed-size control frame, rounded surface, and label.</summary>
    private void FixedButton(EntMut ent, Vec2 size, int fontSize, bool active)
    {
        ButtonFrame(ent, size);
        surface.Apply(ent, size, fontSize, active);
    }

    /// <summary>Builds a text-measured control frame, rounded surface, and label.</summary>
    private void Button(EntMut ent, float height, int fontSize, float horizontalPadding, bool active)
    {
        MeasuredButtonFrame(ent, height, fontSize, horizontalPadding);
        surface.Apply(ent, (0, height), fontSize, active);
    }

    /// <summary>Applies common focus and pointer behavior to a fixed-size button.</summary>
    private void ButtonFrame(EntMut ent, Vec2 size) => ent.Mutate()
        .Mutate(style.Board)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(style.ActivateOnEnter);

    /// <summary>Applies common focus and pointer behavior to a text-measured button.</summary>
    private void MeasuredButtonFrame(EntMut ent, float height, int fontSize, float horizontalPadding) => ent.Mutate()
        .Mutate(style.Board)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, height))
        .FontV(font)
        .FontSizeV(fontSize)
        .TextPaddingV((horizontalPadding, 0, horizontalPadding, 0))
        .TextColorV(default)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(style.ActivateOnEnter);
}
