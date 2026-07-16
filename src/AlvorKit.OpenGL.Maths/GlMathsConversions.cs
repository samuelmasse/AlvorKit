namespace AlvorKit.OpenGL;

internal static class GlMathsConversions
{
    internal static int ToSize(uint value) => checked((int)value);

    internal static Vec2i ToSize(Vec2u value) =>
        (checked((int)value.X), checked((int)value.Y));

    internal static Vec3i ToSize(Vec3u value) =>
        (checked((int)value.X), checked((int)value.Y), checked((int)value.Z));

    internal static Vec2i ToEnd(Vec2i origin, Vec2u size)
    {
        var x = (long)origin.X + size.X;
        var y = (long)origin.Y + size.Y;
        return (checked((int)x), checked((int)y));
    }
}
