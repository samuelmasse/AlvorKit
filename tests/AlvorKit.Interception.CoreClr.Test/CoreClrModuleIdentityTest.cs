namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies managed PE module identity evidence without loading a second assembly.</summary>
[TestClass]
public sealed class CoreClrModuleIdentityTest
{
    /// <summary>Reads the exact MVID reported by the already loaded CoreClr assembly.</summary>
    [TestMethod]
    public void ReadModuleMvidReturnsAssemblyIdentity()
    {
        var assembly = typeof(CoreClrModuleIdentity).Assembly;

        var moduleVersionId = CoreClrModuleIdentity.ReadModuleMvid(
            assembly.Location);

        Assert.AreEqual(
            assembly.ManifestModule.ModuleVersionId,
            moduleVersionId);
    }

    /// <summary>Accepts the exact expected MVID and rejects a deterministic mismatch.</summary>
    [TestMethod]
    public void ValidateModuleMvidRequiresExactIdentity()
    {
        var assembly = typeof(CoreClrModuleIdentity).Assembly;
        var actual = assembly.ManifestModule.ModuleVersionId;
        var wrongBytes = actual.ToByteArray();
        wrongBytes[0] ^= 0x01;
        var wrong = new Guid(wrongBytes);

        CoreClrModuleIdentity.ValidateModuleMvid(
            assembly.Location,
            actual);
        var error = Assert.ThrowsExactly<InvalidDataException>(
            () => CoreClrModuleIdentity.ValidateModuleMvid(
                assembly.Location,
                wrong));

        StringAssert.Contains(error.Message, $"expected {wrong:D}");
        StringAssert.Contains(error.Message, $"actual {actual:D}");
    }
}
