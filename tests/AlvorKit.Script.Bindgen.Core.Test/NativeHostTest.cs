using System.Runtime.InteropServices;

namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Covers host runtime identifier and native library file-name mappings.</summary>
[TestClass]
public sealed class NativeHostTest
{
    /// <summary>Supported operating system and architecture pairs map to NuGet runtime IDs.</summary>
    [TestMethod]
    public void RuntimeIdentifier_FormatsSupportedRuntimeIdentifiers()
    {
        Assert.AreEqual("win-x64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Windows, Architecture.X64));
        Assert.AreEqual("win-x86", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Windows, Architecture.X86));
        Assert.AreEqual("win-arm64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Windows, Architecture.Arm64));
        Assert.AreEqual("linux-x64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Linux, Architecture.X64));
        Assert.AreEqual("linux-arm64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Linux, Architecture.Arm64));
        Assert.AreEqual("linux-arm", NativeHost.RuntimeIdentifier(NativeOperatingSystem.Linux, Architecture.Arm));
        Assert.AreEqual("osx-x64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.MacOS, Architecture.X64));
        Assert.AreEqual("osx-arm64", NativeHost.RuntimeIdentifier(NativeOperatingSystem.MacOS, Architecture.Arm64));
    }

    /// <summary>Unsupported CPU architectures fail with platform errors instead of silent guesses.</summary>
    [TestMethod]
    public void RuntimeIdentifier_RejectsUnsupportedArchitectures()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => NativeHost.RuntimeIdentifier(NativeOperatingSystem.Windows, Architecture.Arm));
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => NativeHost.RuntimeIdentifier(NativeOperatingSystem.Linux, Architecture.X86));
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => NativeHost.RuntimeIdentifier(NativeOperatingSystem.MacOS, Architecture.Arm));
    }

    /// <summary>Native library file names follow the platform loader conventions.</summary>
    [TestMethod]
    public void LibraryFileName_FormatsPlatformFileNames()
    {
        Assert.AreEqual("fixture.dll", NativeHost.LibraryFileName(NativeOperatingSystem.Windows, "fixture"));
        Assert.AreEqual("libfixture.so", NativeHost.LibraryFileName(NativeOperatingSystem.Linux, "fixture"));
        Assert.AreEqual("libfixture.dylib", NativeHost.LibraryFileName(NativeOperatingSystem.MacOS, "fixture"));
    }

    /// <summary>Unsupported operating system values fail without producing invented identifiers or file names.</summary>
    [TestMethod]
    public void UnsupportedOperatingSystemValues_Throw()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => NativeHost.RuntimeIdentifier((NativeOperatingSystem)999, Architecture.X64));
        Assert.ThrowsException<PlatformNotSupportedException>(
            () => NativeHost.LibraryFileName((NativeOperatingSystem)999, "fixture"));
    }
}
