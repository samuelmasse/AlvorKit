namespace AlvorKit.UI.Blend;

/// <summary>
/// Single-line editable text state: buffer, caret, selection, and key handling. One instance backs one
/// editable field; the field's number edit mode and <c>TextField</c> share this engine. Editing is a cold
/// path, so the backing buffer may grow (allocate) while typing.
/// </summary>
public class BlendTextEdit(Keyboard keyboard)
{
    private char[] buffer = new char[32];
    private int length;
    private int caret;
    private int anchor;
    private bool active;
    private long blinkEpoch;

    /// <summary>Gets whether an edit session is active.</summary>
    public bool IsActive => active;

    /// <summary>Gets the current text.</summary>
    public ReadOnlySpan<char> Span => buffer.AsSpan(0, length);

    /// <summary>Gets the caret position in characters from the start of the text.</summary>
    public int Caret => caret;

    /// <summary>Gets the selection range; start equals end when nothing is selected.</summary>
    public (int Start, int End) Selection => caret < anchor ? (caret, anchor) : (anchor, caret);

    /// <summary>Gets whether a non-empty selection exists.</summary>
    public bool HasSelection => caret != anchor;

    /// <summary>Gets whether the caret is in the visible half of its blink period; typing keeps it visible.</summary>
    public bool IsCaretVisible => ((Environment.TickCount64 - blinkEpoch) % BlinkMs) < (BlinkMs / 2);

    private long BlinkMs => 1000;

    /// <summary>Starts an edit session over the supplied initial text.</summary>
    public void Begin(ReadOnlySpan<char> initial, bool selectAll)
    {
        Reserve(initial.Length);
        initial.CopyTo(buffer);
        length = initial.Length;
        caret = length;
        anchor = selectAll ? 0 : length;
        active = true;
        blinkEpoch = Environment.TickCount64;
    }

    /// <summary>Ends the edit session without signaling a commit or cancel.</summary>
    public void End() => active = false;

    /// <summary>
    /// Consumes this tick's text runes and edit keys. Call once per update while active; the caller applies
    /// <see cref="BlendTextEditResult.Commit"/> by parsing <see cref="Span"/> and ends the session either way.
    /// </summary>
    public BlendTextEditResult Update()
    {
        var text = keyboard.Text;
        for (var i = 0; i < text.Count; i++)
            InsertRune(text[i]);

        var shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        var control = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

        if (control && keyboard.IsKeyPressed(Keys.A))
        {
            anchor = 0;
            caret = length;
            Wake();
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Left))
            MoveCaret(HasSelection && !shift ? Selection.Start : Math.Max(0, caret - 1), shift);
        if (keyboard.IsKeyPressedRepeated(Keys.Right))
            MoveCaret(HasSelection && !shift ? Selection.End : Math.Min(length, caret + 1), shift);
        if (keyboard.IsKeyPressedRepeated(Keys.Home))
            MoveCaret(0, shift);
        if (keyboard.IsKeyPressedRepeated(Keys.End))
            MoveCaret(length, shift);

        if (keyboard.IsKeyPressedRepeated(Keys.Backspace))
        {
            if (!HasSelection && caret > 0)
                MoveCaret(caret - 1, true);
            RemoveSelection();
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Delete))
        {
            if (!HasSelection && caret < length)
                MoveCaret(caret + 1, true);
            RemoveSelection();
        }

        if (keyboard.IsKeyPressed(Keys.Enter) || keyboard.IsKeyPressed(Keys.Tab))
            return BlendTextEditResult.Commit;
        if (keyboard.IsKeyPressed(Keys.Escape))
            return BlendTextEditResult.Cancel;

        return BlendTextEditResult.None;
    }

    private void InsertRune(Rune rune)
    {
        if (Rune.IsControl(rune))
            return;

        RemoveSelection();

        Span<char> encoded = stackalloc char[2];
        var count = rune.EncodeToUtf16(encoded);
        Reserve(length + count);

        buffer.AsSpan(caret, length - caret).CopyTo(buffer.AsSpan(caret + count));
        encoded[..count].CopyTo(buffer.AsSpan(caret));
        length += count;
        caret += count;
        anchor = caret;
        Wake();
    }

    private void RemoveSelection()
    {
        var (start, end) = Selection;
        if (start == end)
            return;

        buffer.AsSpan(end, length - end).CopyTo(buffer.AsSpan(start));
        length -= end - start;
        caret = start;
        anchor = start;
        Wake();
    }

    private void MoveCaret(int index, bool keepAnchor)
    {
        caret = Math.Clamp(index, 0, length);
        if (!keepAnchor)
            anchor = caret;
        Wake();
    }

    private void Wake() => blinkEpoch = Environment.TickCount64;

    private void Reserve(int capacity)
    {
        if (capacity <= buffer.Length)
            return;

        var grown = new char[Math.Max(capacity, buffer.Length * 2)];
        buffer.AsSpan(0, length).CopyTo(grown);
        buffer = grown;
    }
}
