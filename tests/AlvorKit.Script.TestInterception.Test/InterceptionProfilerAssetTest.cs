namespace AlvorKit;

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
                isMacOS: false,
                Architecture.X64));
        Assert.AreEqual(
            "linux-x64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                isMacOS: false,
                Architecture.X64));
        Assert.AreEqual(
            "linux-arm64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                isMacOS: false,
                Architecture.Arm64));
        Assert.AreEqual(
            "osx-arm64",
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: false,
                isMacOS: true,
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
                isMacOS: false,
                Architecture.Arm64));
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: true,
                isMacOS: false,
                Architecture.X86));
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: false,
                isMacOS: false,
                Architecture.X64));
        Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
            InterceptionProfilerAsset.RuntimeIdentifierFor(
                isWindows: false,
                isLinux: false,
                isMacOS: true,
                Architecture.X64));
    }
}
