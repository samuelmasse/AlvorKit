namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for runtime identifier parsing and toolchain mapping.</summary>
[TestClass]
public sealed class TargetRidTest
{
    /// <summary>Known RIDs parse into their operating system and architecture pieces.</summary>
    [TestMethod]
    public void Parse_KnownRid_ReturnsParts()
    {
        var target = TargetRid.Parse("linux-arm");

        Assert.AreEqual(TargetOperatingSystem.Linux, target.OperatingSystem);
        Assert.AreEqual(TargetArchitecture.Arm, target.Architecture);
        Assert.AreEqual("arm-linux-gnueabihf-gcc", target.LinuxCompiler);
    }

    /// <summary>Unsupported RIDs are rejected early.</summary>
    [TestMethod]
    public void Parse_UnknownRid_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TargetRid.Parse("linux-riscv64"));
    }

    /// <summary>Native library file names follow .NET runtime probing conventions.</summary>
    [TestMethod]
    public void LibraryFileName_UsesPlatformConventions()
    {
        Assert.AreEqual("glfw3.dll", TargetRid.Parse("win-x64").LibraryFileName("glfw3"));
        Assert.AreEqual("libglfw3.so", TargetRid.Parse("linux-x64").LibraryFileName("glfw3"));
        Assert.AreEqual("libglfw3.dylib", TargetRid.Parse("osx-arm64").LibraryFileName("glfw3"));
    }

}
