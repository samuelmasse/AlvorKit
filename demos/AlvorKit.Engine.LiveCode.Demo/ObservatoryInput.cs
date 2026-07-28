namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Maps ordinary engine input into direct manipulation of live colony state.</summary>
[Root]
public sealed class ObservatoryInput(
    RootKeyboard keyboard,
    RootMouse mouse,
    RootScale scale,
    ObservatoryFreeze freeze)
{
    /// <summary>Handles selection, dragging, movement, pulses, and network toggling.</summary>
    public void Update(UniverseColonies universe, Vec2 canvas, double delta)
    {
        if (keyboard.IsKeyPressed(Keys.Tab))
            universe.SelectNext();

        var selected = universe.Selected;
        if (selected is not null && keyboard.IsKeyPressed(Keys.Space))
        {
            selected.Garden.Burst(1.25f);
            universe.LastIntervention = $"Manual pulse sent to {selected.Name}.";
        }

        if (keyboard.IsKeyPressed(Keys.B))
        {
            foreach (var colony in universe.Span)
                colony.Garden.Burst(1.1f);
            universe.LastIntervention = "Manual synchronized bloom across every scope.";
        }

        if (keyboard.IsKeyPressed(Keys.L))
        {
            universe.NetworkIntensity = universe.NetworkIntensity > 0.5f ? 0.18f : 0.82f;
            universe.LastIntervention = "Constellation link intensity toggled.";
        }

        if (keyboard.IsKeyPressed(Keys.F))
        {
            freeze.Request(universe.Clock.Tick);
            universe.LastIntervention = "The next frame boundary will deliberately freeze for out-of-band inspection.";
        }

        if (mouse.IsMainPressed())
            SelectAt(universe, mouse.Position, canvas, scale.Scale);

        selected = universe.Selected;
        if (selected is null)
            return;

        if (mouse.IsMainDown() && ObservatoryLayout.IsInField(mouse.Position, canvas, scale.Scale))
            selected.Garden.Anchor = ObservatoryLayout.Anchor(mouse.Position, canvas, scale.Scale);

        if (mouse.IsSecondaryPressed())
        {
            selected.Garden.Burst(1.8f);
            universe.LastIntervention = $"Pointer burst sent to {selected.Name}.";
        }

        var motion = Motion() * (float)delta * 0.28f;
        if (motion.X != 0f || motion.Y != 0f)
            selected.Garden.Anchor = Clamp(selected.Garden.Anchor + motion);
    }

    private void SelectAt(UniverseColonies universe, Vec2 position, Vec2 canvas, float uiScale)
    {
        UniverseColony? nearest = null;
        var nearestDistanceSquared = float.MaxValue;
        foreach (var colony in universe.Span)
        {
            var center = ObservatoryLayout.Center(colony.Garden.Anchor, canvas, uiScale);
            var delta = center - position;
            var distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            var reach = (colony.Garden.OrbitRadius + 28f) * uiScale;
            if (distanceSquared > reach * reach || distanceSquared >= nearestDistanceSquared)
                continue;

            nearest = colony;
            nearestDistanceSquared = distanceSquared;
        }

        if (nearest is not null)
            universe.Select(nearest);
    }

    private Vec2 Motion() =>
        (
            Axis(Keys.Right) - Axis(Keys.Left),
            Axis(Keys.Down) - Axis(Keys.Up));

    private float Axis(Keys key) => keyboard.IsKeyDown(key) ? 1f : 0f;

    private static Vec2 Clamp(Vec2 value) =>
        (Math.Clamp(value.X, 0f, 1f), Math.Clamp(value.Y, 0f, 1f));
}
