namespace AlvorKit.UI;

[Root]
public class RootUiDraw(RootSprites sprites, RootUiScale scale, RootUiPosition position, RootUiClipping clipping)
{
    internal void Draw(Ent n)
    {
        var clip = sprites.Batch.Clip;
        sprites.Batch.Clip = clipping.IntersectClips(clip, new(n.PositionR, n.PositionR + n.SizeR));

        DrawNode(n);
        foreach (var sc in n.NodesR.Span)
        {
            if (sc.IsFloatingFV.Resolve())
                continue;

            Draw(sc);
        }

        foreach (var sc in n.NodesR.Span)
        {
            if (!sc.IsFloatingFV.Resolve())
                continue;

            Draw(sc);
        }

        sprites.Batch.Clip = clip;
    }

    private void DrawNode(Ent n)
    {
        DrawFlatSurface(n);
        DrawTexture(n);
        DrawText(n);

        n.OnDrawFV.Resolve()?.Invoke();
        n.OnFrameFV.Resolve()?.Invoke();
    }

    private void DrawFlatSurface(Ent n)
    {
        var color = n.ColorFV.Resolve();
        if (n.SizeR == (0, 0) || color.W == 0)
            return;

        sprites.Batch.Draw(n.PositionR, n.SizeR, color);
    }

    private void DrawTexture(Ent n)
    {
        var texture = n.TextureFV.Resolve();
        if (texture == null)
            return;

        var color = n.TextureColorFV.Resolve() ?? Vec4.One;
        var margin = n.TextureMarginFV.Resolve();
        var position = n.PositionR + margin.Xy;
        var size = n.SizeR - margin.Xy - margin.Zw;
        var subSizeRelative = n.TextureSubSizeRelativeFV.Resolve();
        var subSizeFixed = n.TextureSubSizeFV.Resolve();
        var subSize = subSizeRelative.HasValue || subSizeFixed.HasValue
            ? (subSizeRelative ?? default) * size + (subSizeFixed ?? default)
            : texture.Size;
        var anchor = n.TextureOriginRelativeFV.Resolve();
        var subPosition = (n.TextureSubPositionFV.Resolve() ?? Vec2.Zero)
            - (anchor ?? default) * subSize;
        var textureSnap = n.TextureAlignmentSnapFV.Resolve();
        subPosition = (Snap.Round(subPosition.X, textureSnap), Snap.Round(subPosition.Y, textureSnap));
        var rotation = n.TextureRotationFV.Resolve() ?? SpriteBatchRotation.None;
        var flip = n.TextureFlipFV.Resolve() ?? SpriteBatchFlip.None;

        sprites.Batch.Draw(texture, position, size, subPosition, subSize, color, rotation, flip);
    }

    private void DrawText(Ent n)
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

        var textColor = n.TextColorFV.Resolve();
        if (textColor.W == 0)
            return;

        var alignment = n.TextAlignmentFV.Resolve() ?? Alignment.Center;
        var glyphSnap = n.TextGlyphAlignmentSnapFV.Resolve() * scale.Scale;
        var size = new Vec2(
            sprites.Batch.Measure(font.Size(fontSize), text, glyphSnap) / scale.Scale,
            font.Size(fontSize).Metrics.Height / scale.Scale);
        var fontPadding = n.FontPaddingFV.Resolve();
        var textPadding = n.TextPaddingFV.Resolve();
        var contentOffset = fontPadding.Xy + textPadding.Xy;
        var contentSize = n.SizeR - contentOffset - fontPadding.Zw - textPadding.Zw;

        var offset = position.Align(contentOffset, size, contentSize, alignment, 0);
        offset.Y += size.Y / 2;
        offset += n.TextOffsetFV.Resolve();

        var blockSnap = n.TextAlignmentSnapFV.Resolve();
        offset.X = Snap.Round(offset.X, blockSnap);
        offset.Y = Snap.Round(offset.Y, blockSnap);

        var pos = n.PositionR + offset;

        var shadowOffset = n.TextShadowOffsetFV.Resolve();
        if (shadowOffset.HasValue)
        {
            var shadowColor = n.TextShadowColorFV.Resolve()
                ?? (n.TextShadowColorRelativeFV.Resolve() ?? Vec4.One) * textColor;
            sprites.Batch.Write(font.Size(fontSize), text, (pos + shadowOffset.Value) * scale.Scale, shadowColor, scale.Scale, glyphSnap);
        }

        sprites.Batch.Write(font.Size(fontSize), text, pos * scale.Scale, textColor, scale.Scale, glyphSnap);
    }
}
