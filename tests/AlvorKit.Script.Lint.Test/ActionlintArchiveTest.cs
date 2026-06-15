using System.Runtime.InteropServices;

namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests platform-specific actionlint release asset selection.</summary>
[TestClass]
public sealed class ActionlintArchiveTest
{
    /// <summary>Builds the expected Linux x64 actionlint release URL.</summary>
    [TestMethod]
    public void ForLinuxX64BuildsReleaseUrl()
    {
        var archive = ActionlintArchive.For(windows: false, linux: true, osx: false, Architecture.X64);

        Assert.AreEqual("linux", archive.Os);
        Assert.AreEqual("amd64", archive.Arch);
        Assert.AreEqual("tar.gz", archive.Extension);
        Assert.AreEqual("actionlint", archive.ExecutableName);
        Assert.AreEqual(
            "https://github.com/rhysd/actionlint/releases/download/v1.7.12/actionlint_1.7.12_linux_amd64.tar.gz",
            archive.Url("1.7.12"));
    }

    /// <summary>Builds the expected Windows x64 actionlint release archive metadata.</summary>
    [TestMethod]
    public void ForWindowsX64BuildsZipArchive()
    {
        var archive = ActionlintArchive.For(windows: true, linux: false, osx: false, Architecture.X64);

        Assert.AreEqual("windows", archive.Os);
        Assert.AreEqual("zip", archive.Extension);
        Assert.AreEqual("actionlint.exe", archive.ExecutableName);
        Assert.IsTrue(archive.IsZip);
    }

    /// <summary>Builds the expected macOS arm64 actionlint release archive metadata.</summary>
    [TestMethod]
    public void ForMacArm64BuildsTarArchive()
    {
        var archive = ActionlintArchive.For(windows: false, linux: false, osx: true, Architecture.Arm64);

        Assert.AreEqual("darwin", archive.Os);
        Assert.AreEqual("arm64", archive.Arch);
        Assert.AreEqual("tar.gz", archive.Extension);
        Assert.IsFalse(archive.IsZip);
    }

    /// <summary>Rejects unsupported architectures explicitly.</summary>
    [TestMethod]
    public void ForRejectsUnsupportedArchitecture()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => ActionlintArchive.For(windows: true, linux: false, osx: false, Architecture.X86));
    }

    /// <summary>Rejects unsupported operating systems explicitly.</summary>
    [TestMethod]
    public void ForRejectsUnsupportedOperatingSystem()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => ActionlintArchive.For(windows: false, linux: false, osx: false, Architecture.X64));
    }
}
