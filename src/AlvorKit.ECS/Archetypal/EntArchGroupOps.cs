namespace AlvorKit.ECS;

/// <summary>Provides allocator lifecycle operations for one registered archetypal group.</summary>
internal abstract class EntArchGroupOps
{
    /// <summary>Removes an Ent from this group when it currently owns a row.</summary>
    internal abstract void Remove(EntMut ent);

    /// <summary>Returns every alloc-local row and component array owned by this group.</summary>
    internal abstract void ClearAlloc(int allocId);
}
