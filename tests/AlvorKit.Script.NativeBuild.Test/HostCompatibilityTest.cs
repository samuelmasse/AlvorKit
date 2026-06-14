using System.Runtime.InteropServices;
using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for host and target compatibility checks.</summary>
[TestClass]
public sealed class HostCompatibilityTest
{
    /// <summary>Matching Linux x64 host and target are accepted.</summary>
    [TestMethod]
    public void EnsureCanBuild_MatchingLinuxHost_Allows()
    {
        HostCompatibility.EnsureCanBuild(TargetRid.Parse("linux-x64"), new(false, true, false, Architecture.X64));
    }

    /// <summary>Wrong operating systems are rejected before a build starts.</summary>
    [TestMethod]
    public void EnsureCanBuild_WindowsTargetOnLinux_Throws()
    {
        Assert.ThrowsExactly<PlatformNotSupportedException>(
            () => HostCompatibility.EnsureCanBuild(TargetRid.Parse("win-x64"), new(false, true, false, Architecture.X64)));
    }

    /// <summary>Linux arm cross builds are allowed on Linux hosts.</summary>
    [TestMethod]
    public void EnsureCanBuild_LinuxArmOnArm64Linux_Allows()
    {
        HostCompatibility.EnsureCanBuild(TargetRid.Parse("linux-arm"), new(false, true, false, Architecture.Arm64));
    }
}
