namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests shared font context resource ownership.</summary>
[TestClass]
public sealed class FontContextTest
{
    /// <summary>Construction initializes FreeType and disposal releases context-owned resources.</summary>
    [TestMethod]
    public void ConstructorAndDispose_InitializesAndReleasesResources()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();

        Assert.AreEqual(1, driver.InitFreeTypeCount);
        Assert.AreEqual(0, driver.DoneFreeTypeCount);

        context.Dispose();
        batch.Dispose();
        driver.Dispose();

        Assert.AreEqual(1, driver.DoneFreeTypeCount);
        Assert.IsTrue(backend.Deleted.Count > 0);
    }
}
