namespace AlvorKit;

/// <summary>Verifies mounted popup isolation, first-frame preparation, and selection/dismissal behavior.</summary>
[TestClass]
public class BlendDropdownTest
{
    /// <summary>Opening builds option rows on the first draw without consuming the opening Enter key as a selection.</summary>
    [TestMethod]
    public void Open_PreparesFirstDrawAndSkipsOpeningKey()
    {
        using var h = new BlendTestHarness();
        var picks = 0;
        var (popup, layer, options, field) = Mount(h, index => picks++);
        h.Draw();
        h.Host.RaiseKeyDown(Keys.Enter);
        h.Root.Get<RootUiFocus>().Focus(field, true);
        field.OnUpdateFV.Resolve()!();
        Assert.IsTrue(popup.IsOpenFor(field));

        h.Draw();

        Assert.AreEqual(2, UiSyntax.NodesCount(options));
        Assert.AreEqual("First", UiSyntax.Nodes(UiSyntax.Nodes(options)[0])[0].TextFV.Resolve().ToString());
        Assert.AreEqual(0, picks);
        Assert.IsFalse(layer.IsDisabledFV.Resolve());
        h.Tick();
        Assert.IsTrue(popup.IsOpen);
        h.Host.RaiseKeyUp(Keys.Enter);
        h.Tick();
        h.Press(Keys.Enter);
        Assert.AreEqual(1, picks);
        Assert.IsFalse(popup.IsOpen);
    }

    /// <summary>Keyboard navigation wraps in either direction and Enter picks the highlighted option after closing.</summary>
    [TestMethod]
    public void Keyboard_WrapsAndCommitsHighlightedOption()
    {
        using var h = new BlendTestHarness();
        var selected = -1;
        var (popup, layer, options, field) = Mount(h, index => selected = index);
        h.Draw();
        field.OnPressFV.Resolve()!();
        h.Tick();
        h.Press(Keys.Up);
        h.Press(Keys.Enter);
        Assert.AreEqual(1, selected);
        Assert.IsFalse(popup.IsOpen);

        popup.Open(field, [new("First"), new("Second")], 1, index =>
        {
            Assert.IsFalse(popup.IsOpen);
            selected = index;
        });
        h.Tick();
        h.Press(Keys.Down);
        h.Press(Keys.Enter);
        Assert.AreEqual(0, selected);
    }

    /// <summary>Escape and pressing the popup backdrop dismiss without invoking the selection callback.</summary>
    [TestMethod]
    public void EscapeAndClickAway_DismissWithoutPicking()
    {
        using var h = new BlendTestHarness();
        var picks = 0;
        var (popup, layer, options, field) = Mount(h, index => picks++);
        h.Draw();
        field.OnPressFV.Resolve()!();
        h.Tick();
        h.Press(Keys.Escape);
        Assert.IsFalse(popup.IsOpen);
        Assert.AreEqual(0, picks);

        field.OnPressFV.Resolve()!();
        h.Draw();
        h.Host.RaiseMouseDown(MouseButton.Left);
        h.Tick();
        h.Host.RaiseMouseUp(MouseButton.Left);
        h.Tick();
        Assert.IsFalse(popup.IsOpen);
        Assert.AreEqual(0, picks);
    }

    /// <summary>Fields sharing one handle replace its options, anchor, and callback without retaining the previous field.</summary>
    [TestMethod]
    public void SharedHandle_SwitchesFieldsAndCallbacks()
    {
        using var h = new BlendTestHarness();
        var firstPicks = 0;
        var secondPicked = -1;
        var (popup, layer, options, first) = Mount(h, index => firstPicks++);
        var second = h.Blend.Fields.DropdownField(h.Ui, popup, "Other", [new("Other")], () => 0,
            index => secondPicked = index);
        h.Draw();
        first.OnPressFV.Resolve()!();
        h.Draw();
        Assert.AreEqual(2, UiSyntax.NodesCount(options));
        second.OnPressFV.Resolve()!();
        h.Draw();

        Assert.IsFalse(popup.IsOpenFor(first));
        Assert.IsTrue(popup.IsOpenFor(second));
        Assert.AreEqual(1, UiSyntax.NodesCount(options));
        UiSyntax.Nodes(options)[0].OnPressFV.Resolve()!();
        Assert.AreEqual(0, firstPicks);
        Assert.AreEqual(0, secondPicked);
        Assert.IsFalse(popup.IsOpen);
    }

