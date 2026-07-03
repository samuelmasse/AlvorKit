namespace AlvorKit.UI;

[Root]
public class RootUiClipping
{
    public SpriteBatchClip IntersectClips(SpriteBatchClip? current, SpriteBatchClip next)
    {
        if (current is not SpriteBatchClip existing)
            return next;

        var min = Vec2.Max(existing.Min, next.Min);
        var max = Vec2.Min(existing.Max, next.Max);

        if (max.X < min.X || max.Y < min.Y)
            return default;

        return new(min, max);
    }

    public Box2 IntersectClips(Box2? current, Box2 next)
    {
        if (current is not Box2 existing)
            return next;

        var min = Vec2.Max(existing.Min, next.Min);
        var max = Vec2.Min(existing.Max, next.Max);

        if (max.X < min.X || max.Y < min.Y)
            return default;

        return new(min, max);
    }
}
