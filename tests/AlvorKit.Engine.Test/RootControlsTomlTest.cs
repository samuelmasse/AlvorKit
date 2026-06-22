namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootControlsTomlTest
{
    /// <summary>TOML control loading binds named keys without changing their case.</summary>
    [TestMethod]
    public void Load_BindsNamedControls()
    {
        var host = new FakeWindowHost();
        var controls = new RootControls(new(host));
        var loader = new RootControlsToml(controls);

        loader.Load("""
            [Jump]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);
        host.RaiseKeyDown(Keys.Space);

        Assert.IsTrue(controls["Jump"].Run());
    }
}