    /// <summary>Creating two popups from one builder keeps their open state, anchors, options, and callbacks independent.</summary>
    [TestMethod]
    public void SeparateMounts_KeepIndependentState()
    {
        using var h = new BlendTestHarness();
        var firstPicked = -1;
        var secondPicked = -1;
        var (first, firstLayer, firstOptions, firstField) = Mount(h, index => firstPicked = index);
        var (second, secondLayer, secondOptions, secondField) = Mount(h, index => secondPicked = index);
        h.Draw();
        firstField.OnPressFV.Resolve()!();
        secondField.OnPressFV.Resolve()!();
        h.Draw();

        Assert.AreNotSame(first, second);
        Assert.IsTrue(first.IsOpenFor(firstField));
        Assert.IsTrue(second.IsOpenFor(secondField));
        UiSyntax.Nodes(firstOptions)[1].OnPressFV.Resolve()!();
        Assert.AreEqual(1, firstPicked);
        Assert.AreEqual(-1, secondPicked);
        Assert.IsTrue(second.IsOpen);
        UiSyntax.Nodes(secondOptions)[0].OnPressFV.Resolve()!();
        Assert.AreEqual(0, secondPicked);
    }

    /// <summary>Replacing options retains the popup's decorative border and prepares the new row geometry.</summary>
    [TestMethod]
    public void Refresh_PreservesBorderAndPreparesReplacementOptions()
    {
        using var h = new BlendTestHarness();
        var (popup, layer, options, field) = Mount(h, index => { });
        h.Draw();
        field.OnPressFV.Resolve()!();
        h.Draw();
        var panel = UiSyntax.Nodes(layer)[0];
        var border = UiSyntax.Nodes(panel)[1..].ToArray();
        Assert.AreEqual(4, border.Length);

        popup.Open(field, [new("Replacement")], 0, index => { });
        h.Draw();

        CollectionAssert.AreEqual(border, UiSyntax.Nodes(panel)[1..].ToArray());
        Assert.AreEqual(1, UiSyntax.NodesCount(options));
        Assert.AreEqual(h.Blend.S.Metrics.DropdownOptionHeight, UiSyntax.Nodes(options)[0].SizeR.Y);
        Assert.AreEqual("Replacement", UiSyntax.Nodes(UiSyntax.Nodes(options)[0])[0].TextFV.Resolve().ToString());
    }

    /// <summary>A popup under an offset mount still appears beneath its anchor in the root's coordinate space.</summary>
    [TestMethod]
    public void OffsetMount_PositionsPopupUnderAnchor()
    {
        using var h = new BlendTestHarness();
        UiSyntax.Node(h.Ui, out var mount)
            .SizeRelativeV((0, 0))
            .SizeV((400, 300))
            .OffsetV((80, 60));
        UiSyntax.Node(mount, out var content)
            .SizeRelativeV((1, 1));
        UiSyntax.Node(mount, out var popupMount)
            .SizeRelativeV((1, 1));
        var popup = h.Blend.Dropdown.Create(popupMount);
        var field = h.Blend.Fields.DropdownField(content, popup, "Choice", [new("First")], () => 0, index => { });
        field.Mutate()
            .SizeRelativeV((0, 0))
            .SizeV((120, 22))
            .OffsetV((30, 40));
        h.Draw();
        field.OnPressFV.Resolve()!();
        h.Draw();
        var panel = UiSyntax.Nodes(UiSyntax.Nodes(popupMount)[0])[0];

        Assert.AreEqual(field.PositionR.X, panel.PositionR.X);
        Assert.AreEqual(field.PositionR.Y + field.SizeR.Y + h.Blend.S.Metrics.DropdownPopupGap, panel.PositionR.Y);
    }

    private static (BlendDropdownHandle Popup, EntMut Layer, EntMut Options, EntMut Field) Mount(
        BlendTestHarness h, Action<int> pick)
    {
        UiSyntax.Node(h.Ui, out var content)
            .SizeRelativeV((1, 1));
        UiSyntax.Node(h.Ui, out var popupMount)
            .SizeRelativeV((1, 1));
        var popup = h.Blend.Dropdown.Create(popupMount);
        var field = h.Blend.Fields.DropdownField(content, popup, "Choice", [new("First"), new("Second")], () => 0, pick);
        field.Mutate()
            .SizeRelativeV((0, 0))
            .SizeV((120, 22))
            .OffsetV((20, 20));
        var layer = UiSyntax.Nodes(popupMount)[0];
        return (popup, layer, UiSyntax.Nodes(UiSyntax.Nodes(layer)[0])[0], field);
    }
}
