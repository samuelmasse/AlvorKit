namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootControlsTomlTest
{
    /// <summary>TOML file loading binds named keys without changing their case.</summary>
    [TestMethod]
    public void AddFromFile_BindsNamedControls()
    {
        var host = new FakeWindowHost();
        var controls = new RootControls(new(host));
        var loader = new RootControlsToml(controls);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.toml");

        File.WriteAllText(path, """
            [Jump]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);

        try
        {
            loader.AddFromFile(path);
            host.RaiseKeyDown(Keys.Space);

            Assert.IsTrue(controls["Jump"].Run());
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Dash-suffixed TOML file sections bind the base control name.</summary>
    [TestMethod]
    public void AddFromFile_BindsDashSuffixedSectionsToBaseControl()
    {
        var host = new FakeWindowHost();
        var controls = new RootControls(new(host));
        var loader = new RootControlsToml(controls);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.toml");

        File.WriteAllText(path, """
            [Jump-Keyboard]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);

        try
        {
            loader.AddFromFile(path);
            host.RaiseKeyDown(Keys.Space);

            Assert.IsTrue(controls["Jump"].Run());
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Control loading reads the direct path supplied by the caller.</summary>
    [TestMethod]
    public void AddFromFile_BindsDirectPath()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "Controls.toml"),
            """
            [Jump]
            KeyPress = "Space"
            Shift = "Any"
            Control = "Any"
            Alt = "Any"
            """);

        try
        {
            var host = new FakeWindowHost();
            var controls = new RootControls(new(host));

            new RootControlsToml(controls).AddFromFile(Path.Combine(root, "Controls.toml"));
            host.RaiseKeyDown(Keys.Space);

            Assert.IsTrue(controls["Jump"].Run());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
