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

        Assert.AreEqual("libc.so.6,libm.so.6", string.Join(",", NativeDependencyVerifier.ElfDependencies(output)));
    }

    /// <summary>Malformed dependency lines without a closing bracket are ignored.</summary>
    [TestMethod]
    public void ElfDependencies_MalformedLine_Ignores()
    {
        var dependencies = NativeDependencyVerifier.ElfDependencies("0x0001 (NEEDED) Shared library: [libc.so.6\n");

        Assert.AreEqual(0, dependencies.Count);
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
        Assert.ThrowsExactly<InvalidOperationException>(
            () => NativeDependencyVerifier.EnsureElfDependenciesAllowed(["libbad.so.1"], ["libc.so.6"]));
    }
}
