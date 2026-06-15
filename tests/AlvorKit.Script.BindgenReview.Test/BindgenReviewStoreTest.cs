namespace AlvorKit.Script.BindgenReview.Test;

/// <summary>Tests manifest storage for disposable bindgen review sessions.</summary>
[TestClass]
public sealed class BindgenReviewStoreTest
{
    /// <summary>ReadManifest reports a missing manifest before cleanup can delete arbitrary directories.</summary>
    [TestMethod]
    public void ReadManifest_MissingManifest_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var session = BindgenReviewPaths.Create(workspace.Root, "xxhash", null, () => "a1b2c");

        Assert.ThrowsExactly<InvalidOperationException>(() => new BindgenReviewStore().ReadManifest(session));
    }

    /// <summary>ReadManifest reports a JSON null manifest as invalid.</summary>
    [TestMethod]
    public void ReadManifest_NullManifest_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var session = BindgenReviewPaths.Create(workspace.Root, "xxhash", null, () => "a1b2c");
        Directory.CreateDirectory(session.Root);
        File.WriteAllText(Path.Combine(session.Root, ".bindgen-review.json"), "null");

        Assert.ThrowsExactly<InvalidOperationException>(() => new BindgenReviewStore().ReadManifest(session));
    }
}
