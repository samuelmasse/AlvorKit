namespace AlvorKit.Graphics2D;

public partial class SpriteBatchWriter
{
    /// <summary>Builds texture-space corners for a subregion, rotation, and flip combination.</summary>
    private static QuadCorners TexCorners(Vec2 subPosition, Vec2 subSize, SpriteBatchRotation rotation, SpriteBatchFlip flip)
    {
        var left = subPosition.X;
        var right = left + subSize.X;
        var top = subPosition.Y;
        var bottom = top + subSize.Y;

        if ((flip & SpriteBatchFlip.Horizontal) != 0)
            (left, right) = (right, left);

        if ((flip & SpriteBatchFlip.Vertical) != 0)
            (top, bottom) = (bottom, top);

        var corners = new QuadCorners(
            new Vec2(left, top),
            new Vec2(right, top),
            new Vec2(left, bottom),
            new Vec2(right, bottom));

        return rotation switch
        {
            SpriteBatchRotation.Clockwise90 => new QuadCorners(corners.BottomLeft, corners.TopLeft, corners.BottomRight, corners.TopRight),
            SpriteBatchRotation.Clockwise180 => new QuadCorners(corners.BottomRight, corners.BottomLeft, corners.TopRight, corners.TopLeft),
            SpriteBatchRotation.Clockwise270 => new QuadCorners(corners.TopRight, corners.BottomRight, corners.TopLeft, corners.BottomLeft),
            _ => corners
        };
    }

    /// <summary>Clips the supplied texture corners by bilinear edge interpolation.</summary>
    private static QuadCorners ClipCorners(QuadCorners corners, float leftT, float rightT, float topT, float bottomT) => new(
        Sample(corners, leftT, topT),
        Sample(corners, rightT, topT),
        Sample(corners, leftT, bottomT),
        Sample(corners, rightT, bottomT));

    /// <summary>Samples one point within texture-space quad corners.</summary>
    private static Vec2 Sample(QuadCorners corners, float x, float y)
    {
        var top = corners.TopLeft + ((corners.TopRight - corners.TopLeft) * x);
        var bottom = corners.BottomLeft + ((corners.BottomRight - corners.BottomLeft) * x);
        return top + ((bottom - top) * y);
    }

    /// <summary>Texture-space corners for a generated quad.</summary>
    private readonly struct QuadCorners(Vec2 topLeft, Vec2 topRight, Vec2 bottomLeft, Vec2 bottomRight)
    {
        /// <summary>Gets the top-left texture-space corner.</summary>
        internal Vec2 TopLeft => topLeft;

        /// <summary>Gets the top-right texture-space corner.</summary>
        internal Vec2 TopRight => topRight;

        /// <summary>Gets the bottom-left texture-space corner.</summary>
        internal Vec2 BottomLeft => bottomLeft;

        /// <summary>Gets the bottom-right texture-space corner.</summary>
        internal Vec2 BottomRight => bottomRight;
    }
}
