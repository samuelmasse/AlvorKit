namespace AlvorKit.ECS;

// Archetypal design:
// - A identifies an independent arch group. Each participating Ent has a sparse EntArchLoc<A> containing its row-set ID,
//   arch, and dense row. EntArchColumn<T, N, A> maps that row-set ID directly to the component array.
// - EntArchGraph<A> is shared by the group. Its lock serializes field registration, arch creation, transition caching,
//   row-set catalog growth, and column-directory growth. Runtime row sets remain partitioned by alloc ownership.
// - Every canonical packed signature is the immutable field-membership authority. Ordinary
//   point access goes directly through its closed-generic column at rowSetId/row; it does not search the signature or graph.
//   Packed membership and sparse transition edges are used only while resolving structural changes.
// - EntArchColumn is the beforefieldinit hot Values holder. Its FieldId forwards to the precise EntArchColumnOps initializer,
//   so absent Get, Has, and Unset do not register unused fields or allocate their ops. A live structural Set registers the
//   field before resolving an arch; the CLR runs that initializer once and graph publication remains locked.
// - After a column lookup succeeds, point access uses the subsystem-owned loc as a valid row and skips the final array bounds
//   check. Append and Move return a capacity-backed row, Move copies retained fields, and the checked first write initializes
//   a new field before point access resumes. Swap-back repairs the moved Ent's loc immediately. The resulting typed managed
//   byref preserves GC write barriers.
// - One thread exclusively owns an alloc's data for a given group. Reads, writes, and row changes inside that alloc are
//   intentionally unsynchronized. Multiple threads may use the same group and arch concurrently only through different allocs.
// - Span queries are alloc-scoped. Each query caches matching arch IDs, while each alloc keeps only its active row-set IDs.
//   Enumeration scans the smaller list and uses lazy arch-membership bits when the active side wins. The matching prefix,
//   membership-ready flag, and initialized bit-array publication share one atomic scan snapshot. A typed selection chain
//   resolves each requested column once per chunk, after which Ent and component iteration is aligned span indexing. Structural
//   changes in the same alloc/group are forbidden while an enumerator, chunk, or span is active; other alloc owners remain independent.
// - When a new Ent's complete shape is known, AllocArchetypal builds a typed value chain and appends directly to that final arch.
//   Each closed builder shape resolves and caches its canonical arch once, so repeated construction performs no transition walk
//   and never occupies intermediate row sets. Sequential Set remains the structural API for genuine incremental shape changes.
// - Structural changes move an Ent between dense arch row sets. One row-set ID represents one observed alloc/arch pair and is
//   recycled when the alloc is cleared. Adding copies every src field; removing copies only fields retained by dst. Transition
//   lists remain compact through degree eight; higher-degree archs also use one group-shared sparse transition hash.
// - Row and component arrays use one exact power-of-two pool per T across all arch groups and allocs. Capacity starts at four,
//   doubles when full, and halves below 25% occupancy. A zero-row alloc/arch state returns all payload arrays, and the alloc's
//   active row-set index follows the same bounded-capacity policy. Each size uses an
//   O(1)-amortized synchronized stack that grows with observed return demand instead of reserving per-thread slots. A Gen2
//   change ages inactive buckets, and a later Gen2 change drops those still unused, including their stack metadata. Cached
//   reference arrays are cleared before publication; reference-free arrays remain dirty. Pool synchronization exists only on
//   structural paths; point access stays direct and typed.
// - Removing a src row uses swap-back compaction: the last Ent and all its fields fill the hole, and that Ent's loc is
//   repaired immediately. Reference-containing fields are cleared at the old tail for GC correctness. Fields without
//   references and the tail Ent slot are intentionally left dirty beyond Count. This avoids unnecessary writes and shifting
//   later rows; compaction touches only the src arch's component columns.
// - Arch 0 means the Ent is outside this group; there is no stored empty arch. Removing the last field compacts the
//   singleton row set and unsets EntArchLoc<A>.
public readonly partial record struct EntMut
{
    /// <summary>Gets an archetypal component or the default value when this Ent does not have the field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? GetArchetypal<T, N, A>()
    {
        var loc = Get<EntArchLoc, A>();
        var values = EntArchColumn<T, N, A>.ValuesAt(loc.RowSetId);

        if (values == null)
            return default;

        return Unsafe.Add(
            ref MemoryMarshal.GetArrayDataReference(values),
            loc.Row);
    }

    /// <summary>Returns whether this Ent currently has the specified archetypal field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasArchetypal<T, N, A>()
    {
        var loc = Get<EntArchLoc, A>();

        return EntArchColumn<T, N, A>.ValuesAt(loc.RowSetId) != null;
    }

    /// <summary>Overwrites an existing archetypal component or structurally adds the missing field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetArchetypal<T, N, A>(in T value)
    {
        var loc = Get<EntArchLoc, A>();
        var values = EntArchColumn<T, N, A>.ValuesAt(loc.RowSetId);

        if (values != null)
        {
            Unsafe.Add(
                ref MemoryMarshal.GetArrayDataReference(values),
                loc.Row) = value;
            return;
        }

        if (!IsAlive)
            return;

        int allocId = EntReg.PageAllocators[PageIndex];
        EntAllocator alloc = EntReg.Allocators[allocId];
        alloc.DrainPendingArchetypal();
        loc = Get<EntArchLoc, A>();
        int srcArchId = loc.ArchId;

        int fieldId = EntArchColumn<T, N, A>.FieldId;
        int dstArchId;
        if (srcArchId == EntArchGraph<A>.NoArchId)
        {
            dstArchId = EntArchGraph<A>.GetSingletonArchId(fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveSingleton(fieldId);

            loc = EntArchRows<A>.Append(allocId, dstArchId, this);
        }
        else
        {
            dstArchId = EntArchGraph<A>.GetTransitionArchId(srcArchId, fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveAdd(srcArchId, fieldId);

            loc = EntArchRows<A>.Move(allocId, loc.RowSetId, loc.Row, dstArchId, srcArchId);
        }

        Set<EntArchLoc, A>(loc);
        EntArchColumn<T, N, A>.Values[loc.RowSetId][loc.Row] = value;
    }

    /// <summary>Structurally removes an archetypal field and returns whether it was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool UnsetArchetypal<T, N, A>()
    {
        var loc = Get<EntArchLoc, A>();

        if (EntArchColumn<T, N, A>.ValuesAt(loc.RowSetId) == null)
            return false;

        int allocId = EntReg.PageAllocators[PageIndex];
        EntReg.Allocators[allocId].DrainPendingArchetypal();
        loc = Get<EntArchLoc, A>();
        int srcArchId = loc.ArchId;

        if (EntArchGraph<A>.IsSingleton(srcArchId))
        {
            EntArchRows<A>.Remove(allocId, loc.RowSetId, loc.Row);
            Unset<EntArchLoc, A>();
        }
        else
        {
            int fieldId = EntArchColumn<T, N, A>.FieldId;
            int dstArchId = EntArchGraph<A>.GetTransitionArchId(srcArchId, fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveRemove(srcArchId, fieldId);

            loc = EntArchRows<A>.Move(allocId, loc.RowSetId, loc.Row, dstArchId, dstArchId);
            Set<EntArchLoc, A>(loc);
        }

        return true;
    }
}
