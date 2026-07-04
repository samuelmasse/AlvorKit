namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppMemoryStripGeometry
{
    public bool Intersects(AppMemoryStripView view, long index, long size) =>
        Math.Max(index, view.ViewStart) < Math.Min(index + size, view.ViewEnd);

    public Vec2 SegmentOffset(EntMut strip, AppMemoryStripView view, long index)
    {
        var clippedStart = Math.Max(index, view.ViewStart);
        var scale = strip.SizeR.X / Math.Max(1f, view.ViewEnd - view.ViewStart);
        return ((clippedStart - view.ViewStart) * scale, 0);
    }

    public Vec2 SegmentSize(EntMut strip, AppMemoryStripView view, long index, long size)
    {
        const float segmentMinimumWidth = 1.5f;

        var clippedStart = Math.Max(index, view.ViewStart);
        var clippedEnd = Math.Min(index + size, view.ViewEnd);
        if (clippedEnd <= clippedStart)
            return default;

        var scale = strip.SizeR.X / Math.Max(1f, view.ViewEnd - view.ViewStart);
        return (Math.Max(segmentMinimumWidth, (clippedEnd - clippedStart) * scale), strip.SizeR.Y);
    }
}
