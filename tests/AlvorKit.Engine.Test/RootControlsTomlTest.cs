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

    /// <summary>Dash-suffixed TOML sections bind the base control name.</summary>
    [TestMethod]
    public void Load_BindsDashSuffixedSectionsToBaseControl()
    {
        var host = new FakeWindowHost();
        var controls = new RootControls(new(host));
        var loader = new RootControlsToml(controls);

        loader.Load("""
            [Jump-Keyboard]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);
        host.RaiseKeyDown(Keys.Space);

        Assert.IsTrue(controls["Jump"].Run());
    }

    /// <summary>Control loading can read a named TOML file from a nearby root res directory.</summary>
    [TestMethod]
    public void AddFromFile_BindsRootResFileByName()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "res"));
        File.WriteAllText(
            Path.Combine(root, "res", "Controls.toml"),
            """
            [Jump]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);

        try
        {
            using var directory = new CurrentDirectoryScope(root);
            var host = new FakeWindowHost();
            var controls = new RootControls(new(host));

            new RootControlsToml(controls).AddFromFile("Controls.toml");
            host.RaiseKeyDown(Keys.Space);

            Assert.IsTrue(controls["Jump"].Run());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
