namespace AlvorKit.Script.TestInterception;

/// <summary>Verifies supported host mappings for packaged profiler assets.</summary>
[TestClass]
public class InterceptionProfilerAssetTest
{
    /// <summary>Maps every supported host to its exact native package RID.</summary>
    [TestMethod]
    public void RuntimeIdentifierForSupportedHostsReturnsExactRid()
    {
        Assert.AreEqual(
            "win-x64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: true,
                isLinux: false,
                Architecture.X64));
        Assert.AreEqual(
            "linux-x64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                Architecture.X64));
        Assert.AreEqual(
            "linux-arm64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                Architecture.Arm64));
    }

    /// <summary>Rejects operating-system and architecture combinations without native assets.</summary>
    [TestMethod]
    public void RuntimeIdentifierForUnsupportedHostsThrows()
    {
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: true,
                isLinux: false,
                Architecture.Arm64));
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                Architecture.X86));
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: false,
                Architecture.X64));
    }
}
