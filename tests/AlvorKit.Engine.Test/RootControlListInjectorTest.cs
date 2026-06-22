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

    [Root]
    private sealed record BadControls(string Name) : ControlList;
}
