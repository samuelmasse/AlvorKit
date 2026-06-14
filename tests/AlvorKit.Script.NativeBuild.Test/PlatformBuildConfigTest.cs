using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for platform manifest package selection.</summary>
[TestClass]
public sealed class PlatformBuildConfigTest
{
    /// <summary>Native Linux builds combine base packages with native packages.</summary>
    [TestMethod]
    public void LinuxPackages_NativeTarget_UsesNativePackages()
    {
        var config = new PlatformBuildConfig { Packages = ["base"], NativePackages = ["native"], ArmPackages = ["arm"] };

        CollectionAssert.AreEqual(new[] { "base", "native" }, config.LinuxPackages(TargetRid.Parse("linux-x64")).ToArray());
    }

    /// <summary>linux-arm builds combine base packages with arm cross packages.</summary>
    [TestMethod]
    public void LinuxPackages_ArmTarget_UsesArmPackages()
    {
        var config = new PlatformBuildConfig { Packages = ["base"], NativePackages = ["native"], ArmPackages = ["arm"] };

        CollectionAssert.AreEqual(new[] { "base", "arm" }, config.LinuxPackages(TargetRid.Parse("linux-arm")).ToArray());
    }
}
