namespace AlvorKit;

/// <summary>Implements Blend button, chip, field, and keyboard-activation recipes.</summary>
internal class BlendStyleControls
{
    /// <summary>Style façade supplying current palette, metrics, and shared recipes.</summary>
    private readonly BlendStyle s;

    /// <summary>Regular Inter face used to measure control labels.</summary>
    private readonly Font font;

    /// <summary>Keyboard root used for focus activation.</summary>
    private readonly RootKeyboard keyboard;

    /// <summary>Rounded surface renderer shared by every interactive control.</summary>
    private readonly BlendStyleControlSurface surface;

    /// <summary>Creates control recipes over the owning style and runtime collaborators.</summary>
    internal BlendStyleControls(BlendStyle s, Font font, GlLayer gl, RootUiScale scale, RootKeyboard keyboard)
    {
        this.s = s;
        this.font = font;
        this.keyboard = keyboard;
        surface = new(s, new(gl, scale));
    }

    /// <summary>Builds a compact rounded button using the standard Blend button font size.</summary>
    internal void Button(EntMut ent) =>
        Button(ent, s.Metrics.ButtonHeight, s.Metrics.ButtonFontSize, s.Metrics.ButtonTextPadding, false);

    /// <summary>Builds an active compact rounded button using the standard Blend button font size.</summary>
    internal void ActiveButton(EntMut ent) =>
        Button(ent, s.Metrics.ButtonHeight, s.Metrics.ButtonFontSize, s.Metrics.ButtonTextPadding, true);

    /// <summary>Builds a compact rounded button sized for title rows and toolbar strips.</summary>
    internal void ToolbarButton(EntMut ent)
    {
        Button(ent, s.Metrics.ToolbarButtonHeight, s.Metrics.ButtonFontSize, s.Metrics.ButtonTextPadding, false);
        ent.Mutate().OffsetV((0, -s.Metrics.Hairline));
    }

    /// <summary>Builds an active compact rounded button sized for title rows and toolbar strips.</summary>
    internal void ActiveToolbarButton(EntMut ent)
    {
        Button(ent, s.Metrics.ToolbarButtonHeight, s.Metrics.ButtonFontSize, s.Metrics.ButtonTextPadding, true);
        ent.Mutate().OffsetV((0, -s.Metrics.Hairline));
    }

    /// <summary>Builds a compact square button using the standard Blend square-button font size.</summary>
    internal void SquareButton(EntMut ent) =>
        FixedButton(ent, (s.Metrics.SquareButtonSize, s.Metrics.SquareButtonSize), s.Metrics.SquareButtonFontSize, false);

    /// <summary>Builds an active compact square button using the standard Blend square-button font size.</summary>
    internal void ActiveSquareButton(EntMut ent) =>
        FixedButton(ent, (s.Metrics.SquareButtonSize, s.Metrics.SquareButtonSize), s.Metrics.SquareButtonFontSize, true);

    /// <summary>Applies a smaller toolbar chip.</summary>
    internal void Chip(EntMut ent) =>
        Button(ent, s.Metrics.ChipHeight, s.Metrics.ChipFontSize, s.Metrics.ChipTextPadding, false);

    /// <summary>Applies a non-interactive readout chip that remains hoverable for tooltips.</summary>
    internal void ReadoutChip(EntMut ent) => ent.Mutate()
        .Mutate(s.Board)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 0))
        .SizeV((0, s.Metrics.ChipHeight))
        .FontV(font)
        .FontSizeV(s.Metrics.ChipFontSize)
        .TextPaddingV((s.Metrics.ChipTextPadding, 0, s.Metrics.ChipTextPadding, 0))
        .TextAlignmentV(Alignment.Center)
        .TextColorV(s.Palette.MutedText)
        .ColorV(s.Palette.Panel)
        .IsSelectableV(true)
        .Mutate(s.Border);

    /// <summary>Applies a static field-like surface.</summary>
    internal void Field(EntMut ent) => ent.Mutate()
        .Mutate(s.Text)
        .SizeRelativeV((1, 0))
        .SizeV((0, s.Metrics.FieldHeight))
        .TextPaddingV((s.Metrics.FieldTextPadding, 0, s.Metrics.FieldTextPadding, 0))
        .TextColorV(s.Palette.MutedText)
        .ColorV(s.Palette.AppBackground)
        .Mutate(s.Border);

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
        .Mutate(s.Board)
        .SizeRelativeV((0, 0))
        .SizeV(size)
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand)
        .Mutate(s.ActivateOnEnter);

    /// <summary>Applies common focus and pointer behavior to a text-measured button.</summary>
    private void MeasuredButtonFrame(EntMut ent, float height, int fontSize, float horizontalPadding) => ent.Mutate()
        .Mutate(s.Board)
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
        .Mutate(s.ActivateOnEnter);
}
