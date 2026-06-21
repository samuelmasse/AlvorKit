namespace AlvorKit.Windowing.Test;

[TestClass]
public class GlfwWindowHostTest
{
    /// <summary>Verifies direct GLFW-backed host state writes are forwarded through the generated noop test double.</summary>
    [TestMethod]
    public void GlfwWindowHost_StateWrites_ForwardToGlfw()
    {
        var glfw = new WindowingTestGlfw(new(100, 80));
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            IsVisible = true,
            ClientSize = new(320, 200),
            CursorMode = CursorMode.Captured,
            WindowState = WindowState.Maximized
        };
        host.SwapBuffers();
        host.Close();

        Assert.IsTrue(host.IsVisible);
        Assert.AreEqual(new Vec2u(320u, 200u), host.ClientSize);
        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.AreEqual(1, glfw.MaximizeWindowCalls);
        Assert.AreEqual(1, glfw.SwapBufferCalls);
        Assert.AreEqual(1, glfw.SetWindowShouldCloseCalls);
    }

    /// <summary>Verifies GLFW receives zero sizes while invalid enum values are rejected instead of normalized.</summary>
    [TestMethod]
    public void GlfwWindowHost_ZeroSizeForwardsAndInvalidStateValuesThrow()
    {
        var glfw = new WindowingTestGlfw(new(100, 80));
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            ClientSize = (0u, 1u)
        };

        Assert.AreEqual(new Vec2u(0u, 1u), host.ClientSize);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.CursorMode = (CursorMode)999);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.WindowState = (WindowState)999);
    }
}
