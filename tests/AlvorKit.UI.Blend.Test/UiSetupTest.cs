namespace AlvorKit;

[TestClass]
public class UiSetupTest
{
    /// <summary>UI setup registers its script once and releases UI trees after game unload callbacks.</summary>
    [TestMethod]
    public void UiSetup_RegistersScriptAndRetainsShutdownOrdering()
    {
        using var h = new BlendTestHarness();
        var scripts = h.Root.Get<RootScripts>();
        var ui = h.Ui;
        var aliveDuringGameUnload = false;
        h.Root.Get<RootUnload>().Add(() => aliveDuringGameUnload = ui.IsAlive);

        Assert.AreEqual(1, scripts.Span.Length);
        Assert.AreSame(h.Root.Get<RootUiScript>(), scripts.Span[0]);
        scripts.Remove(scripts.Span[0]);
        h.Root.Get<RootUnload>().Run();

        Assert.IsTrue(aliveDuringGameUnload);
        Assert.IsFalse(ui.IsAlive);
        Assert.AreEqual(0, scripts.Span.Length);
    }

    /// <summary>The default Blend toolkit is shared with child scopes and its textures outlive application graphics.</summary>
    [TestMethod]
    public void BlendUiSetup_UsesRootOwnershipWithoutAddingUiScript()
    {
        using var h = new BlendTestHarness();
        var scripts = h.Root.Get<RootScripts>();
        scripts.Remove(h.Root.Get<RootUiScript>());
        new BlendUiSetup().Apply(h.Root);
        var blend = h.Root.Get<BlendUi>();
        var child = h.Root.Scope<ChildScope>();

        Assert.AreSame(blend, child.Get<BlendUi>());
        Assert.AreEqual(BlendPalette.Default, blend.S.Palette);
        Assert.AreEqual(0, scripts.Span.Length);
        UiSyntax.Node(h.Ui, out var button)
            .Mutate(blend.S.Button)
            .TextV("Root-owned");
        var texture = UiSyntax.Nodes(button)[0].TextureFV.Resolve()!;

        h.AppGl.Dispose();

        Assert.IsTrue(h.Backend.HasTexture(texture.Id));
        h.Draw();
        h.Dispose();
        Assert.IsFalse(h.Backend.HasTexture(texture.Id));
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    private class ChildAttribute : InjectorAttribute;

    [Child]
    private class ChildScope : InjectorScope<ChildAttribute>;
}
