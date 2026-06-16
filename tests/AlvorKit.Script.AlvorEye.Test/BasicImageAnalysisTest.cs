namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests lightweight image analysis helpers.</summary>
[TestClass]
public sealed class BasicImageAnalysisTest
{
    /// <summary>Detects nonblank images, target colors, and changed bounds.</summary>
    [TestMethod]
    public void Analyze_ColoredPixel_ReturnsExpectedSummary()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            return;

        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("frame.png");
        using (var bitmap = new Bitmap(4, 4))
        {
            bitmap.SetPixel(2, 1, Color.Red);
            bitmap.Save(path, ImageFormat.Png);
        }

        var result = BasicImageAnalysis.Analyze(path, color: "#ff0000");

        Assert.IsTrue(result.NonBlank);
        Assert.AreEqual(1, result.ColorHits);
        Assert.AreEqual(2, result.MinX);
        Assert.AreEqual(1, result.MinY);
        Assert.AreEqual(2, result.MaxX);
        Assert.AreEqual(1, result.MaxY);
    }

    /// <summary>Rejects malformed requested colors.</summary>
    [TestMethod]
    public void Analyze_InvalidColor_Throws()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            return;

        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("frame.png");
        TestPng.WriteRed(path);

        try
        {
            BasicImageAnalysis.Analyze(path, color: "#f00");
            Assert.Fail("Expected malformed colors to fail.");
        }
        catch (ArgumentException)
        {
        }
    }
}
