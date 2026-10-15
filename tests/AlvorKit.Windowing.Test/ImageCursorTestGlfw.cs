namespace AlvorKit;

/// <summary>Observes native image-cursor calls without owning desktop resources.</summary>
internal class ImageCursorTestGlfw : GlfwNoop
{
    /// <summary>The last custom-image dimensions.</summary>
    private Vec2u size;
    /// <summary>The last custom-image hotspot.</summary>
    private Vec2i hotspot;
    /// <summary>A copy of the creation pixels, made only by this test double.</summary>
    private Vec4u8[] pixels = [];
    /// <summary>The native handle returned by creation.</summary>
    private GlfwCursor created = new((nint)19);
    /// <summary>The window last assigned a cursor.</summary>
    private GlfwWindow window;
    /// <summary>The handle last assigned to the window.</summary>
    private GlfwCursor applied;
    /// <summary>The number of native destruction calls.</summary>
    private int destroyed;
    /// <summary>The configured platform for metrics dispatch.</summary>
    private GlfwPlatform platform = GlfwPlatform.PlatformNull;

    /// <summary>Gets the observed image dimensions.</summary>
    internal Vec2u Size => size;
    /// <summary>Gets the observed image hotspot.</summary>
    internal Vec2i Hotspot => hotspot;
    /// <summary>Gets the observed creation pixels.</summary>
    internal ReadOnlySpan<Vec4u8> Pixels => pixels;
    /// <summary>Gets the window last assigned a cursor.</summary>
    internal GlfwWindow Window => window;
    /// <summary>Gets the handle last assigned to the window.</summary>
    internal GlfwCursor Applied => applied;
    /// <summary>Gets the number of destruction calls.</summary>
    internal int Destroyed => destroyed;
    /// <summary>Allows a test to configure the native creation result.</summary>
    internal ref GlfwCursor Created => ref created;
    /// <summary>Allows a test to configure the initialized desktop platform.</summary>
    internal ref GlfwPlatform Platform => ref platform;

    /// <inheritdoc />
    public override GlfwPlatform GetPlatform() => platform;

    /// <inheritdoc />
    public override unsafe GlfwCursor CreateCursor(in GlfwImage image, int xhot, int yhot)
    {
        size = new((uint)image.Width, (uint)image.Height);
        hotspot = new(xhot, yhot);
        pixels = [.. new ReadOnlySpan<Vec4u8>((void*)image.Pixels, image.Width * image.Height)];
        return created;
    }

    /// <inheritdoc />
    public override void SetCursor(GlfwWindow target, GlfwCursor cursor)
    {
        window = target;
        applied = cursor;
    }

    /// <inheritdoc />
    public override void DestroyCursor(GlfwCursor cursor)
    {
        Assert.AreEqual(created, cursor);
        destroyed++;
    }
}
