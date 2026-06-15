namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class NativeExportVerifierTest
{
    [TestMethod]
    public void Verify_ReturnsMissingLibraryResultWithoutLoading()
    {
        var model = new BindingModel([], [], [], [], [new("test_missing", "Missing", "void", "void", [], null)], [], [], []);
        var path = Path.Combine(Path.GetTempPath(), "AlvorKit.Script.Bindgen.CHeaders.Test", Guid.NewGuid().ToString("N"), "missing.dll");

        var result = NativeExportVerifier.Verify(path, model);

        Assert.IsFalse(result.LibraryExists);
        Assert.IsFalse(result.AllExportsFound);
        Assert.AreEqual(path, result.LibraryPath);
        Assert.AreEqual(0, result.MissingFunctions.Count);
    }
}
