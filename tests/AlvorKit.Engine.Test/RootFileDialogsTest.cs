namespace AlvorKit.Engine.Test;

/// <summary>Tests the root-scoped file-dialog facade.</summary>
[TestClass]
public sealed class RootFileDialogsTest
{
    /// <summary>The root facade forwards filters and paths while preserving cancellation.</summary>
    [TestMethod]
    public void OpenFile_ForwardsRequestAndPreservesCancellation()
    {
        var host = new EngineTestFileDialogHost();
        var dialogs = new RootFileDialogs(host);
        var filter = new FileDialogFilter("NES ROM", "nes");

        var result = dialogs.OpenFile([filter], "C:/roms");

        Assert.IsNull(result);
        CollectionAssert.AreEqual(new[] { filter }, host.Filters);
        Assert.AreEqual("C:/roms", host.DefaultPath);
    }
}
