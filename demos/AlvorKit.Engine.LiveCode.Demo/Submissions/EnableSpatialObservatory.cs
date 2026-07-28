namespace AgentSubmissions;

using AlvorKit.Engine.Loop;
using AlvorKit.Graphics2D;
using AlvorKit.Graphics2D.Fonts;

[Root]
public sealed class EnableSpatialObservatory(
    RootScope root,
    RootScripts scripts,
    UniverseColonies universe) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        var overlayPresent = false;
        foreach (var script in scripts.Span)
        {
            if (script.GetType().FullName == typeof(SpatialObservatoryOverlay).FullName)
                overlayPresent = true;
        }

        if (!overlayPresent)
            scripts.Add(root.New<SpatialObservatoryOverlay>());

        universe.NetworkColor = (0.16f, 0.92f, 1f, 0.92f);
        universe.NetworkIntensity = 1f;
        universe.LastIntervention =
            "LiveCode projected every active scope into a rotating three-dimensional observatory.";
        foreach (var colony in universe.Span)
            colony.Garden.Burst(2.4f);

        output.WriteLine("Attached a perspective-projected 3D observatory to the running root script list.");
        output.Value("overlayAdded", !overlayPresent);
        output.Value("activeColonies", universe.Span.Length);
        output.Value("projection", "rotating perspective camera");
        output.Value("depthSorting", true);
        output.Value("restartRequired", false);
    }
}

