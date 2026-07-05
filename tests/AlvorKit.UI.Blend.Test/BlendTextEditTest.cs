namespace AlvorKit.UI.Blend.Test;

/// <summary>Covers the single-line text edit engine: buffer, caret, selection, and key handling.</summary>
[TestClass]
public class BlendTextEditTest
{
    private readonly FakeWindowHost host;
    private readonly BlendTextEdit edit;

    public BlendTextEditTest()
    {
        host = new FakeWindowHost();
        var window = new WindowLoop(host);
        edit = new BlendTextEdit(new Keyboard(window));
    }

    /// <summary>Beginning with select-all selects the initial text and puts the caret at the end.</summary>
    [TestMethod]
    public void Begin_WithSelectAll_SelectsInitialText()
    {
        edit.Begin("abc", true);

        Assert.IsTrue(edit.IsActive);
        Assert.AreEqual("abc", edit.Span.ToString());
        Assert.AreEqual(3, edit.Caret);
        Assert.AreEqual((0, 3), edit.Selection);
    }

    /// <summary>Typing inserts runes at the caret.</summary>
    [TestMethod]
    public void Update_TypedRunes_InsertAtCaret()
    {
        edit.Begin("ab", false);

        Type("cd");

        Assert.AreEqual("abcd", edit.Span.ToString());
        Assert.AreEqual(4, edit.Caret);
    }

    /// <summary>Typing over a selection replaces the selected text.</summary>
    [TestMethod]
    public void Update_TypeOverSelection_ReplacesSelection()
    {
        edit.Begin("value", true);

        Type("x");

        Assert.AreEqual("x", edit.Span.ToString());
    }

    /// <summary>Backspace deletes the character before the caret.</summary>
    [TestMethod]
    public void Update_Backspace_DeletesBeforeCaret()
    {
        edit.Begin("abc", false);

        Press(Keys.Backspace);

        Assert.AreEqual("ab", edit.Span.ToString());
        Assert.AreEqual(2, edit.Caret);
    }

    /// <summary>Delete removes the selected range instead of a single character.</summary>
    [TestMethod]
    public void Update_DeleteWithSelection_RemovesSelection()
    {
        edit.Begin("abcd", true);

        Press(Keys.Delete);

        Assert.AreEqual(0, edit.Span.Length);
        Assert.AreEqual(0, edit.Caret);
    }

    /// <summary>Arrow keys move the caret and shift extends the selection.</summary>
    [TestMethod]
    public void Update_ShiftLeft_ExtendsSelection()
    {
        edit.Begin("abc", false);

        host.RaiseKeyDown(Keys.LeftShift);
        Press(Keys.Left);
        host.RaiseKeyUp(Keys.LeftShift);

        Assert.AreEqual((2, 3), edit.Selection);
        Assert.AreEqual(2, edit.Caret);
    }

    /// <summary>A plain arrow with a selection collapses the selection instead of moving past it.</summary>
    [TestMethod]
    public void Update_LeftWithSelection_CollapsesToSelectionStart()
    {
        edit.Begin("abc", true);

        Press(Keys.Left);

        Assert.IsFalse(edit.HasSelection);
        Assert.AreEqual(0, edit.Caret);
    }

    /// <summary>Home and End jump to the text boundaries.</summary>
    [TestMethod]
    public void Update_HomeThenEnd_JumpsToBoundaries()
    {
        edit.Begin("abc", false);

        Press(Keys.Home);
        Assert.AreEqual(0, edit.Caret);

        Press(Keys.End);
        Assert.AreEqual(3, edit.Caret);
    }

    /// <summary>Ctrl+A selects the whole text.</summary>
    [TestMethod]
    public void Update_CtrlA_SelectsAll()
    {
        edit.Begin("abc", false);
        Press(Keys.Home);

        host.RaiseKeyDown(Keys.LeftControl);
        Press(Keys.A);
        host.RaiseKeyUp(Keys.LeftControl);

        Assert.AreEqual((0, 3), edit.Selection);
    }

    /// <summary>Enter commits, Escape cancels, and Tab commits.</summary>
    [TestMethod]
    public void Update_EnterEscapeTab_SignalCommitAndCancel()
    {
        edit.Begin("abc", false);

        Assert.AreEqual(BlendTextEditResult.Commit, Press(Keys.Enter));
        Assert.AreEqual(BlendTextEditResult.Cancel, Press(Keys.Escape));
        Assert.AreEqual(BlendTextEditResult.Commit, Press(Keys.Tab));
    }

    /// <summary>Control runes from the text stream are ignored.</summary>
    [TestMethod]
    public void Update_ControlRune_IsIgnored()
    {
        edit.Begin("ab", false);

        host.RaiseText(new Rune('\t'));
        edit.Update();
        host.RaiseUpdate();

        Assert.AreEqual("ab", edit.Span.ToString());
    }

    /// <summary>Text longer than the initial buffer grows the buffer and keeps content intact.</summary>
    [TestMethod]
    public void Update_LongText_GrowsBuffer()
    {
        edit.Begin("", false);

        var expected = new string('x', 100);
        Type(expected);

        Assert.AreEqual(expected, edit.Span.ToString());
    }

    private BlendTextEditResult Press(Keys key)
    {
        host.RaiseKeyDown(key);
        var result = edit.Update();
        host.RaiseKeyUp(key);
        host.RaiseUpdate();
        return result;
    }

    private void Type(string text)
    {
        foreach (var rune in text.EnumerateRunes())
            host.RaiseText(rune);
        edit.Update();
        host.RaiseUpdate();
    }
}
