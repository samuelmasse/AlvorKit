namespace AlvorKit;

/// <summary>Tests shared font context resource ownership.</summary>
[TestClass]
public class FontContextTest
{
    /// <summary>Context disposal releases FreeType while GPU resources remain owned by the GL layer.</summary>
    [TestMethod]
    public void ConstructorAndDispose_InitializesAndReleasesResources()
    {
        var (backend, driver, _, context) = FontsTestHarness.CreateContext();

        Assert.AreEqual(1, driver.InitFreeTypeCount);
        Assert.AreEqual(0, driver.DoneFreeTypeCount);
        var deletedBeforeDispose = backend.Deleted.Count;

        context.Dispose();

        Assert.AreEqual(1, driver.DoneFreeTypeCount);
        Assert.AreEqual(deletedBeforeDispose, backend.Deleted.Count);

        context.GL.Dispose();

        Assert.IsTrue(backend.Deleted.Count > deletedBeforeDispose);
        driver.Dispose();
    }
}
