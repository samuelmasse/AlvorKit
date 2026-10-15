namespace AlvorKit;

/// <summary>Checks desktop cursor-size policy without modifying OS display or accessibility settings.</summary>
[TestClass]
public class GlfwCursorSizeTest
{
    /// <summary>Cocoa image units remain logical points; the null platform uses the original nominal image size.</summary>
    [DataTestMethod]
    [DataRow(GlfwPlatform.PlatformCocoa)]
    [DataRow(GlfwPlatform.PlatformNull)]
    public void LogicalImagePlatformsDoNotApplyFramebufferScale(GlfwPlatform platform)
    {
        var glfw = new ImageCursorTestGlfw
        {
            Platform = platform
        };
        Assert.AreEqual(new Vec2u(32), new GlfwCursorSize(glfw).Get(default));
    }

    /// <summary>Preserves valid Wayland theme sizes and GLFW's documented theme selection for missing or invalid settings.</summary>
    [DataTestMethod]
    [DataRow(null, 16u)]
    [DataRow("", 16u)]
    [DataRow("0", 16u)]
    [DataRow("-1", 16u)]
    [DataRow("invalid", 16u)]
    [DataRow("24", 24u)]
    [DataRow("48", 48u)]
    public void WaylandUsesGlfwThemeSize(string? configured, uint expected) =>
        Assert.AreEqual(expected, GlfwCursorSize.WaylandSize(configured));

    /// <summary>A missing platform is reported instead of silently inventing desktop cursor dimensions.</summary>
    [TestMethod]
    public void UninitializedPlatformIsRejected()
    {
        var glfw = new ImageCursorTestGlfw
        {
            Platform = GlfwPlatform.AnyPlatform
        };
        Assert.ThrowsException<InvalidOperationException>(() => new GlfwCursorSize(glfw).Get(default));
    }
}
