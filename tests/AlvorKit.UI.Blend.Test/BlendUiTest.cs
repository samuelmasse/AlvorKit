namespace AlvorKit;

/// <summary>Verifies composition, appearance sharing, and graphics lifetime for an application-owned Blend toolkit.</summary>
[TestClass]
public class BlendUiTest
{
    /// <summary>All toolkit recipes consume the chosen appearance, while two toolkits keep separate appearance choices.</summary>
    [TestMethod]
    public void Composition_UsesChosenAppearanceAndSharedRootFonts()
    {
        var palette = BlendPalette.Default with { AppBackground = (0.1f, 0.2f, 0.3f, 1), Raised = (0.4f, 0.2f, 0.1f, 1) };
        var metrics = new BlendMetrics { FieldHeight = 35, DropdownOptionHeight = 31 };
        using var h = new BlendTestHarness(palette, metrics);
        var other = new BlendUi(h.Root.Get<RootBlend>(), h.AppGl);

        Assert.AreEqual(palette, h.Blend.S.Palette);
        Assert.AreSame(metrics, h.Blend.S.Metrics);
        Assert.AreEqual(BlendPalette.Default, other.S.Palette);
        Assert.AreSame(h.Blend.S.TextFont, other.S.TextFont);
        Assert.AreNotSame(h.Blend.S, other.S);

        var popup = h.Blend.Dropdown.Create(h.Ui);
        var field = h.Blend.Fields.DropdownField(h.Ui, popup, "Choice", [new("First")], () => 0, index => { });
        Assert.AreEqual(metrics.FieldHeight, field.SizeFV.Resolve().Y);
        Assert.AreEqual(palette.AppBackground, field.ColorFV.Resolve());
    }

    /// <summary>Rounded control caps share one toolkit cache and die with its graphics node while root fonts survive.</summary>
    [TestMethod]
    public void ControlTextures_BelongToChosenGraphicsNode()
    {
        using var h = new BlendTestHarness();
        UiSyntax.Node(h.Ui, out var first)
            .Mutate(h.Blend.S.Button)
            .TextV("First");
        UiSyntax.Node(h.Ui, out var second)
            .Mutate(h.Blend.S.Button)
            .TextV("Second");

        var firstCap = UiSyntax.Nodes(first)[0].TextureFV.Resolve()!;
        var secondCap = UiSyntax.Nodes(second)[0].TextureFV.Resolve()!;
        Assert.AreSame(firstCap, secondCap);
        Assert.IsTrue(h.Backend.HasTexture(firstCap.Id));
        h.Blend.S.TextFont.Size(14).GlyphSlot(new Rune('A'));
        var fontTexture = h.Blend.S.TextFont.Textures[0];
        Assert.IsTrue(h.Backend.HasTexture(fontTexture.Id));

        h.AppGl.Dispose();

        Assert.IsFalse(h.Backend.HasTexture(firstCap.Id));
        Assert.IsTrue(h.Backend.HasTexture(fontTexture.Id));
    }

    /// <summary>One injected scroll builder creates independent offsets and preserves the explicit wheel-step overload.</summary>
    [TestMethod]
    public void ScrollViews_KeepIndependentOffsets()
    {
        using var h = new BlendTestHarness();
        var first = h.Blend.ScrollView.Create(h.Ui, out var firstViewport, out var firstContent);
        var second = h.Blend.ScrollView.Create(h.Ui, out var secondViewport, out var secondContent, 20);
        firstViewport.Mutate()
            .SizeRelativeV((0, 0))
            .SizeV((100, 100));
        secondViewport.Mutate()
            .SizeRelativeV((0, 0))
            .SizeV((100, 100));
        firstContent.Mutate()
            .SizeInnerSumRelativeV((0, 0))
            .SizeV((100, 500));
        secondContent.Mutate()
            .SizeInnerSumRelativeV((0, 0))
            .SizeV((100, 500));
        h.Draw();
        firstViewport.OnScrollFV.Resolve()!((0, -1));
        secondViewport.OnScrollFV.Resolve()!((0, -1));

        Assert.AreEqual(48f, first.Offset);
        Assert.AreEqual(20f, second.Offset);
        first.Reset();
        Assert.AreEqual(20f, second.Offset);
    }
}
