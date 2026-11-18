namespace AlvorKit;

[TestClass]
public class UiSetupTest
{
    /// <summary>UI setup registers its script once and releases UI trees after game unload callbacks.</summary>
    [TestMethod]
    public void UiSetup_RegistersScriptAndRetainsShutdownOrdering()
    {
        using var h = new BlendTestHarness();
        new UiSetup().Apply(h.Root);
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
    public void RootBlendUi_IsSharedAcrossChildMenusAndOwnsTexturesUntilRootShutdown()
    {
        using var h = new BlendTestHarness();
        var scripts = h.Root.Get<RootScripts>();
        scripts.Remove(h.Root.Get<RootUiScript>());
        var first = h.Root.Scope<ChildScope>().Get<ChildBlendMenu>();
        var second = h.Root.Scope<ChildScope>().Get<ChildBlendMenu>();
        var blend = first.Blend;

        Assert.AreSame(h.Root.Get<RootBlendUi>(), blend);
        Assert.AreSame(blend, second.Blend);
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

    /// <summary>Extra UI passes interleave with game scripts and leave the registry when their surface disappears.</summary>
    [TestMethod]
    public void AdditionalSurfaces_InterleaveWithGameScripts()
    {
        using var h = new BlendTestHarness();
        var ui = h.Root.Get<RootUi>();
        var child = h.Root.Scope<ChildScope>();
        var scripts = h.Root.Get<RootScripts>();
        var game = new GamePass();
        scripts.Add(game);
        using var earlier = ui.Surfaces.Create(order: -2f);
        child.Add(earlier.Root);
        var menu = child.Get<ChildMenu>();
        Assert.AreSame(ui, menu.Ui);
        Assert.AreSame(earlier.Root, menu.Mount);
        Assert.AreNotSame(menu.Ui.Default.Root, menu.Mount);
        var viewport = new Box2((20, 10), (220, 110));
        var later = ui.Surfaces.Create(viewport, scale: 2f, order: 2f);
        Assert.AreEqual(-2f, scripts.Span[0].Order);
        Assert.AreSame(h.Root.Get<RootUiScript>(), scripts.Span[1]);
        Assert.AreSame(game, scripts.Span[2]);
        Assert.AreEqual(viewport, scripts.Span[3].DrawViewport);
        Assert.AreEqual(new Vec2(100, 50), scripts.Span[3].DrawArea);

        later.Dispose();
        later.Dispose();
        Assert.IsFalse(later.Root.IsAlive);
        Assert.AreEqual(3, scripts.Span.Length);
        scripts.Remove(game);
    }

    /// <summary>Reattaching the root script draws existing surfaces once and removes their passes when detached.</summary>
    [TestMethod]
    public void UiSetup_ReattachesAnExistingSurfaceAndRemovesItsDrawPass()
    {
        using var h = new BlendTestHarness();
        var scripts = h.Root.Get<RootScripts>();
        var script = h.Root.Get<RootUiScript>();
        scripts.Remove(script);
        using var surface = h.Root.Get<RootUi>().Surfaces.Create(order: 1f);
        var draws = 0;
        UiSyntax.Node(surface.Root).OnDrawF(() => draws++);

        new UiSetup().Apply(h.Root);
        h.Draw();
        Assert.AreEqual(1, draws);

        scripts.Remove(script);
        Assert.AreEqual(0, scripts.Span.Length);
        Assert.IsTrue(surface.Root.IsAlive);
        h.Draw();
        Assert.AreEqual(1, draws);
    }

    /// <summary>Every engine phase lets a script detach itself and a UI surface while later game callbacks still run.</summary>
    [TestMethod]
    [DataRow(nameof(Script.Update))]
    [DataRow(nameof(Script.Frame))]
    [DataRow(nameof(Script.Render))]
    [DataRow(nameof(Script.Draw))]
    public void ScriptRemovalDuringPhase_ContinuesPastDetachedSurface(string phase)
    {
        using var h = new BlendTestHarness();
        var scripts = h.Root.Get<RootScripts>();
        var engine = h.Root.Get<RootEngine>();
        using var surface = h.Root.Get<RootUi>().Surfaces.Create(order: 1);
        var surfaceDraws = 0;
        UiSyntax.Node(surface.Root).OnDrawF(() => surfaceDraws++);
        var visited = new List<Script>();
        var first = new PhaseScript(-1, (script, current) =>
        {
            if (current == phase)
            {
                visited.Add(script);
                scripts.Remove(script);
                surface.Dispose();
            }
        });
        var last = new PhaseScript(2, (script, current) =>
        {
            if (current == phase)
                visited.Add(script);
        });
        scripts.Add(first);
        scripts.Add(last);

        engine.Update(0.01);
        engine.Frame(0.01);
        engine.Render();

        CollectionAssert.AreEqual(new Script[] { first, last }, visited);
        Assert.AreEqual(1, first.Unloads);
        Assert.AreEqual(0, last.Unloads);
        Assert.AreEqual(0, surfaceDraws);
    }

    private class PhaseScript(float order, Action<Script, string> visit) : Script
    {
        public override float Order => order;
        internal int Unloads { get; private set; }

        public override void Update(double delta) => visit(this, nameof(Update));
        public override void Frame(double delta) => visit(this, nameof(Frame));
        public override void Render() => visit(this, nameof(Render));
        public override void Draw() => visit(this, nameof(Draw));
        public override void Unload() => Unloads++;
    }

    private class GamePass : Script
    {
        public override float Order => 1f;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    private class ChildAttribute : InjectorAttribute;

    [Child]
    private class ChildBlendMenu(RootBlendUi blend)
    {
        internal RootBlendUi Blend => blend;
    }

    [Child]
    private class ChildMenu(RootUi ui, UiRoot mount)
    {
        internal RootUi Ui => ui;
        internal UiRoot Mount => mount;
    }

    [Child]
    private class ChildScope : InjectorScope<ChildAttribute>;
}
