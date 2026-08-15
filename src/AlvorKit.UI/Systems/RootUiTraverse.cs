namespace AlvorKit;

[Root]
public class RootUiTraverse
{
    internal void Traverse(EntMut n, float? snap, int depth)
    {
        if (depth == 0)
            n.UiRoot.TraverseBufferIndex = 0;

        n.SnapR = n.AlignmentSnapFV.Resolve() ?? snap ?? 0;

        RemoveNodes(n);
        OrderNodes(n);
        CompileNodes(n);

        var innerSnap = ResolveInnerSnap(n, snap);
        foreach (var c in n.NodesR.Span)
            Traverse(c, innerSnap, depth + 1);
    }

    private static float? ResolveInnerSnap(EntMut n, float? snap)
    {
        var innerSnap = n.InnerAlignmentSnapFV.Resolve();
        if (innerSnap != null)
            return innerSnap;
        var aligned = (n.AlignmentFV.Resolve() & (Alignment.Horizontal | Alignment.Vertical)) != 0;
        if (aligned)
            return null;
        return snap;
    }

    internal bool Delay(EntMut n)
    {
        bool delay = false;
        var rem = n.RenderDelayFV.Resolve();

        if (rem > 0)
        {
            n.RenderDelayFV = rem - 1;
            delay = true;
        }

        foreach (var c in n.NodesR.Span)
        {
            if (Delay(c))
                delay = true;
        }

        return delay;
    }

    private void OrderNodes(EntMut n)
    {
        var ordered = n.IsOrderedFV.Resolve();
        if (!ordered)
            return;

        var root = n.UiRoot;
        var nodes = Nodes(n);
        if (root.OrderBufferKeys.Length <= nodes.Length)
        {
            var newSize = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)nodes.Length);
            Array.Resize(ref root.OrderBufferKeys, newSize);
            Array.Resize(ref root.OrderBufferValues, newSize);
        }

        var keys = root.OrderBufferKeys.AsSpan()[..nodes.Length];
        var vals = root.OrderBufferValues.AsSpan()[..nodes.Length];

        for (int i = 0; i < nodes.Length; i++)
        {
            keys[i] = nodes[i];
            vals[i] = nodes[i].OrderValueFV.Resolve();
        }

        vals.Sort(keys);

        for (int i = 0; i < nodes.Length; i++)
            nodes[i] = keys[i];
    }

    private void RemoveNodes(EntMut n)
    {
        for (int i = NodesCount(n) - 1; i >= 0; i--)
        {
            var c = Nodes(n)[i];

            var isDeleted = c.IsDeletedFV.Resolve();
            if (isDeleted)
                NodesRemoveAt(n, i);
        }
    }

    private void CompileNodes(EntMut n)
    {
        var root = n.UiRoot;
        int start = root.TraverseBufferIndex;
        int count = 0;

        foreach (var c in Nodes(n))
        {
            var disabled = c.IsDisabledFV.Resolve();
            if (disabled)
                continue;

            AddToBuffer(root, c);
            count++;
        }

        foreach (var entry in NodeStack(n))
        {
            var companion = entry.CompanionFV.Resolve();
            if (companion != default && !companion.IsDisabledFV.Resolve())
            {
                AddToBuffer(root, companion);
                count++;
            }
        }

        if (NodeStackTryPeek(n, out var top))
        {
            AddToBuffer(root, top);
            count++;
        }

        n.NodesR = root.TraverseBuffer.AsMemory().Slice(start, count);
    }

    private static void AddToBuffer(
        RootUi root,
        EntMut n)
    {
        if (root.TraverseBufferIndex == root.TraverseBuffer.Length)
            Array.Resize(ref root.TraverseBuffer, root.TraverseBuffer.Length * 2);
        root.TraverseBuffer[root.TraverseBufferIndex++] = n;
    }
}
