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

    /// <summary>Verifies disabled cursor mode enables GLFW raw mouse motion when the platform supports it.</summary>
    [TestMethod]
    public void GlfwWindowHost_DisabledCursor_EnablesRawMouseMotionWhenSupported()
    {
        var glfw = new WindowingTestGlfw(new(100, 80))
        {
            IsRawMouseMotionSupported = true
        };
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            CursorMode = CursorMode.Disabled
        };

        Assert.AreEqual(CursorMode.Disabled, host.CursorMode);
        Assert.IsTrue(glfw.LastRawMouseMotion);
        Assert.AreEqual(1, glfw.RawMouseMotionSupportedCalls);
        Assert.AreEqual(2, glfw.InputModeCalls.Count);
        Assert.AreEqual((GlfwInputMode.Cursor, (int)GlfwCursorMode.Disabled), glfw.InputModeCalls[0]);
        Assert.AreEqual((GlfwInputMode.RawMouseMotion, 1), glfw.InputModeCalls[1]);
    }

    /// <summary>Verifies leaving disabled cursor mode disables GLFW raw mouse motion when it had been requested.</summary>
    [TestMethod]
    public void GlfwWindowHost_CapturedCursor_DisablesRawMouseMotionWhenSupported()
    {
        var glfw = new WindowingTestGlfw(new(100, 80))
        {
            IsRawMouseMotionSupported = true
        };
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            CursorMode = CursorMode.Disabled
        };

        glfw.InputModeCalls.Clear();
        host.CursorMode = CursorMode.Captured;

        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.IsFalse(glfw.LastRawMouseMotion);
        Assert.AreEqual(2, glfw.RawMouseMotionSupportedCalls);
        Assert.AreEqual(2, glfw.InputModeCalls.Count);
        Assert.AreEqual((GlfwInputMode.Cursor, (int)GlfwCursorMode.Captured), glfw.InputModeCalls[0]);
        Assert.AreEqual((GlfwInputMode.RawMouseMotion, 0), glfw.InputModeCalls[1]);
    }

    /// <summary>Verifies unsupported GLFW raw mouse motion is checked and then left untouched.</summary>
    [TestMethod]
    public void GlfwWindowHost_DisabledCursor_SkipsRawMouseMotionWhenUnsupported()
    {
        var glfw = new WindowingTestGlfw(new(100, 80));
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            CursorMode = CursorMode.Disabled
        };

        Assert.AreEqual(CursorMode.Disabled, host.CursorMode);
        Assert.IsFalse(glfw.LastRawMouseMotion);
        Assert.AreEqual(1, glfw.RawMouseMotionSupportedCalls);
        Assert.AreEqual(1, glfw.InputModeCalls.Count);
        Assert.AreEqual((GlfwInputMode.Cursor, (int)GlfwCursorMode.Disabled), glfw.InputModeCalls[0]);
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
