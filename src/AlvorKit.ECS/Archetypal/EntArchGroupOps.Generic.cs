namespace AlvorKit.ECS;

/// <summary>Connects one closed archetypal group to allocator cleanup without adding point-access indirection.</summary>
internal sealed class EntArchGroupOps<A> : EntArchGroupOps
{
    /// <summary>The stateless lifecycle adapter shared by every alloc using this group.</summary>
    internal static readonly EntArchGroupOps<A> Instance = new();

    private EntArchGroupOps() { }

    internal override void Remove(EntMut ent)
    {
        var page = EntStorage<EntArchLoc, A>.Sparse[ent.PageIndex];
        if (page == null)
            return;

        ref var stored = ref page[ent.SubIndex];
        if (stored.Generation != ent.Generation)
            return;

        var loc = stored.Value;
        EntArchRows<A>.Remove(loc.AllocId, loc.ArchId, loc.Row);
        stored = default;
    }

    internal override void ClearAlloc(int allocId)
    {
        EntArchGraph<A>.ClearAllocColumns(allocId);
        EntArchRows<A>.ClearAlloc(allocId);
    }
}
