namespace AlvorKit.Windowing;

/// <summary>Maps AlvorKit cursor shapes to cached GLFW standard cursor handles.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwCursorShapes(Glfw glfw)
{
    private readonly GlfwCursor[] cursors = new GlfwCursor[9];
    private readonly bool[] cached = new bool[9];

    /// <summary>Returns a cached GLFW cursor handle for the requested shape.</summary>
    internal GlfwCursor Get(CursorShape shape)
    {
        if (shape == CursorShape.Default)
            return default;

        var index = ToIndex(shape);
        if (!cached[index])
        {
            cursors[index] = glfw.CreateStandardCursor(ToGlfw(shape));
            cached[index] = true;
        }

        return cursors[index];
    }

    /// <summary>Destroys GLFW cursor handles created by this cache.</summary>
    internal void Destroy()
    {
        for (var i = 0; i < cursors.Length; i++)
        {
            if (cached[i] && cursors[i].Handle != 0)
                glfw.DestroyCursor(cursors[i]);
        }
    }

    private static int ToIndex(CursorShape shape) =>
        shape switch
        {
            CursorShape.Text => 0,
            CursorShape.Crosshair => 1,
            CursorShape.Hand => 2,
            CursorShape.ResizeHorizontal => 3,
            CursorShape.ResizeVertical => 4,
            CursorShape.ResizeDiagonalDown => 5,
            CursorShape.ResizeDiagonalUp => 6,
            CursorShape.Move => 7,
            CursorShape.NotAllowed => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Cursor shape must be a defined value.")
        };

    private static GlfwCursorShape ToGlfw(CursorShape shape) =>
        shape switch
        {
            CursorShape.Text => GlfwCursorShape.IBeam,
            CursorShape.Crosshair => GlfwCursorShape.Crosshair,
            CursorShape.Hand => GlfwCursorShape.PointingHand,
            CursorShape.ResizeHorizontal => GlfwCursorShape.ResizeEw,
            CursorShape.ResizeVertical => GlfwCursorShape.ResizeNs,
            CursorShape.ResizeDiagonalDown => GlfwCursorShape.ResizeNwse,
            CursorShape.ResizeDiagonalUp => GlfwCursorShape.ResizeNesw,
            CursorShape.Move => GlfwCursorShape.ResizeAll,
            CursorShape.NotAllowed => GlfwCursorShape.NotAllowed,
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Cursor shape must be a defined value.")
        };
}
