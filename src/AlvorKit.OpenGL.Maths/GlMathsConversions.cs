namespace AlvorKit;

internal static class GlMathsConversions
{
    internal static int ToSize(uint value) => Convert.ToInt32(value);

    internal static Vec2i ToSize(Vec2u value) =>
        (Convert.ToInt32(value.X), Convert.ToInt32(value.Y));

    internal static Vec3i ToSize(Vec3u value) =>
        (Convert.ToInt32(value.X), Convert.ToInt32(value.Y), Convert.ToInt32(value.Z));

    internal static Vec2i ToEnd(Vec2i origin, Vec2u size)
    {
        var x = (long)origin.X + size.X;
        var y = (long)origin.Y + size.Y;
        return (Convert.ToInt32(x), Convert.ToInt32(y));
    }
}
