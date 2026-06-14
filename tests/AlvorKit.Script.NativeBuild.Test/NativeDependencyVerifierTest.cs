using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for native dependency parsing and allow-list enforcement.</summary>
[TestClass]
public sealed class NativeDependencyVerifierTest
{
    /// <summary>readelf output is reduced to dependency names.</summary>
    [TestMethod]
    public void ElfDependencies_ExtractsSharedLibraries()
    {
        const string output = """
            0x0000000000000001 (NEEDED)             Shared library: [libc.so.6]
            0x0000000000000001 (NEEDED)             Shared library: [libm.so.6]
            """;

        CollectionAssert.AreEqual(new[] { "libc.so.6", "libm.so.6" }, NativeDependencyVerifier.ElfDependencies(output).ToArray());
    }

    /// <summary>Loader dependencies are allowed even when absent from the manifest list.</summary>
    [TestMethod]
    public void EnsureElfDependenciesAllowed_IgnoresLdLinux()
    {
        NativeDependencyVerifier.EnsureElfDependenciesAllowed(["ld-linux-aarch64.so.1", "libc.so.6"], ["libc.so.6"]);
    }

    /// <summary>Unexpected dependencies fail verification.</summary>
    [TestMethod]
    public void EnsureElfDependenciesAllowed_UnexpectedDependency_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(
            () => NativeDependencyVerifier.EnsureElfDependenciesAllowed(["libbad.so.1"], ["libc.so.6"]));
    }
}
