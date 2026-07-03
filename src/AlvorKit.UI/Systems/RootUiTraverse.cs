namespace AlvorKit.UI;

[Root]
public class RootUiTraverse
{
    private EntMut[] traverseBuffer = new EntMut[16];
    private int traverseBufferIndex;

    private EntMut[] orderBufferKeys = new EntMut[16];
    private float[] orderBufferVals = new float[16];

    internal void Traverse(EntMut n, float? snap, int depth)
    {
        if (depth == 0)
            traverseBufferIndex = 0;

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

        var nodes = Nodes(n);
        if (orderBufferKeys.Length <= nodes.Length)
        {
            var newSize = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)nodes.Length);
            Array.Resize(ref orderBufferKeys, newSize);
            Array.Resize(ref orderBufferVals, newSize);
        }

        var keys = orderBufferKeys.AsSpan()[..nodes.Length];
        var vals = orderBufferVals.AsSpan()[..nodes.Length];

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
        int start = traverseBufferIndex;
        int count = 0;

        foreach (var c in Nodes(n))
        {
            var disabled = c.IsDisabledFV.Resolve();
            if (disabled)
                continue;

            AddToBuffer(c);
            count++;
        }

        foreach (var entry in NodeStack(n))
        {
            var companion = entry.CompanionFV.Resolve();
            if (companion != default && !companion.IsDisabledFV.Resolve())
            {
                AddToBuffer(companion);
                count++;
            }
        }

        if (NodeStackTryPeek(n, out var top))
        {
            AddToBuffer(top);
            count++;
        }

        n.NodesR = traverseBuffer.AsMemory().Slice(start, count);
    }

    private void AddToBuffer(EntMut n)
    {
        if (traverseBufferIndex == traverseBuffer.Length)
            Array.Resize(ref traverseBuffer, traverseBuffer.Length * 2);
        traverseBuffer[traverseBufferIndex++] = n;
    }
}
