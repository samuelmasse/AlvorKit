namespace AlvorKit.UI.Blend;

/// <summary>
/// Builders for Blend form controls: drag/edit number fields, bounded sliders, dropdown fields, text
/// fields, and checkboxes. Controls keep the label inside the field at <see cref="BlendMetrics.FieldHeight"/>;
/// per-field state (drag origin, edit session, format buffer) lives in build-time closures, so per-frame
/// callbacks stay allocation-free.
/// </summary>
public class BlendFields
{
    private readonly BlendStyle style;
    private readonly Keyboard keyboard;
    private readonly BlendDropdownMenu dropdown;
    private readonly BlendFieldChrome chrome;
    private readonly BlendDragField dragField;

    /// <summary>Creates the builders over the shared style, input roots, and dropdown popup.</summary>
    public BlendFields(
        BlendStyle style,
        RootUiScale uiScale,
        RootSprites sprites,
        RootUiMouse uiMouse,
        Keyboard keyboard,
        BlendDropdownMenu dropdown)
    {
        this.style = style;
        this.keyboard = keyboard;
        this.dropdown = dropdown;
        chrome = new(style, uiScale, sprites);
        dragField = new(style, chrome, uiMouse, keyboard);
    }

    /// <summary>Builds an unbounded float field: drag scrubs by step, arrows step, click or Enter edits inline.</summary>
    public EntMut NumberField(EntMut parent, BlendNumberFieldOptions o) =>
        dragField.Build(parent, o.Label, o.Get, o.Set, o.Step, o.Min, o.Max, o.Format, false, false);

    /// <summary>Builds a bounded float slider with a fill bar; dragging maps relative motion to the range.</summary>
    public EntMut SliderField(EntMut parent, BlendNumberFieldOptions o) =>
        dragField.Build(parent, o.Label, o.Get, o.Set, o.Step, o.Min, o.Max, o.Format, true, false);

    /// <summary>Builds an integer field: drag scrubs whole steps, arrows step by one, click or Enter edits inline.</summary>
    public EntMut IntField(EntMut parent, BlendIntFieldOptions o) =>
        dragField.Build(
            parent,
            o.Label,
            () => o.Get(),
            v => o.Set((int)MathF.Round(v)),
            1f,
            o.Min,
            o.Max,
            "0",
            false,
            true,
            o.Get,
            o.Set);

    /// <summary>Builds a dropdown field that opens the shared <see cref="BlendDropdownMenu"/> popup under itself.</summary>
    public EntMut DropdownField(EntMut parent, string label, IReadOnlyList<BlendDropdownItem> items, Func<int> get, Action<int> pick)
    {
        Node(parent, out var field)
            .Mutate(chrome.Surface)
            .CursorF(() => CursorShape.Hand)
            .OnPressF(() => dropdown.Open(field, items, get(), pick))
            .OnUpdateF(() =>
            {
                if (field.IsFocusedR && !dropdown.IsOpen && keyboard.IsKeyPressed(Keys.Enter))
                    dropdown.Open(field, items, get(), pick);
            })
            .ColorF(() => dropdown.IsOpenFor(field) ? style.Palette.Hover : style.Palette.AppBackground);
        {
            BlendDropdownItem Current() => items[Math.Clamp(get(), 0, items.Count - 1)];

            chrome.Label(field, label, () => false, () => false);

            var caretInset = style.Metrics.FieldTextPadding + style.Metrics.RightGlyphPadding;
            var valueInset = caretInset + style.Metrics.FieldTextPadding + style.Metrics.CompactSpacing;

            Node(field)
                .Mutate(style.CellLabel)
                .IsFloatingV(true)
                .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                .TextPaddingV((0, 0, valueInset, 0))
                .TextF(() => Current().Text);

            Node(field)
                .Mutate(style.Swatch)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Right | Alignment.Vertical)
                .OffsetF(() => (-(valueInset + chrome.MeasureUi(Current().Text) + style.Metrics.CompactSpacing + style.Metrics.SwatchWidth), 0))
                .ColorF(() => Current().Swatch)
                .IsDisabledF(() => Current().Swatch.W == 0);

            chrome.DownCaret(field, caretInset);
            chrome.Border(field, () => dropdown.IsOpenFor(field), () => field.IsHoveredR);
        }

