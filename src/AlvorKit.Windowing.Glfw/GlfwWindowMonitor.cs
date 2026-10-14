namespace AlvorKit;

/// <summary>Selects the monitor containing most of a window, or the nearest monitor when it lies outside every display.</summary>
internal static class GlfwWindowMonitor
{
    /// <summary>Scans GLFW's native monitor span without allocating; a fullscreen window keeps its attached monitor.</summary>
    internal static unsafe GlfwMonitor Find(Glfw glfw, GlfwWindow window)
    {
        var attached = glfw.GetWindowMonitor(window);

        if (attached != default)
            return attached;

        var monitors = glfw.GetMonitors();

        if (monitors.IsEmpty)
            throw new InvalidOperationException("GLFW reported no monitors for the window.");

        glfw.GetWindowPos(window, out var x, out var y);
        glfw.GetWindowSize(window, out var width, out var height);
        var bounds = new Box2(new Vec2(x, y), new Vec2(x + width, y + height));
        var center = (bounds.Min + bounds.Max) / 2;
        var selected = monitors[0];
        var bestArea = -1f;
        var nearestDistance = float.PositiveInfinity;

        foreach (var monitor in monitors)
        {
            glfw.GetMonitorPos(monitor, out var mx, out var my);
            var mode = glfw.GetVideoMode(monitor);

            if (mode == null)
                throw new InvalidOperationException("GLFW could not read the monitor video mode.");

            var min = new Vec2(mx, my);
            var max = min + new Vec2(mode->Width, mode->Height);
            var overlap = Vec2.Max(Vec2.Zero, Vec2.Min(bounds.Max, max) - Vec2.Max(bounds.Min, min));
            var area = overlap.X * overlap.Y;
            var nearest = Vec2.Clamp(center, min, max);
            var distance = Vec2.DistanceSquared(center, nearest);

            if (area > bestArea || (area == bestArea && distance < nearestDistance))
            {
                selected = monitor;
                bestArea = area;
                nearestDistance = distance;
            }
        }

        return selected;
    }
}
