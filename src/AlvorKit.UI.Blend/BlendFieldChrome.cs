namespace AlvorKit.UI.Blend;

/// <summary>
/// Shared visual plumbing for Blend form fields: the field surface, reactive border rules, the edit-mode
/// selection/caret overlay, and UI-space text measurement that mirrors the draw system's scaling.
/// </summary>
internal sealed class BlendFieldChrome(BlendStyle style, RootUiScale uiScale, RootSprites sprites)
{
    /// <summary>Applies the label-inside field surface: full-width FieldHeight board over AppBackground.</summary>
    internal void Surface(EntMut ent) => ent.Mutate()
        .Mutate(style.Board)
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((1, 0))
        .SizeV((0, style.Metrics.FieldHeight))
        .ColorV(style.Palette.AppBackground)
        .IsSelectableV(true)
        .IsFocusableV(true);

    /// <summary>Adds the four reactive border rules; accent wins over hot/focus, which wins over the idle border.</summary>
    internal void Border(EntMut ent, Func<bool> accent, Func<bool> hot)
    {
        Vec4 Color() => accent()
            ? style.Palette.Accent
            : hot() || ent.IsFocusedR ? style.Palette.StrongBorder : style.Palette.Border;

        var hairline = style.Metrics.Hairline;
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (1, 0), (0, hairline), Color);
        BlendStyle.Rule(ent, Alignment.Bottom | Alignment.Left, (1, 0), (0, hairline), Color);
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Left, (0, 1), (hairline, 0), Color);
        BlendStyle.Rule(ent, Alignment.Top | Alignment.Right, (0, 1), (hairline, 0), Color);
    }

    /// <summary>Adds the muted field label, hidden while the field is being edited.</summary>
    internal void Label(EntMut field, string label, Func<bool> editing, Func<bool> inset)
    {
        Node(field)
            .Mutate(style.MutedCellLabel)
            .IsFloatingV(true)
            .TextAlignmentV(Alignment.Left | Alignment.Vertical)
            .TextPaddingF(() => (LabelInset(inset()), 0, 0, 0))
            .TextV(label)
            .IsDisabledF(() => editing());
    }

    /// <summary>Adds the selection fill for an edit session; declare it before the text node so glyphs draw over it.</summary>
    internal void EditSelection(EntMut field, BlendTextEdit edit, Func<float> textStart) =>
        Node(field)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Left | Alignment.Vertical)
            .ColorV(style.Palette.Selection)
            .SizeRelativeV((0, 0))
            .OffsetF(() => (textStart() + MeasureUi(edit.Span[..edit.Selection.Start]), 0))
            .SizeF(() => (MeasureUi(edit.Span[edit.Selection.Start..edit.Selection.End]), style.Metrics.CaretHeight))
            .IsDisabledF(() => !edit.IsActive || !edit.HasSelection);

    /// <summary>Adds the blinking caret for an edit session; declare it after the text node so it draws over glyphs.</summary>
    internal void EditCaret(EntMut field, BlendTextEdit edit, Func<float> textStart) =>
        Node(field)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Left | Alignment.Vertical)
            .ColorF(() => edit.IsCaretVisible ? style.Palette.Text : default)
            .SizeRelativeV((0, 0))
            .SizeV((style.Metrics.CaretWidth, style.Metrics.CaretHeight))
            .OffsetF(() => (textStart() + MeasureUi(edit.Span[..edit.Caret]), 0))
            .IsDisabledF(() => !edit.IsActive);

    /// <summary>Adds a node-built down caret; tiny font glyphs (▾ and friends) rasterize unreliably, bars do not.</summary>
    internal void DownCaret(EntMut field, float rightInset)
    {
        const float caretWidth = 7f;
        const float caretHeight = 4f;

        Node(field, out var caret)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Right | Alignment.Vertical)
            .OffsetV((-rightInset, 0))
            .SizeRelativeV((0, 0))
            .SizeV((caretWidth, caretHeight));
        for (var row = 0; row < 4; row++)
        {
            Node(caret)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Top | Alignment.Horizontal)
                .OffsetV((0, row))
                .SizeRelativeV((0, 0))
                .SizeV((caretWidth - (row * 2), 1))
                .ColorV(style.Palette.MutedText);
        }
    }

    /// <summary>Adds a node-built horizontal step arrow centered in its hit node.</summary>
    internal void SideArrow(EntMut hit, bool pointLeft)
    {
        const float arrowWidth = 4f;
        const float arrowHeight = 7f;

        Node(hit, out var arrow)
            .IsFloatingV(true)
            .AlignmentV(Alignment.Horizontal | Alignment.Vertical)
            .SizeRelativeV((0, 0))
            .SizeV((arrowWidth, arrowHeight));
        for (var column = 0; column < 4; column++)
        {
            var height = pointLeft ? 1 + (column * 2) : 7 - (column * 2);
            Node(arrow)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Left | Alignment.Vertical)
                .OffsetV((column, 0))
                .SizeRelativeV((0, 0))
                .SizeV((1, height))
                .ColorV(style.Palette.MutedText);
        }
    }

    /// <summary>Gets the left inset of edit-mode text inside a field.</summary>
    internal float TextLeft() => style.Metrics.FieldTextPadding;

    /// <summary>Gets the label inset, widened while hover arrows are visible.</summary>
    internal float LabelInset(bool arrows) =>
        style.Metrics.FieldTextPadding + (arrows ? style.Metrics.FieldArrowInset : 0);

    /// <summary>Measures text width in UI units the same way the draw system does: at the scaled pixel size, divided back.</summary>
    internal float MeasureUi(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return 0;

        var fontSize = (int)(style.Metrics.TextFontSize * uiScale.Scale);
        if (fontSize <= 0)
            return 0;

        return sprites.Batch.Measure(style.TextFont.Size(fontSize), text) / uiScale.Scale;
    }
}
