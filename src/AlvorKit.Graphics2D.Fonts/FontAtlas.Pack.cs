namespace AlvorKit.Graphics2D.Fonts;

internal sealed partial class FontAtlas
{
    /// <summary>Rebuilds the atlas in descending glyph-height order when that layout fits.</summary>
    internal void Pack()
    {
        full = false;
        packed = true;

        if (!CanPack())
            return;

        var ordered = OrderedSlots();
        batch.Begin(buffer.Tablet.Texture.Size);
        used = new int[buffer.Tablet.Size];
        cursor = 0;

        foreach (var slot in ordered)
        {
            var (x, y) = NextSlot(slot.Glyph);
            batch.Writer.Draw(tablet.Texture, new Vec2(x, y), slot.Glyph.Box, slot.Position, slot.Glyph.Box);
            Advance(slot.Glyph, x, y);
            slot.Texture = buffer.Tablet.Texture;
            slot.Position = new Vec2u(checked((uint)x), checked((uint)y));
        }

        RenderRepackedAtlas();
        (tablet, buffer.Tablet) = (buffer.Tablet, tablet);
    }

    /// <summary>Checks whether the descending-height layout fits without changing current placement.</summary>
    private bool CanPack()
    {
        var previousCursor = cursor;
        var previousUsed = used;
        used = new int[tablet.Size];
        cursor = 0;

        var canPack = true;
        foreach (var slot in OrderedSlots())
        {
            var (x, y) = NextSlot(slot.Glyph);
            Advance(slot.Glyph, x, y);
            if (y + checked((int)slot.Glyph.Box.Y) <= tablet.Size)
                continue;

            canPack = false;
            break;
        }

        cursor = previousCursor;
        used = previousUsed;
        return canPack;
    }

    /// <summary>Renders the queued repack sprite batch into the scratch atlas texture.</summary>
    private void RenderRepackedAtlas()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, buffer.Framebuffer);
        gl.FramebufferTexture(GlFramebufferTarget.Framebuffer, GlFramebufferAttachment.ColorAttachment0, buffer.Tablet.Texture.Id, 0);
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        gl.Viewport(0, 0, buffer.Tablet.Size, buffer.Tablet.Size);
        gl.ClearColor(0f, 0f, 0f, 0f);
        gl.Clear(GlClearBufferMask.ColorBufferBit);
        gl.ResetClearColor();
        batch.End();
        gl.ResetViewport();
        gl.ResetDrawBuffers();
        gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer);
    }

    /// <summary>Copies slots into descending glyph-height order.</summary>
    private List<FontGlyphSlot> OrderedSlots()
    {
        var ordered = new List<FontGlyphSlot>(slots);
        ordered.Sort(static (left, right) => right.Glyph.Box.Y.CompareTo(left.Glyph.Box.Y));
        return ordered;
    }
}
