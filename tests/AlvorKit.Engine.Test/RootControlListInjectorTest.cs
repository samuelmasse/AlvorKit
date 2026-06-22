namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootControlListInjectorTest
{
    /// <summary>Control-list injection maps constructor parameter names onto root controls.</summary>
    [TestMethod]
    public void Get_InjectsControlsByParameterName()
    {
        var root = new Injector().Scope<RootScope>();
        var controls = CreateControls();
        root.Add(controls);
        root.Handler(new RootControlListInjector(controls));

        var list = root.Get<PlayerControls>();

        Assert.AreSame(controls["MoveLeft"], list.MoveLeft);
        Assert.AreSame(controls["Jump"], list.Jump);
    }

    /// <summary>Root-loop style handler registration injects unmarked game control-list records used by root services.</summary>
    [TestMethod]
    public void New_WithHandlerOnInjector_InjectsUnmarkedControlListParameter()
    {
        var injector = new Injector();
        var root = injector.Scope<RootScope>();
        var controls = CreateControls();
        root.Add(controls);
        injector.Handler(new RootControlListInjector(controls));

        var owner = root.New<ControlOwner>();

        Assert.AreSame(controls["MoveLeft"], owner.Controls.MoveLeft);
        Assert.AreSame(controls["Jump"], owner.Controls.Jump);
    }

    /// <summary>Control-list injection rejects non-control constructor parameters.</summary>
    [TestMethod]
    public void Get_WithInvalidParameter_Throws()
    {
        var root = new Injector().Scope<RootScope>();
        var controls = CreateControls();
        root.Add(controls);
        root.Handler(new RootControlListInjector(controls));

        Assert.ThrowsException<InjectorException>(() => root.Get<BadControls>());
    }

    private static RootControls CreateControls() => new(new(new FakeWindowHost()));

    [Root]
    private sealed record PlayerControls(Control MoveLeft, Control Jump) : ControlList;

    private sealed record GameControls(Control MoveLeft, Control Jump) : ControlList;

    [Root]
    private sealed class ControlOwner(GameControls controls)
    {
        public GameControls Controls => controls;
    }

    [Root]
    private sealed record BadControls(string Name) : ControlList;
}
