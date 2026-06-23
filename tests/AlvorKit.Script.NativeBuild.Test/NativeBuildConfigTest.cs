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

    /// <summary>FastNoise2 Linux ARM manifests avoid upstream FastSIMD ARM SIMD paths that do not compile on CI GCC.</summary>
    [TestMethod]
    public void FastNoise2Manifest_LinuxArmTargetsUseScalarFastSimd()
    {
        var context = LibraryBuildContext.Load(RepositoryLayout.FindFrom(AppContext.BaseDirectory), "fastnoise2");

        foreach (var rid in new[] { "linux-arm64", "linux-arm" })
        {
            var options = context.Build.Linux.CMakeOptionsFor(TargetRid.Parse(rid)).ToArray();

            CollectionAssert.Contains(options, "-DCMAKE_CXX_FLAGS=-DFASTSIMD_MAX_FEATURE_SET=SCALAR -DFASTSIMD_DEFAULT_FEATURE_SET=SCALAR -ffp-contract=off");
            CollectionAssert.Contains(options, "-DFASTNOISE2_FASTSIMD_FEATURE_SETS=FEATURE_SETS;SCALAR");
        }
    }
}
