namespace AlvorKit;

/// <summary>Covers mouse hover, press, click, capture, and scroll dispatch through <see cref="RootUiScript"/> frames.</summary>
[TestClass]
public class RootUiMouseDispatchTest
{
    /// <summary>Hovering a selectable node marks it hovered and clears when the cursor leaves.</summary>
    [TestMethod]
    public void Hover_OverSelectableNode_MarksHovered()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true);

        h.MoveMouse((50, 25));
        h.Frame();
        Assert.IsTrue(button.IsHoveredR);

        h.MoveMouse((300, 300));
        h.Frame();
        Assert.IsFalse(button.IsHoveredR);
    }

    /// <summary>Pressing the main button over a selectable node runs its press callback and marks it pressed.</summary>
    [TestMethod]
    public void Press_OverSelectableNode_RunsPressCallback()
    {
        using var h = new UiTestHarness();
        var presses = 0;
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .OnPressF(() => presses++);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        Assert.AreEqual(1, presses);
        Assert.IsTrue(button.IsPressedR);
    }

    /// <summary>Releasing the main button over the pressed node runs its click callback.</summary>
    [TestMethod]
    public void Click_PressAndReleaseOverNode_RunsClickCallback()
    {
        using var h = new UiTestHarness();
        var clicks = 0;
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .OnClickF(() => clicks++);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Frame();

        Assert.AreEqual(1, clicks);
        Assert.IsFalse(button.IsPressedR);
    }

    /// <summary>Pressing a focusable node focuses it.</summary>
    [TestMethod]
    public void Press_OverFocusableNode_FocusesNode()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsFocusableV(true);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        Assert.IsTrue(button.IsFocusedR);
    }

    /// <summary>Releasing away from the pressed node does not run the click callback.</summary>
    [TestMethod]
    public void Click_ReleaseOffNode_DoesNotRunClickCallback()
    {
        using var h = new UiTestHarness();
        var clicks = 0;
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .OnClickF(() => clicks++);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        h.MoveMouse((300, 300));
        h.Frame();

        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Frame();

        Assert.AreEqual(0, clicks);
    }

    /// <summary>The pressed node keeps its pressed state while the button is held, even after the cursor leaves it.</summary>
    [TestMethod]
    public void Press_CursorLeavesWhileHeld_KeepsPressedState()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        h.MoveMouse((300, 300));
        h.Frame();

        Assert.IsTrue(button.IsPressedR);

        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Frame();

        Assert.IsFalse(button.IsPressedR);
    }

    /// <summary>Pressing a silent-focusable node blurs the focused node without joining Tab order; Blend's root uses this for click-away blur.</summary>
    [TestMethod]
    public void Press_OnSilentFocusableNode_BlursFocusedNode()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsFocusableV(true);
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .OffsetV((0, 100))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsSilentFocusableV(true);

        h.MoveMouse((50, 25));
        h.Frame();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Frame();
        Assert.IsTrue(button.IsFocusedR);

        h.MoveMouse((50, 125));
        h.Frame();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        Assert.IsFalse(button.IsFocusedR);
    }

    /// <summary>Pressing a selectable but non-focusable node leaves the focused node alone; blur policies are opt-in per style, not engine behavior.</summary>
    [TestMethod]
    public void Press_OnNonFocusableNode_KeepsFocus()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsFocusableV(true);
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .OffsetV((0, 100))
            .SizeV((100, 50))
            .IsSelectableV(true);

        h.MoveMouse((50, 25));
        h.Frame();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Frame();
        Assert.IsTrue(button.IsFocusedR);

        h.MoveMouse((50, 125));
        h.Frame();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Frame();

        Assert.IsTrue(button.IsFocusedR);
    }

    /// <summary>A press and release spanning only update phases, with no render in between, still presses and clicks.</summary>
    [TestMethod]
    public void Click_AgentGestureWithoutRenders_RunsPressAndClickCallbacks()
    {
        using var h = new UiTestHarness();
        var presses = 0;
        var clicks = 0;
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .OnPressF(() => presses++)
            .OnClickF(() => clicks++);

        h.MoveMouse((50, 25));
        h.Update();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Update();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Update();

        Assert.AreEqual(1, presses);
        Assert.AreEqual(1, clicks);
    }

    /// <summary>Removing the UI script releases the hovered control's hand cursor instead of leaving it stuck.</summary>
    [TestMethod]
    public void Unload_WhileHoveringHandCursorNode_ResetsCursorShape()
    {
        using var h = new UiTestHarness();
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .CursorF(() => CursorShape.Hand);

        h.MoveMouse((50, 25));
        h.Frame();
        h.Script.Draw();
        Assert.AreEqual(CursorShape.Hand, h.Host.CursorShape);

        h.Script.Unload();
        Assert.AreEqual(CursorShape.Default, h.Host.CursorShape);
    }

    /// <summary>Wheel input over a scrollable node dispatches the scroll callback with the wheel offset.</summary>
    [TestMethod]
    public void Scroll_OverScrollableNode_RunsScrollCallback()
    {
        using var h = new UiTestHarness();
        Vec2 scrolled = default;
        Node(h.Ui, out _)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsScrollableV(true)
            .OnScrollF(offset => scrolled += offset);

        h.MoveMouse((50, 25));
        h.Frame();

        h.Host.RaiseMouseWheel((0, 1));
        h.Frame();

        Assert.AreEqual(new Vec2(0, 1), scrolled);
    }

    /// <summary>A focus callback can remove its node before the same pointer event attempts to press it.</summary>
    [TestMethod]
    public void Press_FocusRemovesTarget_SkipsPressCallback()
    {
        using var h = new UiTestHarness();
        var focuses = 0;
        var presses = 0;
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsFocusableV(true)
            .OnFocusF(() =>
            {
                focuses++;
                NodesRemove(h.Ui, button);
            })
            .OnPressF(() => presses++);
        h.MoveMouse((50, 25));
        h.Update();

        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Update();

        Assert.AreEqual(1, focuses);
        Assert.AreEqual(0, presses);
        Assert.IsFalse(button.IsPressedR);
        Assert.AreEqual(0, NodesCount(h.Ui));
    }

    /// <summary>Scroll handling that detaches the hovered node prevents later press and click dispatch in that tick.</summary>
    [TestMethod]
    public void Scroll_RemovesHoveredTarget_SkipsPressAndClick()
    {
        using var h = new UiTestHarness();
        var scrolls = 0;
        var presses = 0;
        var clicks = 0;
        Node(h.Ui, out var button)
            .SizeRelativeV((0, 0))
            .SizeV((100, 50))
            .IsSelectableV(true)
            .IsScrollableV(true)
            .OnScrollF(offset =>
            {
                scrolls++;
                NodesRemove(h.Ui, button);
            })
            .OnPressF(() => presses++)
            .OnClickF(() => clicks++);
        h.MoveMouse((50, 25));
        h.Update();

        h.Host.RaiseMouseWheel((0, 1));
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Update();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Update();

        Assert.AreEqual(1, scrolls);
        Assert.AreEqual(0, presses);
        Assert.AreEqual(0, clicks);
        Assert.IsFalse(button.IsPressedR);
    }
}
