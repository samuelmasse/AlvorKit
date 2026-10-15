namespace AlvorKit;

/// <summary>Owns a native image cursor. Create, apply, and dispose it on the GLFW main thread before terminating GLFW.</summary>
/// <param name="glfw">The GLFW instance that owns the cursor.</param>
/// <param name="size">Image dimensions in pixels.</param>
/// <param name="pixels">Straight-alpha RGBA pixels, left-to-right and top-to-bottom; copied by GLFW during construction.</param>
/// <param name="hotspot">The active pixel relative to the image's top-left corner.</param>
public class GlfwImageCursor(Glfw glfw, Vec2u size, ReadOnlySpan<Vec4u8> pixels, Vec2u hotspot) : IDisposable
{
    /// <summary>The owned native cursor, cleared after destruction.</summary>
    private GlfwCursor cursor = Create(glfw, size, pixels, hotspot);

    /// <summary>Assigns this cursor to a window; pointer movement is handled by the OS independently of rendering.</summary>
    public void Apply(GlfwWindow window)
    {
        ObjectDisposedException.ThrowIf(cursor == default, this);
        glfw.SetCursor(window, cursor);
    }

    /// <summary>Releases the cursor. GLFW restores the default cursor on windows still using it.</summary>
    public void Dispose()
    {
        if (cursor == default)
            return;

        glfw.DestroyCursor(cursor);
        cursor = default;
    }

    /// <summary>Validates image bounds before handing pinned managed pixels to GLFW.</summary>
    private static unsafe GlfwCursor Create(Glfw glfw, Vec2u size, ReadOnlySpan<Vec4u8> pixels, Vec2u hotspot)
    {
        if (size.X == 0 || size.Y == 0 || size.X > int.MaxValue || size.Y > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(size));

        if ((ulong)size.X * size.Y != (ulong)pixels.Length)
            throw new ArgumentException("Cursor pixels must match the image dimensions.", nameof(pixels));

        if (hotspot.X >= size.X || hotspot.Y >= size.Y)
            throw new ArgumentOutOfRangeException(nameof(hotspot));

        fixed (Vec4u8* data = pixels)
        {
            var image = new GlfwImage { Width = (int)size.X, Height = (int)size.Y, Pixels = (nint)data };
            var result = glfw.CreateCursor(in image, (int)hotspot.X, (int)hotspot.Y);

            if (result == default)
                throw new InvalidOperationException("GLFW could not create the native image cursor.");

            return result;
        }
    }
}
