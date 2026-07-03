namespace AlvorKit.UI;

using AlvorKit.UI.Root;

public static class UiSyntax
{
    public static EntMut NodeC(EntMut parent) => parent.UiRoot.Alloc();

    public static EntMutator<EntMut> Node(EntMut parent)
    {
        var val = parent.UiRoot.Alloc();
        parent.UiNodes = Append(parent.UiRoot.Allocator, parent.UiNodes, val);
        return val.Mutate();
    }

    public static EntMutator<EntMut> Node(EntMut parent, out EntMut val)
    {
        val = parent.UiRoot.Alloc();
        parent.UiNodes = Append(parent.UiRoot.Allocator, parent.UiNodes, val);
        return val.Mutate();
    }

    public static EntMutator<EntMut> NodeS(EntMut parent)
    {
        var val = parent.UiRoot.Alloc();
        parent.UiNodeStack = Append(parent.UiRoot.Allocator, parent.UiNodeStack, val);
        return val.Mutate();
    }

    public static Span<EntMut> Nodes(EntMut parent) => Used(parent.UiRoot.Allocator, parent.UiNodes);

    public static void NodesClear(EntMut parent) => parent.UiNodes = parent.UiNodes with { Count = 0 };

    public static int NodesCount(EntMut parent) => parent.UiNodes.Count;

    public static void NodesAdd(EntMut parent, EntMut child) =>
        parent.UiNodes = Append(parent.UiRoot.Allocator, parent.UiNodes, child);

    public static bool NodesRemove(EntMut parent, EntMut child)
    {
        var slot = parent.UiNodes;
        var span = Used(parent.UiRoot.Allocator, slot);
        var index = span.IndexOf(child);
        if (index < 0)
            return false;

        parent.UiNodes = RemoveAt(parent.UiRoot.Allocator, slot, index);

        return true;
    }

    public static void NodesRemoveAt(EntMut parent, int index) => parent.UiNodes = RemoveAt(parent.UiRoot.Allocator, parent.UiNodes, index);

    public static Span<EntMut> NodeStack(EntMut parent) => Used(parent.UiRoot.Allocator, parent.UiNodeStack);

    public static int NodeStackCount(EntMut parent) => parent.UiNodeStack.Count;

    public static EntMut NodeStackPop(EntMut parent)
    {
        var slot = parent.UiNodeStack;
        var lastIndex = slot.Count - 1;
        var span = parent.UiRoot.Allocator.Span(slot);

        var val = span[lastIndex];
        span[lastIndex] = default;
        parent.UiNodeStack = slot with { Count = lastIndex };

        return val;
    }

    public static bool NodeStackTryPeek(EntMut parent, out EntMut child)
    {
        var slot = parent.UiNodeStack;
        if (slot.Count == 0)
        {
            child = default;
            return false;
        }

        child = parent.UiRoot.Allocator.Span(slot)[slot.Count - 1];
        return true;
    }

    private static Span<EntMut> Used(NodeArrayAllocator pool, NodeArray slot) => pool.Span(slot)[..slot.Count];

    private static NodeArray Append(NodeArrayAllocator pool, NodeArray slot, EntMut value)
    {
        var capacity = slot.Rank == 0 ? 0 : pool.BlockSize(slot.Rank);
        if (slot.Count >= capacity)
            slot = Promote(pool, slot);

        pool.Span(slot)[slot.Count] = value;
        return slot with { Count = slot.Count + 1 };
    }

    private static NodeArray RemoveAt(NodeArrayAllocator pool, NodeArray slot, int index)
    {
        var span = Used(pool, slot);
        span[(index + 1)..].CopyTo(span[index..]);
        var newCount = slot.Count - 1;
        span[newCount] = default;
        return slot with { Count = newCount };
    }

    private static NodeArray Promote(NodeArrayAllocator pool, NodeArray slot)
    {
        var newRank = slot.Rank == 0 ? 1 : slot.Rank + 1;
        var newSlot = pool.Alloc(newRank);

        if (slot.Rank != 0)
        {
            Used(pool, slot).CopyTo(pool.Span(newSlot));
            pool.Free(slot);
        }

        return newSlot with { Count = slot.Count };
    }
}
