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
        Assert.AreEqual("arm-linux-gnueabihf-g++", target.LinuxCxxCompiler);
        Assert.AreEqual("arm-linux-gnueabihf-readelf", target.LinuxReadElf);
        Assert.AreEqual("arm-linux-gnueabihf-strip", target.LinuxStrip);
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

    /// <summary>Windows runtime identifiers expose MSVC and diagnostic architecture labels.</summary>
    [TestMethod]
    public void WindowsArchitectureProperties_UseMsvcNames()
    {
        Assert.AreEqual("amd64", TargetRid.Parse("win-x64").VisualStudioArchitecture);
        Assert.AreEqual("x86", TargetRid.Parse("win-x86").VisualStudioArchitecture);
        Assert.AreEqual("arm64", TargetRid.Parse("win-arm64").VisualStudioArchitecture);
        Assert.AreEqual("x64", TargetRid.Parse("win-x64").WindowsArchitecture);
        Assert.AreEqual("x86", TargetRid.Parse("win-x86").WindowsArchitecture);
        Assert.AreEqual("arm64", TargetRid.Parse("win-arm64").WindowsArchitecture);
    }

    /// <summary>macOS runtime identifiers expose clang architecture names.</summary>
    [TestMethod]
    public void MacArchitecture_UsesClangNames()
    {
        Assert.AreEqual("x86_64", TargetRid.Parse("osx-x64").MacArchitecture);
        Assert.AreEqual("arm64", TargetRid.Parse("osx-arm64").MacArchitecture);
    }

    /// <summary>Toolchain properties reject architectures unsupported by that toolchain family.</summary>
    [TestMethod]
    public void PlatformSpecificProperties_RejectUnsupportedArchitectures()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = TargetRid.Parse("linux-arm").VisualStudioArchitecture);
        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = TargetRid.Parse("linux-arm").WindowsArchitecture);
        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = TargetRid.Parse("win-x86").MacArchitecture);
    }

    /// <summary>Fallback switch arms fail loudly for impossible target values.</summary>
    [TestMethod]
    public void InvalidEnumValues_ThrowPlatformErrors()
    {
        var invalidArchitecture = new TargetRid("fixture", TargetOperatingSystem.Windows, (TargetArchitecture)999);
        var invalidOperatingSystem = new TargetRid("fixture", (TargetOperatingSystem)999, TargetArchitecture.X64);

        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = invalidArchitecture.VisualStudioArchitecture);
        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = invalidArchitecture.WindowsArchitecture);
        Assert.ThrowsException<PlatformNotSupportedException>(() => _ = invalidOperatingSystem.LibraryFileName("fixture"));
    }
}
