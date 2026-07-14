namespace AlvorKit.ECS;

internal class EntAllocator(int allocatorIndex, bool exclusive)
{
    private readonly List<int> pages = [];
    private readonly ConcurrentBag<int> free = [];
    // The alloc owner is the sole reader and writer. Finalizers publish only through pendingArchCleanup.
    private readonly List<EntArchGroupOps> archGroups = [];
    private readonly ConcurrentQueue<EntMut> pendingArchCleanup = [];

    private int nextIndex;
    private int limitIndex;
    private int generation;

    internal Span<int> Pages => CollectionsMarshal.AsSpan(pages);
    internal int Capacity => Pages.Length * EntReg.PageSize;
    internal int Free => BagFree + PageFree;
    internal int BagFree => free.Count;
    internal int PageFree => limitIndex - nextIndex;
    internal ref int Generation => ref generation;

    internal int Next()
    {
        int index = TryAllocFromCurrentPage();
        if (index >= 0)
            return index;

        if (free.TryTake(out index))
            return index;

        lock (this)
        {
            index = TryAllocFromCurrentPage();
            if (index >= 0)
                return index;

            return AllocFromNewPage();
        }
    }

    private int AllocFromNewPage()
    {
        if (!exclusive && EntReg.FreePages.TryTake(out int nextPage))
            EntReg.PageAllocators[nextPage] = allocatorIndex;
        else nextPage = CreateNewPage();

        pages.Add(nextPage);

        int start = nextPage * EntReg.PageSize;
        int end = start + EntReg.PageSize;

        nextIndex = start + 1;
        limitIndex = end;

        return start;
    }

    private int CreateNewPage()
    {
        lock (EntReg.Lock)
        {
            int nextPage = EntReg.NextPage++;
            EntReg.PageAllocators.Add(allocatorIndex);

            if (nextPage >= EntReg.PageGenerations.Length)
                Array.Resize(ref EntReg.PageGenerations, EntReg.PageGenerations.Length * 2);
            EntReg.PageGenerations[nextPage] = new int[EntReg.PageSize];

            foreach (var field in EntReg.Fields)
                field.ExpandCapacity();

            return nextPage;
        }
    }

    private int TryAllocFromCurrentPage()
    {
        while (true)
        {
            int current = Volatile.Read(ref nextIndex);
            int limit = Volatile.Read(ref limitIndex);

            if (current >= limit)
                return -1;

            int next = current + 1;

            if (Interlocked.CompareExchange(ref nextIndex, next, current) == current)
                return current;
        }
    }

    internal void Add(int index) => free.Add(index);

    internal void RegisterArchGroup(EntArchGroupOps group) => archGroups.Add(group);

    internal void RemoveArchetypal(EntMut ent)
    {
        foreach (EntArchGroupOps group in archGroups)
            group.Remove(ent);

        DrainPendingArchetypal();
    }

    internal void QueueArchetypalCleanup(EntMut ent) => pendingArchCleanup.Enqueue(ent);

    internal void DrainPendingArchetypal()
    {
        if (pendingArchCleanup.IsEmpty)
            return;

        while (pendingArchCleanup.TryDequeue(out EntMut ent))
        {
            foreach (EntArchGroupOps group in archGroups)
                group.Remove(ent);
        }
    }

    internal void ClearArchetypal()
    {
        while (pendingArchCleanup.TryDequeue(out _)) { }
        foreach (EntArchGroupOps group in archGroups)
            group.ClearAlloc(allocatorIndex);
        archGroups.Clear();
    }

    internal void Clear()
    {
        pages.Clear();
        free.Clear();
        archGroups.Clear();
        while (pendingArchCleanup.TryDequeue(out _)) { }
        nextIndex = 0;
        limitIndex = 0;
    }
}
