namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for native build manifest platform selection.</summary>
[TestClass]
public sealed class NativeBuildConfigTest
{
    /// <summary>Platform returns the matching platform configuration object.</summary>
    [TestMethod]
    public void Platform_ReturnsRequestedConfig()
    {
        var linux = new PlatformBuildConfig { Packages = ["linux"] };
        var config = new NativeBuildConfig { Kind = "single-c", Linux = linux };

        Assert.AreSame(linux, config.Platform(TargetOperatingSystem.Linux));
    }
}
