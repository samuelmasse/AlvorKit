namespace AlvorKit.ECS;

public readonly partial record struct EntMut
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T? GetArchetypal<T, N, A>()
    {
        var fieldId = EntArchStorage<T, N, A>.ArchFieldId;
        var loc = Get<EntArchLoc, A>();

        if (EntArchSet<A>.GraphAt(loc.ArchId, fieldId).Add != loc.ArchId)
            return default;

        return EntArchStorage<T, N, A>.Data[loc.AllocatorId][loc.ArchId][loc.Row];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasArchetypal<T, N, A>()
    {
        var fieldId = EntArchStorage<T, N, A>.ArchFieldId;
        var loc = Get<EntArchLoc, A>();

        return EntArchSet<A>.GraphAt(loc.ArchId, fieldId).Add == loc.ArchId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetArchetypal<T, N, A>(in T value)
    {
        var fieldId = EntArchStorage<T, N, A>.ArchFieldId;
        var loc = Get<EntArchLoc, A>();
        var add = EntArchSet<A>.GraphAt(loc.ArchId, fieldId).Add;

        if (add != loc.ArchId)
        {
            if (!IsAlive)
                return;

            if (loc.ArchId == 0)
            {
                var initArch = EntArchSet<A>.GraphAt(1, fieldId).Add;
                if (initArch == 0)
                    initArch = EntArchSet<A>.NewArch(fieldId);

                loc.AllocatorId = EntReg.PageAllocators[PageIndex];
                loc.Row = EntArchSet<A>.AddEnt(loc.AllocatorId, initArch, this);
                loc.ArchId = initArch;
            }
            else
            {
                var dstArch = add;
                if (dstArch == 0)
                    dstArch = EntArchSet<A>.ExtendArch(loc.ArchId, fieldId);

                loc.Row = EntArchSet<A>.MoveEnt(loc.AllocatorId, loc.ArchId, loc.Row, dstArch, loc.ArchId);
                loc.ArchId = dstArch;
            }

            Set<EntArchLoc, A>(loc);
        }

        EntArchStorage<T, N, A>.Data[loc.AllocatorId][loc.ArchId][loc.Row] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool UnsetArchetypal<T, N, A>()
    {
        var fieldId = EntArchStorage<T, N, A>.ArchFieldId;
        var loc = Get<EntArchLoc, A>();
        var add = EntArchSet<A>.GraphAt(loc.ArchId, fieldId).Add;

        if (add == loc.ArchId)
        {
            var dstArch = EntArchSet<A>.GraphAt(loc.ArchId, fieldId).Remove;
            if (dstArch < 0)
            {
                EntArchSet<A>.RemoveEnt(loc.AllocatorId, loc.ArchId, loc.Row);
                Unset<EntArchLoc, A>();
            }
            else
            {
                if (dstArch == 0)
                    dstArch = EntArchSet<A>.ReduceArch(loc.ArchId, fieldId);

                loc.Row = EntArchSet<A>.MoveEnt(loc.AllocatorId, loc.ArchId, loc.Row, dstArch, dstArch);
                loc.ArchId = dstArch;

                Set<EntArchLoc, A>(loc);
            }

            return true;
        }
        else return false;
    }
}
