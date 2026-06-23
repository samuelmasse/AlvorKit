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

        Assert.AreEqual("base,native", string.Join(",", config.LinuxPackages(TargetRid.Parse("linux-x64"))));
    }

    /// <summary>linux-arm builds combine base packages with arm cross packages.</summary>
    [TestMethod]
    public void LinuxPackages_ArmTarget_UsesArmPackages()
    {
        var config = new PlatformBuildConfig { Packages = ["base"], NativePackages = ["native"], ArmPackages = ["arm"] };

        Assert.AreEqual("base,arm", string.Join(",", config.LinuxPackages(TargetRid.Parse("linux-arm"))));
    }

    /// <summary>CMake options combine common options with matching RID-specific options.</summary>
    [TestMethod]
    public void CMakeOptionsFor_MatchingRid_AppendsRidOptions()
    {
        var config = new PlatformBuildConfig
        {
            CMakeOptions = ["common"],
            RidCMakeOptions = new() { ["linux-arm64"] = ["arm64"] }
        };

        Assert.AreEqual("common,arm64", string.Join(",", config.CMakeOptionsFor(TargetRid.Parse("linux-arm64"))));
    }
}
