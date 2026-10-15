namespace AlvorKit;

/// <summary>Observes native image-cursor calls without owning desktop resources.</summary>
internal class ImageCursorTestGlfw : GlfwNoop
{
    private Vec2u size;
    private Vec2i hotspot;
    private Vec4u8[] pixels = [];
    private GlfwCursor created = new((nint)19);
    private GlfwWindow window;
    private GlfwCursor applied;
    private int destroyed;

    internal Vec2u Size => size;
    internal Vec2i Hotspot => hotspot;
    internal ReadOnlySpan<Vec4u8> Pixels => pixels;
    internal GlfwWindow Window => window;
    internal GlfwCursor Applied => applied;
    internal int Destroyed => destroyed;
    internal ref GlfwCursor Created => ref created;

    public override unsafe GlfwCursor CreateCursor(in GlfwImage image, int xhot, int yhot)
    {
        size = new((uint)image.Width, (uint)image.Height);
        hotspot = new(xhot, yhot);
        pixels = [.. new ReadOnlySpan<Vec4u8>((void*)image.Pixels, image.Width * image.Height)];
        return created;
    }

    public override void SetCursor(GlfwWindow target, GlfwCursor cursor)
    {
        window = target;
        applied = cursor;
    }

    public override void DestroyCursor(GlfwCursor cursor)
    {
        Assert.AreEqual(created, cursor);
        destroyed++;
    }
}
