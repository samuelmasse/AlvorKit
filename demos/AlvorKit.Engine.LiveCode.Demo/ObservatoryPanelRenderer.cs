namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Draws the live endpoint, injector graph, selection, and interaction HUD.</summary>
[Root]
public sealed class ObservatoryPanelRenderer(
    RootSprites sprites,
    RootRoboto roboto,
    RootText text,
    RootScale scale)
{
    private static readonly Vec4 PanelColor = (0.025f, 0.035f, 0.08f, 0.96f);
    private static readonly Vec4 QuietText = (0.58f, 0.66f, 0.8f, 1f);
    private static readonly Vec4 BrightText = (0.91f, 0.95f, 1f, 1f);

    /// <summary>Draws a current point-in-time view of the scope graph.</summary>
    public void Draw(
        UniverseColonies universe,
        InjectorScopeGraphSnapshot graph,
        LiveCodeSessionManifest session,
        Vec2 canvas)
    {
        var sidebarWidth = ObservatoryLayout.SidebarWidth(scale.Scale);
        var left = canvas.X - sidebarWidth;
        sprites.Batch.Draw((left, 0f), (sidebarWidth, canvas.Y), PanelColor);
        sprites.Batch.Draw((left, 0f), (S(3f), canvas.Y), (0.3f, 0.85f, 1f, 0.8f));

        var x = left + S(24f);
        sprites.Batch.Write(roboto[scale[20]], "LIVE C# PORTAL", (x, S(24f)), BrightText);
        sprites.Batch.Write(
            roboto[scale[13]],
            text.Format("{0}  ·  loopback:{1}", session.Name, session.Port),
            (x, S(52f)),
            (0.28f, 1f, 0.7f, 1f));
        sprites.Batch.Write(
            roboto[scale[13]],
            text.Format("scope graph revision {0}", graph.Revision),
            (x, S(75f)),
            QuietText);

        var y = S(108f);
        var count = Math.Min(graph.Nodes.Length, 8);
        for (var index = 0; index < count; index++)
        {
            DrawScopeCard(universe, graph.Nodes[index], x, y);
            y += S(51f);
        }

        DrawFooter(universe, canvas, x, y + S(18f));
    }

    private void DrawScopeCard(
        UniverseColonies universe,
        InjectorScopeGraphNodeSnapshot node,
        float x,
        float y)
    {
        var selected = universe.Selected?.Id == node.Id;
        var ended = node.Lifecycle == InjectorScopeLifecycle.Ended;
        var accent = ended
            ? (Vec4)(0.45f, 0.47f, 0.55f, 0.8f)
            : selected ? universe.Selected!.Garden.Primary : (0.18f, 0.58f, 0.92f, 0.85f);
        sprites.Batch.Draw((x, y), (S(292f), S(43f)), Alpha(accent, selected ? 0.22f : 0.1f));
        sprites.Batch.Draw((x, y), (S(4f), S(43f)), accent);
        sprites.Batch.Write(
            roboto[scale[14]],
            text.Format("#{0}  {1}", node.Id.Value, node.Label ?? "unlabeled scope"),
            (x + S(13f), y + S(7f)),
            ended ? QuietText : BrightText);
        sprites.Batch.Write(
            roboto[scale[11]],
            ended ? "ENDED · tombstone retained" : node.ParentId is null ? "ACTIVE · engine root" : "ACTIVE · exact executor",
            (x + S(13f), y + S(26f)),
            ended ? (0.7f, 0.5f, 0.56f, 1f) : (0.33f, 0.9f, 0.72f, 1f));
    }

    private void DrawFooter(UniverseColonies universe, Vec2 canvas, float x, float graphBottom)
    {
        var selected = universe.Selected;
        var footerY = Math.Max(graphBottom + S(12f), canvas.Y - S(190f));
        sprites.Batch.Write(roboto[scale[13]], "SELECTED EXECUTOR", (x, footerY), QuietText);
        sprites.Batch.Write(
            roboto[scale[17]],
            selected?.Name ?? "(none)",
            (x, footerY + S(22f)),
            selected?.Garden.Primary ?? BrightText);
        sprites.Batch.Write(roboto[scale[13]], "LAST INTERVENTION", (x, footerY + S(54f)), QuietText);
        WriteIntervention(universe.LastIntervention, x, footerY + S(75f));

        sprites.Batch.Write(
            roboto[scale[10]],
            "Tab select  ·  drag move  ·  right-click pulse",
            (x, canvas.Y - S(49f)),
            QuietText);
        sprites.Batch.Write(
            roboto[scale[10]],
            "Arrows move  ·  Space pulse  ·  B bloom  ·  L links  ·  F freeze",
            (x, canvas.Y - S(30f)),
            QuietText);
    }

    private void WriteIntervention(string value, float x, float y)
    {
        const int lineLength = 42;
        var span = value.AsSpan();
        var firstLength = Math.Min(lineLength, span.Length);
        sprites.Batch.Write(roboto[scale[11]], span[..firstLength], (x, y), BrightText);
        if (span.Length > firstLength)
        {
            var secondLength = Math.Min(lineLength, span.Length - firstLength);
            sprites.Batch.Write(
                roboto[scale[11]],
                span.Slice(firstLength, secondLength),
                (x, y + S(17f)),
                BrightText);
        }
    }

    private float S(float value) => value * scale.Scale;

    private static Vec4 Alpha(Vec4 color, float alpha) =>
        (color.X, color.Y, color.Z, Math.Clamp(alpha, 0f, 1f));
}
