namespace AlvorKit;

/// <summary>Finds the deepest mouse target within a clipped UI tree.</summary>
internal sealed class UiMouseHitSearch(RootUiClipping clipping)
{
    /// <summary>Finds the hovered selectable node at <paramref name="position"/>.</summary>
    internal EntMut Hovered(Vec2 position, EntMut root) =>
        FindHovered(position, null, root, false);

    /// <summary>Finds the scrollable node at <paramref name="position"/>.</summary>
    internal EntMut Scrolled(Vec2 position, EntMut root) =>
        FindScrolled(position, null, root, false);

    private EntMut FindHovered(Vec2 position, Box2? clip, EntMut node, bool inputDisabled)
    {
        var box = clipping.IntersectClips(clip, new Box2(node.PositionR, node.PositionR + node.SizeR));
        inputDisabled |= node.IsInputDisabledFV.Resolve();

        EntMut hovered = default;
        if (!inputDisabled && box.ContainsInclusive(position) && node.IsSelectableFV.Resolve())
            hovered = node;

        foreach (var childNode in node.NodesR.Span)
        {
            var child = FindHovered(position, box, childNode, inputDisabled);
            if (child != default)
                hovered = child;
        }

        return hovered;
    }

    private EntMut FindScrolled(Vec2 position, Box2? clip, EntMut node, bool inputDisabled)
    {
        var box = clipping.IntersectClips(clip, new Box2(node.PositionR, node.PositionR + node.SizeR));
        inputDisabled |= node.IsInputDisabledFV.Resolve();

        EntMut scrolled = default;
        if (!inputDisabled && box.ContainsInclusive(position) && node.IsScrollableFV.Resolve())
            scrolled = node;

        foreach (var childNode in node.NodesR.Span)
        {
            var child = FindScrolled(position, box, childNode, inputDisabled);
            if (child != default)
                scrolled = child;
        }

        return scrolled;
    }
}
