namespace AlvorKit.UI;

[Root]
public class RootUiSize(RootSprites sprites, RootUiScale scale)
{
    internal void Size(Vec2 s, EntMut n)
    {
        SizeNode(s, n);
        n.PaddingR = n.PaddingFV.Resolve();

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            ce.MarginR = c.MarginFV.Resolve();
        }

        SizeInnerSizing(n);

        var innerSpace = n.SizeR - n.PaddingR.XY - n.PaddingR.ZW;
        foreach (var c in n.NodesR.Span)
        {
            var childSpace = c.IsFloatingFV.Resolve() ? n.SizeR : innerSpace;
            Size(childSpace - c.MarginR.XY - c.MarginR.ZW, c);
        }

        SizeInnerMaxRelative(n);
        SizeInnerSumRelative(n);

        SizeAlignmentSnap(n);
        SizeEdgeAlignmentFill(s, n);

        var postInnerSpace = n.SizeR - n.PaddingR.XY - n.PaddingR.ZW;
        foreach (var c in n.NodesR.Span)
        {
            if (!c.IsPostSizedFV.Resolve())
                continue;

            var childSpace = c.IsFloatingFV.Resolve() ? n.SizeR : postInnerSpace;
            Size(childSpace - c.MarginR.XY - c.MarginR.ZW, c);
        }
    }

    private void SizeNode(Vec2 s, EntMut n)
    {
        n.SizeR = default;
        n.SizeR += (n.SizeRelativeFV.Resolve() ?? (1, 1)) * s;
        n.SizeR += n.SizeFV.Resolve();
        SizeTextRelative(n);

        var hor = n.HorizontalWeightSizeR;
        if (hor != null)
            n.SizeR = n.SizeR with { X = hor.GetValueOrDefault() };

        var ver = n.VerticalWeightSizeR;
        if (ver != null)
            n.SizeR = n.SizeR with { Y = ver.GetValueOrDefault() };
    }

    private void SizeTextRelative(EntMut n)
    {
        var font = n.FontFV.Resolve();
        if (font == null)
            return;

        var fontSize = (int)(n.FontSizeFV.Resolve() * scale.Scale);
        if (fontSize <= 0)
            return;

        var text = n.TextFV.Resolve();
        var sizeTextRelative = n.SizeTextRelativeFV.Resolve();
        var glyphSnap = n.TextGlyphAlignmentSnapFV.Resolve() * scale.Scale;
        var size = new Vec2(
            sprites.Batch.Measure(font.Size(fontSize), text, glyphSnap) / scale.Scale,
            font.Size(fontSize).Metrics.Height / scale.Scale);

        var fontPadding = n.FontPaddingFV.Resolve();
        var textPadding = n.TextPaddingFV.Resolve();

        size += fontPadding.XY + fontPadding.ZW;
        size += textPadding.XY + textPadding.ZW;

        n.SizeR += sizeTextRelative * size;
    }

    private void SizeInnerMaxRelative(EntMut n)
    {
        var sizeInnerMaxRelative = n.SizeInnerMaxRelativeFV.Resolve();
        if (sizeInnerMaxRelative == default)
            return;

        var sizeInnerMax = Vec2.Zero;

        foreach (var c in n.NodesR.Span)
        {
            if (c.IsFloatingFV.Resolve())
                continue;

            sizeInnerMax.X = Math.Max(c.SizeR.X + c.MarginR.X + c.MarginR.Z, sizeInnerMax.X);
            sizeInnerMax.Y = Math.Max(c.SizeR.Y + c.MarginR.Y + c.MarginR.W, sizeInnerMax.Y);
        }

        sizeInnerMax.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerMax.Y += n.PaddingR.Y + n.PaddingR.W;

        n.SizeR += sizeInnerMaxRelative * sizeInnerMax;
    }

    private void SizeInnerSumRelative(EntMut n)
    {
        var sizeInnerSumRelative = n.SizeInnerSumRelativeFV.Resolve();
        if (sizeInnerSumRelative == default)
            return;

        var sizeInnerSum = Vec2.Zero;
        var layoutChildCount = 0;

        foreach (var c in n.NodesR.Span)
        {
            if (c.IsFloatingFV.Resolve())
                continue;

            layoutChildCount++;
            sizeInnerSum += c.SizeR + c.MarginR.XY + c.MarginR.ZW;
        }

        sizeInnerSum.X += n.PaddingR.X + n.PaddingR.Z;
        sizeInnerSum.Y += n.PaddingR.Y + n.PaddingR.W;

        var innerSpacing = n.InnerSpacingFV.Resolve();
        var innerSum = sizeInnerSum + new Vec2(innerSpacing) * Math.Max(0, layoutChildCount - 1);
        n.SizeR += sizeInnerSumRelative * innerSum;
        n.SizeInnerSumR = innerSum;
    }

    private void SizeAlignmentSnap(EntMut n)
    {
        var sizeSnap = n.SizeAlignmentSnapFV.Resolve();
        n.SizeR = (Snap.Ceiling(n.SizeR.X, sizeSnap), Snap.Ceiling(n.SizeR.Y, sizeSnap));
    }

    private void SizeEdgeAlignmentFill(Vec2 s, EntMut n)
    {
        if (n.SnapR <= 0)
            return;

        var alignment = n.AlignmentFV.Resolve();
        if ((alignment & Alignment.Right) != 0)
        {
            var desiredLeft = s.X - n.SizeR.X;
            n.SizeR = n.SizeR with { X = n.SizeR.X + desiredLeft - Snap.Floor(desiredLeft, n.SnapR) };
        }
        if ((alignment & Alignment.Bottom) != 0)
        {
            var desiredTop = s.Y - n.SizeR.Y;
            n.SizeR = n.SizeR with { Y = n.SizeR.Y + desiredTop - Snap.Floor(desiredTop, n.SnapR) };
        }
    }

    private void SizeInnerSizing(EntMut n)
    {
        var innerSizing = n.InnerSizingFV.Resolve();
        if (innerSizing == default)
            return;

        float totalWeight = 0;
        var layoutChildCount = 0;

        foreach (var c in n.NodesR.Span)
        {
            var ce = c;
            ce.HorizontalWeightSizeR = null;
            ce.VerticalWeightSizeR = null;

            if (c.IsFloatingFV.Resolve())
                continue;

            layoutChildCount++;

            if (IsSelfWeight(c))
                continue;

            totalWeight += c.SizeWeightFV.Resolve() ?? 1;
        }

        if (totalWeight <= 0)
            return;

        var innerSpacing = n.InnerSpacingFV.Resolve();
        var totalSpacing = innerSpacing * Math.Max(0, layoutChildCount - 1);
        Vec2 useableSize = n.SizeR - n.PaddingR.XY - n.PaddingR.ZW - (totalSpacing, totalSpacing);

        if (innerSizing == InnerSizing.HorizontalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                if (c.IsFloatingFV.Resolve())
                    continue;

                useableSize.X -= c.MarginR.X + c.MarginR.Z;

                if (IsSelfWeight(c))
                    useableSize.X -= c.SizeR.X;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (c.IsFloatingFV.Resolve() || IsSelfWeight(c))
                    continue;

                var ce = c;
                ce.HorizontalWeightSizeR = ((c.SizeWeightFV.Resolve() ?? 1) / totalWeight) * useableSize.X;
            }
        }
        else if (innerSizing == InnerSizing.VerticalWeight)
        {
            foreach (var c in n.NodesR.Span)
            {
                if (c.IsFloatingFV.Resolve())
                    continue;

                useableSize.Y -= c.MarginR.Y + c.MarginR.W;

                if (IsSelfWeight(c))
                    useableSize.Y -= c.SizeR.Y;
            }

            foreach (var c in n.NodesR.Span)
            {
                if (c.IsFloatingFV.Resolve() || IsSelfWeight(c))
                    continue;

                var ce = c;
                ce.VerticalWeightSizeR = ((c.SizeWeightFV.Resolve() ?? 1) / totalWeight) * useableSize.Y;
            }
        }
    }

    private bool IsSelfWeight(EntMut n) => n.SizeWeightTypeFV.Resolve() == SizeWeightType.Self;
}
