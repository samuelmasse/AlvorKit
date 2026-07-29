namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Renders colonies, their injector relationships, and the live execution endpoint.</summary>
[Root]
public sealed class ObservatoryRenderer(
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale,
    ObservatoryPanelRenderer panel)
{
    private const int CircleSegments = 40;
    private static readonly Vec4 QuietText = (0.58f, 0.66f, 0.8f, 1f);
    private static readonly Vec4 BrightText = (0.91f, 0.95f, 1f, 1f);
    private readonly Star[] stars = CreateStars();

    /// <summary>Draws one complete observatory frame without allocating collections.</summary>
    public void Draw(
        UniverseColonies universe,
        InjectorScopeGraphSnapshot graph,
        LiveCodeSessionManifest session,
        Vec2 canvas)
    {
        DrawBackdrop(canvas, universe.Clock.Time);
        DrawSun(canvas, universe.Clock.Time);
        DrawNetwork(universe, canvas);

        foreach (var colony in universe.Span)
            DrawColony(colony, ReferenceEquals(colony, universe.Selected), canvas, universe.Clock.Time);

        DrawTitle(universe, canvas);
        panel.Draw(universe, graph, session, canvas);
    }

    private void DrawSun(Vec2 canvas, double time)
    {
        var center = ObservatoryLayout.Center((0.5f, 0.5f), canvas, scale.Scale);
        var pulse = 1f + MathF.Sin((float)time * 1.7f) * 0.07f;
        var radius = S(34f) * pulse;
        DrawCircle(center, S(64f), (1f, 0.42f, 0.08f, 0.12f), S(8f), 48);
        DrawCircle(center, S(48f), (1f, 0.72f, 0.12f, 0.28f), S(5f), 48);
        DrawGlowSquare(center, radius * 2.1f, (1f, 0.24f, 0.04f, 0.15f));
        DrawGlowSquare(center, radius * 1.45f, (1f, 0.58f, 0.08f, 0.35f));
        DrawGlowSquare(center, radius, (1f, 0.9f, 0.32f, 0.95f));
        for (var ray = 0; ray < 12; ray++)
        {
            var angle = ray / 12f * MathF.Tau + (float)time * 0.08f;
            Vec2 from = center +
                (MathF.Cos(angle) * radius * 0.8f, MathF.Sin(angle) * radius * 0.8f);
            Vec2 to = center +
                (MathF.Cos(angle) * radius * 1.75f, MathF.Sin(angle) * radius * 1.75f);
            sprites.Batch.DrawLine(from, to, S(2f), (1f, 0.58f, 0.12f, 0.46f));
        }
    }

    private void DrawBackdrop(Vec2 canvas, double time)
    {
        var fieldWidth = Math.Max(0f, canvas.X - ObservatoryLayout.SidebarWidth(scale.Scale));
        sprites.Batch.Draw((0f, 0f), (fieldWidth, canvas.Y), (0.012f, 0.018f, 0.045f, 1f));
        sprites.Batch.Draw((0f, canvas.Y * 0.48f), (fieldWidth, canvas.Y * 0.52f), (0.025f, 0.018f, 0.075f, 0.5f));

        for (var x = 0f; x < fieldWidth; x += S(80f))
            sprites.Batch.Draw((x, 0f), (S(1f), canvas.Y), (0.16f, 0.25f, 0.48f, 0.08f));
        for (var y = 0f; y < canvas.Y; y += S(80f))
            sprites.Batch.Draw((0f, y), (fieldWidth, S(1f)), (0.16f, 0.25f, 0.48f, 0.08f));

        foreach (var star in stars)
        {
            var twinkle = 0.45f + MathF.Sin((float)time * star.Speed + star.Phase) * 0.28f;
            var position = (star.Position.X * fieldWidth, star.Position.Y * canvas.Y);
            sprites.Batch.Draw(position, (S(star.Size), S(star.Size)), (0.62f, 0.8f, 1f, twinkle));
        }
    }

    private void DrawNetwork(UniverseColonies universe, Vec2 canvas)
    {
        var colonies = universe.Span;
        for (var first = 0; first < colonies.Length; first++)
        {
            var start = ObservatoryLayout.Center(colonies[first].Garden.Anchor, canvas, scale.Scale);
            for (var second = first + 1; second < colonies.Length; second++)
            {
                var end = ObservatoryLayout.Center(colonies[second].Garden.Anchor, canvas, scale.Scale);
                var alpha = universe.NetworkIntensity * 0.34f;
                sprites.Batch.DrawLine(start, end, S(1.5f), Alpha(universe.NetworkColor, alpha));

                var wave = (MathF.Sin(
                    (float)universe.Clock.Time * 0.8f
                    + first * 2.1f
                    + second) + 1f) * 0.5f;
                var pulse = start + (end - start) * wave;
                DrawGlowSquare(pulse, S(9f), Alpha(universe.NetworkColor, 0.82f));
            }
        }
    }

    private void DrawColony(UniverseColony colony, bool selected, Vec2 canvas, double time)
    {
        var garden = colony.Garden;
        var sky = colony.Sky;
        var center = ObservatoryLayout.Center(garden.Anchor, canvas, scale.Scale);
        var pulse = 1f + garden.Bloom * 0.15f + MathF.Sin((float)time * 2f + (float)garden.Phase) * 0.035f;
        var coreRadius = S(garden.Radius) * pulse;

        if (selected)
        {
            var selectionRadius = S(garden.OrbitRadius + 30f) + MathF.Sin((float)time * 3f) * S(4f);
            DrawCircle(center, selectionRadius, Alpha(garden.Secondary, 0.62f), S(2.5f), 52);
        }

        DrawCircle(center, S(garden.OrbitRadius), Alpha(sky.Halo, 0.3f), S(1.5f), CircleSegments);
        DrawCircle(center, coreRadius * 1.28f, Alpha(garden.Primary, 0.22f), S(5f), CircleSegments);
        DrawCircle(center, coreRadius, Alpha(garden.Primary, 0.84f), S(3f), CircleSegments);
        DrawCore(center, coreRadius, garden.Primary, garden.Secondary);
        DrawSpores(colony, center, time);

        var font = roboto[scale[15]];
        var label = text.Format("{0}  [{1}]", colony.Name, colony.Id.Value);
        var width = sprites.Batch.Measure(font, label);
        sprites.Batch.Write(font, label, center + (-width * 0.5f, S(garden.OrbitRadius + 35f)), BrightText);

        var detail = text.Format("{0} / {1} organisms", garden.Form, garden.SporeCount);
        var detailWidth = sprites.Batch.Measure(font, detail);
        sprites.Batch.Write(font, detail, center + (-detailWidth * 0.5f, S(garden.OrbitRadius + 55f)), QuietText);
    }

    private void DrawCore(Vec2 center, float radius, Vec4 primary, Vec4 secondary)
    {
        var coreSize = Math.Max(S(12f), radius * 0.42f);
        DrawGlowSquare(center, coreSize * 2.2f, Alpha(primary, 0.16f));
        DrawGlowSquare(center, coreSize * 1.4f, Alpha(primary, 0.42f));
        DrawGlowSquare(center, coreSize, Alpha(secondary, 0.92f));

        Vec2 up = (0f, -radius * 0.72f);
        Vec2 right = (radius * 0.72f, 0f);
        sprites.Batch.DrawLine(center + up, center + right, S(2f), Alpha(secondary, 0.8f));
        sprites.Batch.DrawLine(center + right, center - up, S(2f), Alpha(secondary, 0.8f));
        sprites.Batch.DrawLine(center - up, center - right, S(2f), Alpha(secondary, 0.8f));
        sprites.Batch.DrawLine(center - right, center + up, S(2f), Alpha(secondary, 0.8f));
    }

    private void DrawSpores(UniverseColony colony, Vec2 center, double time)
    {
        var garden = colony.Garden;
        var count = Math.Clamp(garden.SporeCount, 4, 64);
        for (var index = 0; index < count; index++)
        {
            var fraction = index / (float)count;
            var angle = (float)garden.Phase + fraction * MathF.Tau;
            var warp = MathF.Sin(angle * 3f + (float)time) * colony.Sky.Warp * garden.OrbitRadius;
            var radius = S(garden.OrbitRadius + warp * 0.22f);
            Vec2 offset = (MathF.Cos(angle) * radius, MathF.Sin(angle) * radius * 0.62f);
            var position = center + offset;
            var size = S(3.5f + (index % 5) * 0.7f + garden.Bloom * 1.5f);
            var color = index % 2 == 0 ? garden.Primary : garden.Secondary;

            if (index % 6 == 0)
                sprites.Batch.DrawLine(center, position, S(0.8f), Alpha(color, 0.16f));
            DrawGlowSquare(position, size, Alpha(color, 0.88f));
        }
    }

    private void DrawTitle(UniverseColonies universe, Vec2 canvas)
    {
        var title = roboto[scale[25]];
        sprites.Batch.Write(title, "MYCELIAL SCOPE OBSERVATORY", (S(28f), S(22f)), BrightText);
        var subtitle = roboto[scale[14]];
        sprites.Batch.Write(
            subtitle,
            text.Format("engine frame {0}  /  {1} active injector scopes", universe.Clock.Tick, universe.Span.Length),
            (S(31f), S(54f)),
            QuietText);

        var fieldWidth = canvas.X - ObservatoryLayout.SidebarWidth(scale.Scale);
        sprites.Batch.Draw(
            (S(28f), S(78f)),
            (Math.Max(0f, fieldWidth - S(56f)), S(2f)),
            (0.25f, 0.6f, 1f, 0.28f));
    }

    private void DrawCircle(Vec2 center, float radius, Vec4 color, float width, int segments)
    {
        Vec2 previous = center + (radius, 0f);
        for (var index = 1; index <= segments; index++)
        {
            var angle = index / (float)segments * MathF.Tau;
            Vec2 current = center + (MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
            sprites.Batch.DrawLine(previous, current, width, color);
            previous = current;
        }
    }

    private void DrawGlowSquare(Vec2 center, float size, Vec4 color) =>
        sprites.Batch.Draw(center - (size * 0.5f, size * 0.5f), (size, size), color);

    private float S(float value) => value * scale.Scale;

    private static Vec4 Alpha(Vec4 color, float alpha) =>
        (color.X, color.Y, color.Z, Math.Clamp(alpha, 0f, 1f));

    private static Star[] CreateStars()
    {
        var result = new Star[72];
        var random = new Random(481516);
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = new(
                ((float)random.NextDouble(), (float)random.NextDouble()),
                1f + (float)random.NextDouble() * 2.5f,
                0.5f + (float)random.NextDouble() * 1.8f,
                (float)random.NextDouble() * MathF.Tau);
        }

        return result;
    }

    private readonly record struct Star(Vec2 Position, float Size, float Speed, float Phase);
}
