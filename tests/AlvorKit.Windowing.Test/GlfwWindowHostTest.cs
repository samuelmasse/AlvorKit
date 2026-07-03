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
            CursorShape = CursorShape.Hand,
            WindowState = WindowState.Maximized
        };
        host.SwapBuffers();
        host.Close();

        Assert.IsTrue(host.IsVisible);
        Assert.AreEqual(new Vec2u(320u, 200u), host.ClientSize);
        Assert.AreEqual(CursorMode.Captured, host.CursorMode);
        Assert.AreEqual(CursorShape.Hand, host.CursorShape);
        CollectionAssert.AreEqual(new[] { GlfwCursorShape.PointingHand }, glfw.CreatedCursorShapes);
        Assert.AreEqual(1, glfw.SetCursors.Count);
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
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.CursorShape = (CursorShape)999);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.WindowState = (WindowState)999);
    }

    /// <summary>Verifies GLFW standard cursors are cached and default restores the platform cursor.</summary>
    [TestMethod]
    public void GlfwWindowHost_CursorShape_CachesAndRestoresDefault()
    {
        var glfw = new WindowingTestGlfw(new(100, 80));
        var host = new GlfwWindowHost(glfw, glfw.Window)
        {
            CursorShape = CursorShape.Text
        };
        host.CursorShape = CursorShape.Text;
        host.CursorShape = CursorShape.Crosshair;
        host.CursorShape = CursorShape.Default;

        CollectionAssert.AreEqual(new[] { GlfwCursorShape.IBeam, GlfwCursorShape.Crosshair }, glfw.CreatedCursorShapes);
        Assert.AreEqual(CursorShape.Default, host.CursorShape);
        Assert.AreEqual(3, glfw.SetCursors.Count);
        Assert.AreEqual((nint)11, glfw.SetCursors[0].Handle);
        Assert.AreEqual((nint)12, glfw.SetCursors[1].Handle);
        Assert.AreEqual(nint.Zero, glfw.SetCursors[2].Handle);

        host.Dispose();

        Assert.AreEqual(4, glfw.SetCursors.Count);
        Assert.AreEqual(nint.Zero, glfw.SetCursors[3].Handle);
        Assert.AreEqual(2, glfw.DestroyedCursors.Count);
        Assert.AreEqual(glfw.SetCursors[0], glfw.DestroyedCursors[0]);
        Assert.AreEqual(glfw.SetCursors[1], glfw.DestroyedCursors[1]);
    }

    /// <summary>Verifies every platform-neutral standard shape maps to the expected GLFW shape.</summary>
    [TestMethod]
    public void GlfwWindowHost_CursorShape_MapsStandardShapes()
    {
        var glfw = new WindowingTestGlfw(new(100, 80));
        using var host = new GlfwWindowHost(glfw, glfw.Window);
        var expected = new (CursorShape Shape, GlfwCursorShape Glfw)[]
        {
            (CursorShape.Text, GlfwCursorShape.IBeam),
            (CursorShape.Crosshair, GlfwCursorShape.Crosshair),
            (CursorShape.Hand, GlfwCursorShape.PointingHand),
            (CursorShape.ResizeHorizontal, GlfwCursorShape.ResizeEw),
            (CursorShape.ResizeVertical, GlfwCursorShape.ResizeNs),
            (CursorShape.ResizeDiagonalDown, GlfwCursorShape.ResizeNwse),
            (CursorShape.ResizeDiagonalUp, GlfwCursorShape.ResizeNesw),
            (CursorShape.Move, GlfwCursorShape.ResizeAll),
            (CursorShape.NotAllowed, GlfwCursorShape.NotAllowed)
        };

        foreach (var item in expected)
            host.CursorShape = item.Shape;

        Assert.AreEqual(expected.Length, glfw.CreatedCursorShapes.Count);
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].Glfw, glfw.CreatedCursorShapes[i]);
    }
}
