namespace AlvorKit;

/// <summary>Checks native cursor image, hotspot, ownership, and allocation contracts.</summary>
[TestClass]
public class GlfwImageCursorTest
{
    /// <summary>Preserves top-down straight-alpha pixels and applies the created handle to the requested window.</summary>
    [TestMethod]
    public void ImageAndHotspotReachNativeCursorUnchanged()
    {
        var glfw = new ImageCursorTestGlfw();
        Vec4u8[] pixels = [new(240, 120, 60, 128), new(1, 2, 3, 0), new(4, 5, 6, 255), new(7, 8, 9, 64)];
        using var cursor = new GlfwImageCursor(glfw, new(2, 2), pixels, new(1, 0));
        var window = new GlfwWindow((nint)7);
        cursor.Apply(window);

        Assert.AreEqual(new Vec2u(2, 2), glfw.Size);
        Assert.AreEqual(new Vec2i(1, 0), glfw.Hotspot);
        Assert.IsTrue(glfw.Pixels.SequenceEqual(pixels));
        Assert.AreEqual(window, glfw.Window);
        Assert.AreEqual(glfw.Created, glfw.Applied);
    }

    /// <summary>Repeated disposal releases the native handle once and prevents further activation.</summary>
    [TestMethod]
    public void DisposalReleasesCursorExactlyOnce()
    {
        var glfw = new ImageCursorTestGlfw();
        var cursor = new GlfwImageCursor(glfw, new(1), [new(255)], default);
        cursor.Dispose();
        cursor.Dispose();

        Assert.AreEqual(1, glfw.Destroyed);
        Assert.ThrowsException<ObjectDisposedException>(() => cursor.Apply(new((nint)1)));
    }

    /// <summary>Rejects empty or overflowing images, mismatched storage, and hotspots outside the image.</summary>
    [TestMethod]
    public void InvalidImageBoundsAreRejectedBeforeNativeAccess()
    {
        var glfw = new ImageCursorTestGlfw();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(0, 1), [], default));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(1, 0), [], default));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(uint.MaxValue, 1), [], default));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(1, uint.MaxValue), [], default));
        Assert.ThrowsException<ArgumentException>(() => new GlfwImageCursor(glfw, new(2), [new(255)], default));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(1), [new(255)], new(1, 0)));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GlfwImageCursor(glfw, new(1), [new(255)], new(0, 1)));
        Assert.AreEqual(0, glfw.Pixels.Length);
    }

    /// <summary>Reports native creation failure without treating a null handle as a usable cursor.</summary>
    [TestMethod]
    public void NativeCreationFailureIsReported()
    {
        var glfw = new ImageCursorTestGlfw();
        glfw.Created = default;
        Assert.ThrowsException<InvalidOperationException>(() => new GlfwImageCursor(glfw, new(1), [new(255)], default));
    }

    /// <summary>Switching cached cursor frames does not allocate managed memory.</summary>
    [TestMethod]
    public void ApplyingAnExistingCursorDoesNotAllocate()
    {
        var glfw = new ImageCursorTestGlfw();
        using var cursor = new GlfwImageCursor(glfw, new(1), [new(255)], default);
        var window = new GlfwWindow((nint)7);
        cursor.Apply(window);
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var index = 0; index < 1000; index++)
            cursor.Apply(window);

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
