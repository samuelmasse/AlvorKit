namespace AlvorKit.ECS;

// Archetypal design:
// - A identifies an independent arch group. Each participating Ent has a sparse EntArchLoc<A> containing its alloc,
//   arch, and dense row. EntArchColumn<T, N, A> stores component values at that same alloc/arch/row coordinate.
// - EntArchGraph<A> is shared by the group. Its lock serializes field registration, arch creation, transition caching,
//   and jagged-array outer growth. Runtime rows and component columns remain partitioned by alloc.
// - Every canonical packed signature is the immutable field-membership authority and has parallel field layouts. Ordinary
//   point access goes directly through its closed-generic column at alloc/arch/row; it does not search the signature or graph.
//   Packed membership and sparse transition edges are used only while resolving structural changes. Reference-free layouts
//   store byte-column prefixes, while reference-containing layouts store type-local columns.
// - EntArchColumn is the beforefieldinit hot Values holder. Its FieldId forwards to the precise EntArchColumnOps initializer,
//   so absent Get, Has, and Unset do not register unused fields or allocate their ops. A live structural Set registers the
//   field before resolving an arch; the CLR runs that initializer once and graph publication remains locked.
// - After a column lookup succeeds, point access uses the subsystem-owned loc as a valid row and skips the final array bounds
//   check. Append and Move return a capacity-backed row, Move copies retained fields, and the checked first write initializes
//   a new field before point access resumes. Swap-back repairs the moved Ent's loc immediately. The resulting typed managed
//   byref preserves GC write barriers.
// - One thread exclusively owns an alloc's data for a given group. Reads, writes, and row changes inside that alloc are
//   intentionally unsynchronized. Multiple threads may use the same group and arch concurrently only through different allocs.
// - Structural changes move an Ent between dense arch row sets. Adding copies every src field; removing copies only the
//   fields retained by dst. The transitioned Ent receives its new loc after the move.
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
        var values = EntArchColumn<T, N, A>.ValuesAt(loc.AllocId, loc.ArchId);

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

        return EntArchColumn<T, N, A>.ValuesAt(loc.AllocId, loc.ArchId) != null;
    }

    /// <summary>Overwrites an existing archetypal component or structurally adds the missing field.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetArchetypal<T, N, A>(in T value)
    {
        var loc = Get<EntArchLoc, A>();
        int srcArchId = loc.ArchId;
        var values = EntArchColumn<T, N, A>.ValuesAt(loc.AllocId, srcArchId);

        if (values != null)
        {
            Unsafe.Add(
                ref MemoryMarshal.GetArrayDataReference(values),
                loc.Row) = value;
            return;
        }

        if (!IsAlive)
            return;

        int fieldId = EntArchColumn<T, N, A>.FieldId;
        int dstArchId;
        if (srcArchId == EntArchGraph<A>.NoArchId)
        {
            dstArchId = EntArchGraph<A>.GetSingletonArchId(fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveSingleton(fieldId);

            loc.AllocId = EntReg.PageAllocators[PageIndex];
            loc.Row = EntArchRows<A>.Append(loc.AllocId, dstArchId, this);
        }
        else
        {
            dstArchId = EntArchGraph<A>.GetTransitionArchId(srcArchId, fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveAdd(srcArchId, fieldId);

            loc.Row = EntArchRows<A>.Move(loc.AllocId, srcArchId, loc.Row, dstArchId, srcArchId);
        }

        loc.ArchId = dstArchId;
        Set<EntArchLoc, A>(loc);
        EntArchColumn<T, N, A>.Values[loc.AllocId][loc.ArchId][loc.Row] = value;
    }

    /// <summary>Structurally removes an archetypal field and returns whether it was present.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool UnsetArchetypal<T, N, A>()
    {
        var loc = Get<EntArchLoc, A>();
        int srcArchId = loc.ArchId;

        if (EntArchColumn<T, N, A>.ValuesAt(loc.AllocId, srcArchId) == null)
            return false;

        if (EntArchGraph<A>.IsSingleton(srcArchId))
        {
            EntArchRows<A>.Remove(loc.AllocId, srcArchId, loc.Row);
            Unset<EntArchLoc, A>();
        }
        else
        {
            int fieldId = EntArchColumn<T, N, A>.FieldId;
            int dstArchId = EntArchGraph<A>.GetTransitionArchId(srcArchId, fieldId);
            if (dstArchId == EntArchGraph<A>.UnresolvedTransitionArchId)
                dstArchId = EntArchGraph<A>.ResolveRemove(srcArchId, fieldId);

            loc.Row = EntArchRows<A>.Move(loc.AllocId, srcArchId, loc.Row, dstArchId, dstArchId);
            loc.ArchId = dstArchId;
            Set<EntArchLoc, A>(loc);
        }

        return true;
    }
}
