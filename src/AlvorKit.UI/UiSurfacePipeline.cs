namespace AlvorKit;

/// <summary>Runs layout and drawing while one UI surface's scale and viewport are active.</summary>
internal sealed class UiSurfacePipeline(
    RootUiSurfaces owner,
    RootUiContext context,
    RootUiTraverse traverse,
    RootUiSize size,
    RootUiPosition position,
    RootUiDraw draw)
{
    /// <summary>Resolves a surface and lays out its retained UI tree.</summary>
    internal void Prepare(UiSurface surface)
    {
        var active = owner.Activate(surface);
        try
        {
            var root = surface.Root;
            root.IsOrderedFV = true;
            root.SizeFV = context.Size;
            root.SizeRelativeFV = (Vec2?)(0, 0);

            do
            {
                traverse.Traverse(root, null, 0);
                size.Size(root.SizeR, root);
                position.Position(root.SizeR, default, root);
                position.Finalize(root.OffsetR, root);
            }
            while (traverse.Delay(root));
        }
        finally
        {
            owner.Restore(active);
        }
    }

    /// <summary>Draws a retained UI tree inside its active surface context.</summary>
    internal void Draw(UiSurface surface)
    {
        var active = owner.Activate(surface);
        try
        {
            draw.Draw(surface.Root);
        }
        finally
        {
            owner.Restore(active);
        }
    }
}