[Root]
public sealed class SpatialObservatoryOverlay(
    RootCanvas canvas,
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale,
    UniverseColonies universe) : Script
{
    private const int MaximumColonies = 16;
    private const int RingSegments = 36;
    private const float CameraDistance = 6.4f;
    private readonly SpatialNode[] nodes = new SpatialNode[MaximumColonies];
    private readonly int[] drawOrder = new int[MaximumColonies];
    private double time;
    private Vec2 viewCenter;
    private float focalLength;
    private float cosYaw;
    private float sinYaw;
    private float cosPitch;
    private float sinPitch;

    public override float Order => 9_000f;

    public override void Update(double delta) => time += delta;

    public override void Draw()
    {
        var size = canvas.Size;
        var ui = scale.Scale;
        var fieldWidth = Math.Max(1f, size.X - ObservatoryLayout.SidebarWidth(ui));
        ConfigureCamera(fieldWidth, size.Y);

        DrawBackdrop(fieldWidth, size.Y, ui);
        DrawFloorGrid(ui);
        DrawSpatialCage(ui);

        var count = BuildNodes();
        DrawNetwork(count, ui);
        DrawSun(ui);
        SortNodes(count);
        for (var index = 0; index < count; index++)
            DrawColony(nodes[drawOrder[index]], ui);

        DrawHud(fieldWidth, ui, count);
    }

    private void ConfigureCamera(float width, float height)
    {
        viewCenter = (width * 0.5f, height * 0.54f);
        focalLength = Math.Min(width, height) * 0.82f;
        var yaw = (float)time * 0.18f;
        var pitch = 0.34f + MathF.Sin((float)time * 0.11f) * 0.08f;
        cosYaw = MathF.Cos(yaw);
        sinYaw = MathF.Sin(yaw);
        cosPitch = MathF.Cos(pitch);
        sinPitch = MathF.Sin(pitch);
    }

    private void DrawBackdrop(float width, float height, float ui)
    {
        sprites.Batch.Draw((0f, 0f), (width, height), (0.004f, 0.008f, 0.024f, 0.985f));
        sprites.Batch.Draw(
            (0f, height * 0.42f),
            (width, height * 0.58f),
            (0.035f, 0.008f, 0.085f, 0.76f));
        sprites.Batch.Draw(
            (0f, height * 0.535f),
            (width, 2f * ui),
            (0.18f, 0.86f, 1f, 0.22f));

        for (var index = 0; index < 46; index++)
        {
            var x = ((index * 97) % 997) / 997f * width;
            var y = ((index * 193) % 991) / 991f * height;
            var shimmer = 0.3f + MathF.Sin((float)time * 1.4f + index * 0.71f) * 0.2f;
            var starSize = (1f + index % 3) * ui;
            sprites.Batch.Draw((x, y), (starSize, starSize), (0.58f, 0.82f, 1f, shimmer));
        }
    }

    private void DrawFloorGrid(float ui)
    {
        for (var index = -5; index <= 5; index++)
        {
            var coordinate = index * 0.7f;
            var major = index == 0;
            Vec4 color = major
                ? (0.2f, 0.9f, 1f, 0.38f)
                : (0.18f, 0.42f, 0.78f, 0.18f);
            var width = (major ? 1.4f : 0.7f) * ui;
            DrawWorldLine(
                (-3.5f, -1.5f, coordinate),
                (3.5f, -1.5f, coordinate),
                width,
                color);
            DrawWorldLine(
                (coordinate, -1.5f, -3.5f),
                (coordinate, -1.5f, 3.5f),
                width,
                color);
        }
    }

    private void DrawSpatialCage(float ui)
    {
        var quiet = (Vec4)(0.24f, 0.48f, 0.92f, 0.17f);
        DrawWorldBox((-3.1f, -1.5f, -2.2f), (3.1f, 1.55f, 2.2f), quiet, 0.75f * ui);

        DrawWorldLine((0f, -1.5f, 0f), (3.5f, -1.5f, 0f), 1.4f * ui, (1f, 0.25f, 0.55f, 0.62f));
        DrawWorldLine((0f, -1.5f, 0f), (0f, 1.9f, 0f), 1.4f * ui, (0.3f, 1f, 0.62f, 0.62f));
        DrawWorldLine((0f, -1.5f, 0f), (0f, -1.5f, 3f), 1.4f * ui, (0.25f, 0.72f, 1f, 0.72f));

        WriteWorldLabel("X", (3.65f, -1.5f, 0f), (1f, 0.3f, 0.58f, 0.9f), ui);
        WriteWorldLabel("Y", (0f, 2.05f, 0f), (0.32f, 1f, 0.66f, 0.9f), ui);
        WriteWorldLabel("Z", (0f, -1.5f, 3.15f), (0.3f, 0.76f, 1f, 0.95f), ui);
    }

    private int BuildNodes()
    {
        var count = 0;
        foreach (var colony in universe.Span)
        {
            if (count == MaximumColonies)
                break;

            var garden = colony.Garden;
            var depthPhase = (float)garden.SolarAngle * 1.25f + colony.Id.Value * 1.73f;
            Vec3 world =
            (
                (garden.Anchor.X - 0.5f) * 5.4f,
                (0.5f - garden.Anchor.Y) * 3.8f + MathF.Cos(depthPhase * 0.61f) * 0.16f,
                MathF.Sin(depthPhase) * 1.55f
            );
            var projected = Project(world);
            nodes[count] = new(colony, world, projected.Screen, projected.Depth, projected.Scale);
            drawOrder[count] = count;
            count++;
        }

        return count;
    }

    private void SortNodes(int count)
    {
        for (var index = 1; index < count; index++)
        {
            var value = drawOrder[index];
            var previous = index - 1;
            while (previous >= 0 && nodes[drawOrder[previous]].Depth < nodes[value].Depth)
            {
                drawOrder[previous + 1] = drawOrder[previous];
                previous--;
            }
            drawOrder[previous + 1] = value;
        }
    }

    private void DrawNetwork(int count, float ui)
    {
        for (var first = 0; first < count; first++)
        {
            for (var second = first + 1; second < count; second++)
            {
                var start = nodes[first].World;
                var end = nodes[second].World;
                var previous = Project(start).Screen;
                for (var segment = 1; segment <= 18; segment++)
                {
                    var amount = segment / 18f;
                    var arc = MathF.Sin(amount * MathF.PI);
                    Vec3 world =
                    (
                        start.X + (end.X - start.X) * amount,
                        start.Y + (end.Y - start.Y) * amount + arc * 0.46f,
                        start.Z + (end.Z - start.Z) * amount + arc * 0.18f
                    );
                    var current = Project(world).Screen;
                    sprites.Batch.DrawLine(
                        previous,
                        current,
                        1.2f * ui,
                        Alpha(universe.NetworkColor, 0.46f));
                    previous = current;
                }

                var pulseAmount =
                    (MathF.Sin((float)time * 1.15f + first * 1.9f + second * 0.8f) + 1f) * 0.5f;
                Vec3 pulseWorld =
                (
                    start.X + (end.X - start.X) * pulseAmount,
                    start.Y + (end.Y - start.Y) * pulseAmount + MathF.Sin(pulseAmount * MathF.PI) * 0.46f,
                    start.Z + (end.Z - start.Z) * pulseAmount + MathF.Sin(pulseAmount * MathF.PI) * 0.18f
                );
                var pulse = Project(pulseWorld);
                DrawGlowSquare(
                    pulse.Screen,
                    Math.Clamp(pulse.Scale * 0.06f, 5f * ui, 13f * ui),
                    (0.28f, 1f, 0.92f, 0.86f));
            }
        }
    }

    private void DrawSun(float ui)
    {
        Vec3 center = (0f, 0f, 0f);
        DrawWorldRing(center, 0.52f, 0, (1f, 0.45f, 0.08f, 0.72f), 2.1f * ui);
        DrawWorldRing(center, 0.52f, 1, (1f, 0.72f, 0.12f, 0.62f), 1.6f * ui);
        DrawWorldRing(center, 0.52f, 2, (1f, 0.28f, 0.08f, 0.62f), 1.6f * ui);

        var projected = Project(center);
        var pulse = 1f + MathF.Sin((float)time * 2.2f) * 0.08f;
        var size = Math.Clamp(projected.Scale * 0.28f * pulse, 24f * ui, 62f * ui);
        DrawGlowSquare(projected.Screen, size * 2.2f, (1f, 0.18f, 0.04f, 0.12f));
        DrawGlowSquare(projected.Screen, size * 1.45f, (1f, 0.52f, 0.08f, 0.34f));
        DrawGlowSquare(projected.Screen, size, (1f, 0.92f, 0.38f, 0.96f));
    }

    private void DrawColony(SpatialNode node, float ui)
    {
        var colony = node.Colony!;
        var garden = colony.Garden;
        var selected = ReferenceEquals(colony, universe.Selected);
        var ringRadius = 0.24f + garden.OrbitRadius / 310f;
        var depthLight = Math.Clamp((8.7f - node.Depth) / 4.2f, 0.34f, 1f);
        var primary = Alpha(garden.Primary, 0.76f * depthLight);
        var secondary = Alpha(garden.Secondary, 0.82f * depthLight);

        DrawWorldRing(node.World, ringRadius, 0, primary, 1.4f * ui);
        DrawWorldRing(node.World, ringRadius, 1, secondary, 1.8f * ui);
        DrawWorldRing(node.World, ringRadius, 2, Alpha(colony.Sky.Halo, 0.58f), 1.1f * ui);
        if (selected)
        {
            DrawWorldRing(
                node.World,
                ringRadius * 1.22f + MathF.Sin((float)time * 3f) * 0.025f,
                1,
                (1f, 0.88f, 0.24f, 0.86f),
                2.5f * ui);
        }

        DrawSpores(node, ringRadius, ui);

        var coreSize = Math.Clamp(
            node.ProjectionScale * (0.14f + garden.Radius / 520f),
            18f * ui,
            58f * ui);
        DrawGlowSquare(node.Screen, coreSize * 2.1f, Alpha(primary, 0.16f));
        DrawGlowSquare(node.Screen, coreSize * 1.4f, Alpha(primary, 0.38f));
        DrawGlowSquare(node.Screen, coreSize, Alpha(secondary, 0.96f));

        var labelFont = roboto[scale[14]];
        var detailFont = roboto[scale[11]];
        var label = text.Format("{0}  /  DEPTH {1:0.00}", colony.Name, node.World.Z);
        var labelWidth = sprites.Batch.Measure(labelFont, label);
        var labelY = node.Screen.Y + ringRadius * node.ProjectionScale + 19f * ui;
        sprites.Batch.Write(
            labelFont,
            label,
            (node.Screen.X - labelWidth * 0.5f, labelY),
            (0.9f, 0.97f, 1f, 1f));

        var detail = text.Format("{0}  /  {1} organisms", garden.Form, garden.SporeCount);
        var detailWidth = sprites.Batch.Measure(detailFont, detail);
        sprites.Batch.Write(
            detailFont,
            detail,
            (node.Screen.X - detailWidth * 0.5f, labelY + 20f * ui),
            Alpha(primary, 0.92f));
    }

    private void DrawSpores(SpatialNode node, float radius, float ui)
    {
        var colony = node.Colony!;
        var count = Math.Clamp(colony.Garden.SporeCount, 14, 52);
        var phase = (float)time * 0.72f + (float)colony.Garden.Phase * 0.18f;
        for (var index = 0; index < count; index++)
        {
            var vertical = 1f - 2f * (index + 0.5f) / count;
            var horizontal = MathF.Sqrt(Math.Max(0f, 1f - vertical * vertical));
            var angle = index * 2.3999632f + phase;
            Vec3 world =
            (
                node.World.X + MathF.Cos(angle) * horizontal * radius,
                node.World.Y + vertical * radius,
                node.World.Z + MathF.Sin(angle) * horizontal * radius
            );
            var projected = Project(world);
            var dotSize = Math.Clamp(projected.Scale * 0.035f, 2.5f * ui, 7.5f * ui);
            var color = index % 2 == 0 ? colony.Garden.Primary : colony.Garden.Secondary;
            if (index % 8 == 0)
            {
                sprites.Batch.DrawLine(
                    node.Screen,
                    projected.Screen,
                    0.55f * ui,
                    Alpha(color, 0.15f));
            }
            DrawGlowSquare(projected.Screen, dotSize, Alpha(color, 0.9f));
        }
    }

    private void DrawHud(float fieldWidth, float ui, int count)
    {
        sprites.Batch.Draw((0f, 0f), (fieldWidth, 88f * ui), (0.006f, 0.018f, 0.05f, 0.92f));
        sprites.Batch.Draw((0f, 86f * ui), (fieldWidth, 2f * ui), (0.16f, 0.9f, 1f, 0.5f));
        sprites.Batch.Write(
            roboto[scale[24]],
            "MYCELIAL SCOPE OBSERVATORY  /  SPATIAL MODE",
            (27f * ui, 18f * ui),
            (0.9f, 0.98f, 1f, 1f));
        sprites.Batch.Write(
            roboto[scale[12]],
            text.Format(
                "perspective camera  /  depth-sorted scopes {0}  /  live frame {1}",
                count,
                universe.Clock.Tick),
            (30f * ui, 54f * ui),
            (0.32f, 0.92f, 1f, 0.9f));
        sprites.Batch.Write(
            roboto[scale[10]],
            "X/Y/Z cage + projected orbital spheres + live injector relationships",
            (fieldWidth - 490f * ui, 57f * ui),
            (0.62f, 0.72f, 0.9f, 0.88f));
    }

    private void DrawWorldBox(Vec3 minimum, Vec3 maximum, Vec4 color, float width)
    {
        DrawWorldLine((minimum.X, minimum.Y, minimum.Z), (maximum.X, minimum.Y, minimum.Z), width, color);
        DrawWorldLine((minimum.X, maximum.Y, minimum.Z), (maximum.X, maximum.Y, minimum.Z), width, color);
        DrawWorldLine((minimum.X, minimum.Y, maximum.Z), (maximum.X, minimum.Y, maximum.Z), width, color);
        DrawWorldLine((minimum.X, maximum.Y, maximum.Z), (maximum.X, maximum.Y, maximum.Z), width, color);

        DrawWorldLine((minimum.X, minimum.Y, minimum.Z), (minimum.X, maximum.Y, minimum.Z), width, color);
        DrawWorldLine((maximum.X, minimum.Y, minimum.Z), (maximum.X, maximum.Y, minimum.Z), width, color);
        DrawWorldLine((minimum.X, minimum.Y, maximum.Z), (minimum.X, maximum.Y, maximum.Z), width, color);
        DrawWorldLine((maximum.X, minimum.Y, maximum.Z), (maximum.X, maximum.Y, maximum.Z), width, color);

        DrawWorldLine((minimum.X, minimum.Y, minimum.Z), (minimum.X, minimum.Y, maximum.Z), width, color);
        DrawWorldLine((maximum.X, minimum.Y, minimum.Z), (maximum.X, minimum.Y, maximum.Z), width, color);
        DrawWorldLine((minimum.X, maximum.Y, minimum.Z), (minimum.X, maximum.Y, maximum.Z), width, color);
        DrawWorldLine((maximum.X, maximum.Y, minimum.Z), (maximum.X, maximum.Y, maximum.Z), width, color);
    }

    private void DrawWorldRing(Vec3 center, float radius, int plane, Vec4 color, float width)
    {
        var previous = Project(RingPoint(center, radius, plane, 0f)).Screen;
        for (var segment = 1; segment <= RingSegments; segment++)
        {
            var angle = segment / (float)RingSegments * MathF.Tau;
            var current = Project(RingPoint(center, radius, plane, angle)).Screen;
            sprites.Batch.DrawLine(previous, current, width, color);
            previous = current;
        }
    }

    private static Vec3 RingPoint(Vec3 center, float radius, int plane, float angle)
    {
        var horizontal = MathF.Cos(angle) * radius;
        var vertical = MathF.Sin(angle) * radius;
        return plane switch
        {
            0 => (center.X + horizontal, center.Y + vertical, center.Z),
            1 => (center.X + horizontal, center.Y, center.Z + vertical),
            _ => (center.X, center.Y + horizontal, center.Z + vertical)
        };
    }

    private void DrawWorldLine(Vec3 start, Vec3 end, float width, Vec4 color)
    {
        var projectedStart = Project(start);
        var projectedEnd = Project(end);
        sprites.Batch.DrawLine(projectedStart.Screen, projectedEnd.Screen, width, color);
    }

    private void WriteWorldLabel(string value, Vec3 world, Vec4 color, float ui)
    {
        var projected = Project(world);
        sprites.Batch.Write(roboto[scale[13]], value, projected.Screen + (4f * ui, -8f * ui), color);
    }

    private SpatialPoint Project(Vec3 world)
    {
        var rotatedX = world.X * cosYaw - world.Z * sinYaw;
        var rotatedZ = world.X * sinYaw + world.Z * cosYaw;
        var pitchedY = world.Y * cosPitch - rotatedZ * sinPitch;
        var pitchedZ = world.Y * sinPitch + rotatedZ * cosPitch;
        var depth = Math.Max(1.25f, CameraDistance + pitchedZ);
        var projectionScale = focalLength / depth;
        return new(
            (
                viewCenter.X + rotatedX * projectionScale,
                viewCenter.Y - pitchedY * projectionScale
            ),
            depth,
            projectionScale);
    }

    private void DrawGlowSquare(Vec2 center, float size, Vec4 color) =>
        sprites.Batch.Draw(center - (size * 0.5f, size * 0.5f), (size, size), color);

    private static Vec4 Alpha(Vec4 color, float alpha) =>
        (color.X, color.Y, color.Z, Math.Clamp(alpha, 0f, 1f));
}

internal readonly record struct SpatialPoint(
    Vec2 Screen,
    float Depth,
    float Scale);

internal readonly record struct SpatialNode(
    UniverseColony? Colony,
    Vec3 World,
    Vec2 Screen,
    float Depth,
    float ProjectionScale);