        return field;
    }

    /// <summary>Builds a single-line text field; focusing it begins an edit with the text selected.</summary>
    public EntMut TextField(EntMut parent, BlendTextFieldOptions o)
    {
        var edit = new BlendTextEdit(keyboard);
        var wasFocused = false;

        Node(parent, out var field)
            .Mutate(chrome.Surface)
            .CursorF(() => CursorShape.Text)
            .OnClickF(() =>
            {
                if (!edit.IsActive)
                    edit.Begin(o.Get(), false);
            })
            .OnUpdateF(() =>
            {
                var focused = field.IsFocusedR;

                if (!edit.IsActive)
                {
                    if (focused && !wasFocused)
                        edit.Begin(o.Get(), true);
                }
                else if (!focused)
                {
                    Commit();
                }
                else
                {
                    switch (edit.Update())
                    {
                        case BlendTextEditResult.Commit:
                            Commit();
                            break;
                        case BlendTextEditResult.Cancel:
                            edit.End();
                            break;
                    }
                }

                wasFocused = focused;
            });
        {
            chrome.EditSelection(field, edit, chrome.TextLeft);

            Node(field)
                .Mutate(style.CellLabel)
                .IsFloatingV(true)
                .TextAlignmentV(Alignment.Left | Alignment.Vertical)
                .TextPaddingV((style.Metrics.FieldTextPadding, 0, style.Metrics.FieldTextPadding, 0))
                .TextColorF(() => Placeholding() ? style.Palette.MutedText : style.Palette.Text)
                .TextF(() => edit.IsActive ? edit.Span : Placeholding() ? o.Placeholder : o.Get());

            chrome.EditCaret(field, edit, chrome.TextLeft);
            chrome.Border(field, () => edit.IsActive, () => field.IsHoveredR);
        }

        return field;

        bool Placeholding() => !edit.IsActive && o.Get().Length == 0;

        void Commit()
        {
            o.Set(edit.Span.ToString());
            edit.End();
        }
    }

    /// <summary>Builds a checkbox row: 14px box plus label, with the whole row as the hit target.</summary>
    public EntMut Checkbox(EntMut parent, string label, Func<bool> get, Action toggle)
    {
        Node(parent, out var row)
            .Mutate(style.Board)
            .SizeWeightTypeV(SizeWeightType.Self)
            .SizeRelativeV((1, 0))
            .SizeV((0, style.Metrics.FieldHeight))
            .IsSelectableV(true)
            .IsFocusableV(true)
            .CursorF(() => CursorShape.Hand)
            .OnClickF(toggle)
            .Mutate(style.ActivateOnEnter);
        {
            var box = style.Metrics.CheckboxSize;

            Node(row, out var boxNode)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Left | Alignment.Vertical)
                .SizeRelativeV((0, 0))
                .SizeV((box, box))
                .ColorF(() => get() ? style.Palette.ActiveSurface : style.Palette.AppBackground);
            {
                Node(boxNode)
                    .IsFloatingV(true)
                    .SizeRelativeV((1, 1))
                    .Mutate(style.CenterText)
                    .FontSizeV(style.Metrics.CheckGlyphFontSize)
                    .TextColorV(style.Palette.Accent)
                    .TextV("✓")
                    .IsDisabledF(() => !get());

                chrome.Border(boxNode, get, () => row.IsHoveredR);
            }

            Node(row)
                .Mutate(style.CellLabel)
                .IsFloatingV(true)
                .TextAlignmentV(Alignment.Left | Alignment.Vertical)
                .TextPaddingV((box + style.Metrics.LooseSpacing, 0, 0, 0))
                .TextV(label);
        }

        return row;
    }
}
