namespace AlvorKit.UI;

[Root]
public class RootUiPosition(RootSprites sprites, RootUiScale scale)
{
    internal void Position(Vec2 s, Vec2 padding, EntMut n)
    {
        PositionNode(s, padding, n);

        foreach (var c in n.NodesR.Span)
            Position(n.SizeR, n.PaddingR.XY, c);

        PositionInnerLayout(n);
    }

    internal void Finalize(Vec2 o, EntMut n)
    {
        n.PositionR = o + n.OffsetR;

        foreach (var sc in n.NodesR.Span)
            Finalize(n.PositionR, sc);
    }

    private void PositionNode(Vec2 s, Vec2 padding, EntMut n)
    {
        n.OffsetR = n.OffsetFV.Resolve();
        PositionPadding(padding, n);
        PositionTextRelative(n);
        PositionAlignement(s, n);
    }

    private void PositionInnerLayout(EntMut n)
    {
        var innerLayout = n.InnerLayoutFV.Resolve();

        if (innerLayout == InnerLayout.VerticalList)
            PositionVerticalList(n);
        else if (innerLayout == InnerLayout.HorizontalList)
            PositionHorizontalList(n);
    }

    private void PositionVerticalList(EntMut n)
    {
        var innerSpacing = n.InnerSpacingFV.Resolve();
        float y = n.InnerScrollOffsetFV.Resolve().Y;

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            if (c.IsFloatingFV.Resolve())
                continue;

            y += c.MarginR.Y;
            ce.OffsetR += (c.MarginR.X, y);
            y += c.SizeR.Y + c.MarginR.W;
            y += innerSpacing;
        }
    }

    private void PositionHorizontalList(EntMut n)
    {
        var innerSpacing = n.InnerSpacingFV.Resolve();
        float x = n.InnerScrollOffsetFV.Resolve().X;

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            if (c.IsFloatingFV.Resolve())
                continue;

            x += c.MarginR.X;
            ce.OffsetR += (x, c.MarginR.Y);
            x += c.SizeR.X + c.MarginR.Z;
            x += innerSpacing;
        }
    }

    private void PositionPadding(Vec2 padding, EntMut n)
    {
        var alignment = n.AlignmentFV.Resolve();
        if ((alignment & (Alignment.Right | Alignment.Horizontal)) == 0)
            n.OffsetR += (padding.X, 0);
        if ((alignment & (Alignment.Bottom | Alignment.Vertical)) == 0)
            n.OffsetR += (0, padding.Y);
    }

    private void PositionAlignement(Vec2 s, EntMut n)
    {
        var alignment = n.AlignmentFV.Resolve();
        n.OffsetR = Align(n.OffsetR, n.SizeR, s, alignment, n.SnapR);
    }

    private void PositionTextRelative(EntMut n)
    {
        var font = n.FontFV.Resolve();
        if (font == null)
            return;

        var fontSize = (int)(n.FontSizeFV.Resolve() * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = n.TextFV.Resolve();
        if (text.IsEmpty)
            return;

        var size = new Vec2(sprites.Batch.Measure(font.Size(fontSize), text), font.Size(fontSize).Metrics.Height) / scale.Scale;
        n.OffsetR += n.OffsetTextRelativeFV.Resolve() * size;
    }

    internal Vec2 Align(Vec2 val, Vec2 size, Vec2 parent, Alignment alignment, float snap)
    {
        if ((alignment & Alignment.Horizontal) != 0)
            val.X += Snap.Round(parent.X / 2, snap) - size.X / 2;
        if ((alignment & Alignment.Vertical) != 0)
            val.Y += Snap.Round(parent.Y / 2, snap) - size.Y / 2;

        if ((alignment & Alignment.Right) != 0)
            val.X += parent.X - size.X;
        if ((alignment & Alignment.Bottom) != 0)
            val.Y += parent.Y - size.Y;

        return val;
    }

}
