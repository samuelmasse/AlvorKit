namespace AlvorKit.UI.Blend;

/// <summary>
/// Builds the numeric drag/edit fields behind <see cref="BlendFields"/>: label-inside surface, optional
/// slider fill, hover step arrows, drag scrubbing (Shift fine, Ctrl snap, Esc cancel), and an inline
/// <see cref="BlendTextEdit"/> session on click or focused-Enter. Per-field state lives in build-time
/// closures so the per-frame callbacks stay allocation-free; int-backed fields use the native int
/// accessors for display, arrow steps, and commits, because the float core cannot represent large ints.
/// </summary>
internal sealed class BlendDragField(
    BlendStyle style,
    BlendFieldChrome chrome,
    RootUiMouse uiMouse,
    Keyboard keyboard)
{
    private const float ArrowHitWidth = 12f;

    /// <summary>Builds one drag/edit field and returns its node; a finite range plus <paramref name="slider"/> adds the fill bar.</summary>
    internal EntMut Build(
        EntMut parent,
        string label,
        Func<float> get,
        Action<float> set,
        float step,
        float min,
        float max,
        string format,
        bool slider,
        bool whole,
        Func<int>? intGet = null,
        Action<int>? intSet = null)
    {
        var edit = new BlendTextEdit(keyboard);
        var buffer = new char[32];
        var pressPosition = Vec2.Zero;
        var startValue = 0f;
        var dragging = false;
        var dragCancelled = false;
        EntMut leftArrow = default;
        EntMut rightArrow = default;

        Node(parent, out var field)
            .Mutate(chrome.Surface)
            .CursorF(() => edit.IsActive ? CursorShape.Text : CursorShape.ResizeHorizontal)
            .OnPressF(() =>
            {
                pressPosition = uiMouse.Position;
                startValue = get();
                dragging = false;
                dragCancelled = false;
            })
            .OnClickF(() =>
            {
                if (!dragging && !edit.IsActive)
                    BeginEdit();
            })
            .OnUpdateF(() =>
            {
                if (edit.IsActive)
                    UpdateEdit();
                else if (field.IsPressedR)
                    UpdateDrag();
                else if (field.IsFocusedR && keyboard.IsKeyPressed(Keys.Enter))
                    BeginEdit();
            });
        {
            if (slider)
            {
                Node(field)
                    .IsFloatingV(true)
                    .OffsetV((1, 1))
                    .SizeRelativeV((0, 0))
                    .SizeF(() => ((field.SizeR.X - 2) * Fraction(), field.SizeR.Y - 2))
                    .ColorF(() => Scrubbing() ? style.Palette.ActiveSurface : style.Palette.Selection)
                    .IsDisabledF(() => edit.IsActive);
            }

            chrome.Label(field, label, static () => false, static () => true);
            chrome.EditSelection(field, edit, EditTextStart);

            // Editing happens in place: the text stays right-aligned where the value sits, and the
            // label and arrows stay put, so entering edit mode never reflows the field.
            Node(field)
                .Mutate(style.CellLabel)
                .IsFloatingV(true)
                .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                .TextPaddingV((0, 0, chrome.LabelInset(true), 0))
                .TextF(() => edit.IsActive ? edit.Span : FormatValue());

            chrome.EditCaret(field, edit, EditTextStart);

            Arrow(out leftArrow, Alignment.Left, true, () => StepBy(-1f));
            Arrow(out rightArrow, Alignment.Right, false, () => StepBy(1f));

            chrome.Border(field, () => edit.IsActive || Scrubbing(), Hot);
        }

        return field;

        float EditTextStart() => field.SizeR.X - chrome.LabelInset(true) - chrome.MeasureUi(edit.Span);

        void Arrow(out EntMut arrow, Alignment side, bool pointLeft, Action apply)
        {
            Node(field, out arrow)
                .IsFloatingV(true)
                .AlignmentV(side | Alignment.Vertical)
                .SizeRelativeV((0, 1))
                .SizeV((ArrowHitWidth, 0))
                .IsSelectableV(true)
                .OnClickF(() =>
                {
                    if (edit.IsActive)
                        CommitEdit();
                    apply();
                });
            chrome.SideArrow(arrow, pointLeft);
        }

        void StepBy(float direction)
        {
            if (intSet != null)
                ApplyInt(intGet!() + (long)direction);
            else
                Apply(get() + (step * direction));
        }

        bool Hot() => field.IsHoveredR || leftArrow.IsHoveredR || rightArrow.IsHoveredR;

        bool Scrubbing() => field.IsPressedR && dragging && !dragCancelled;

        float Fraction() => Math.Clamp((get() - min) / (max - min), 0f, 1f);

        void Apply(float value)
        {
            value = Math.Clamp(value, min, max);
            if (whole)
                value = MathF.Round(value);
            if (value != get())
                set(value);
        }

        ReadOnlySpan<char> FormatValue()
        {
            int written;
            if (intGet != null)
                intGet().TryFormat(buffer, out written, format, CultureInfo.InvariantCulture);
            else
                get().TryFormat(buffer, out written, format, CultureInfo.InvariantCulture);
            return buffer.AsSpan(0, written);
        }

        void BeginEdit() => edit.Begin(FormatValue(), true);

        void CommitEdit()
        {
            if (intSet != null && int.TryParse(edit.Span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                ApplyInt(intValue);
            else if (float.TryParse(edit.Span, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                Apply(value);
            edit.End();
        }

        void ApplyInt(long value)
        {
            var low = float.IsFinite(min) ? (long)min : int.MinValue;
            var high = float.IsFinite(max) ? (long)max : int.MaxValue;
            var clamped = (int)Math.Clamp(value, low, high);
            if (intSet != null && clamped != intGet!())
                intSet(clamped);
        }

        void UpdateEdit()
        {
            if (!field.IsFocusedR)
            {
                CommitEdit();
                return;
            }

            switch (edit.Update())
            {
                case BlendTextEditResult.Commit:
                    CommitEdit();
                    break;
                case BlendTextEditResult.Cancel:
                    edit.End();
                    break;
            }
        }

        void UpdateDrag()
        {
            if (keyboard.IsKeyPressed(Keys.Escape) && dragging)
            {
                Apply(startValue);
                dragCancelled = true;
            }

            if (dragCancelled)
                return;

            var dx = uiMouse.Position.X - pressPosition.X;
            if (!dragging && MathF.Abs(dx) > style.Metrics.DragDeadzone)
                dragging = true;
            if (!dragging)
                return;

            var fine = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift) ? 0.1f : 1f;
            var value = slider
                ? startValue + (dx / MathF.Max(field.SizeR.X, 1f) * (max - min) * fine)
                : startValue + (dx / style.Metrics.DragPixelsPerStep * step * fine);

            if (keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl))
                value = MathF.Round(value / step) * step;

            Apply(value);
        }
    }
}
