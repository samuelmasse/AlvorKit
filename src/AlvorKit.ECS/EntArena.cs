namespace AlvorKit.ECS;

/// <summary>Owns a disposable arena allocator for manually managed ECS entities.</summary>
public readonly struct EntArena : IDisposable
{
    private readonly int index;
    private readonly int generation;

    /// <summary>Returns whether the arena has not been disposed.</summary>
    public bool IsAlive => index != 0 && EntReg.Allocators[index].Generation == generation;

    /// <summary>Gets the number of currently allocated live entities in this arena.</summary>
    public int Allocated => Capacity - Free;

    internal int Index => index;
    internal int Generation => generation;
    internal int Capacity => IsAlive ? Allocator.Capacity : 0;
    internal int Free => IsAlive ? Allocator.Free : 0;
    internal EntAllocator Allocator => EntReg.Allocators[index];
    internal EntRegView Registry => EntReg.View;

    /// <summary>Creates an arena backed by a reusable allocator slot.</summary>
    public EntArena()
    {
        if (!EntReg.FreeAllocators.TryTake(out index))
        {
            lock (EntReg.Allocators)
            {
                index = EntReg.Allocators.Count;
                EntReg.Allocators.Add(new(index, false));
            }
        }

        generation = Allocator.Generation;
    }

    /// <summary>Allocates a new disposable entity owned by this arena.</summary>
    public EntPtr Alloc()
    {
        if (!IsAlive)
            throw new EntArenaDisposedException();

        return new(index);
    }

    /// <summary>Disposes the arena, releases all pages, and invalidates entities allocated from it.</summary>
    public void Dispose()
    {
        if (!IsAlive)
            return;

        lock (Allocator)
        {
            if (!IsAlive)
                return;

            for (int i = Allocator.Pages.Length - 1; i >= 0; i--)
                ReleasePage(Allocator.Pages[i]);

            Allocator.Clear();
            Allocator.Generation++;
            EntReg.FreeAllocators.Add(index);
        }
    }

    private void ReleasePage(int pageIndex)
    {
        BumpPageGenerations(pageIndex);

        foreach (var field in EntReg.PageFields.Fields(pageIndex))
            field.ResetPage(pageIndex);

        EntReg.PageFields.Clear(pageIndex);
        EntReg.PageRefFields.Clear(pageIndex);
        EntReg.PageAllocators[pageIndex] = -1;

        BumpPageGenerations(pageIndex);

        EntReg.FreePages.Add(pageIndex);
    }

    private void BumpPageGenerations(int pageIndex)
    {
        var gens = EntReg.PageGenerations[pageIndex];
        for (int j = 0; j < gens.Length; j++)
            gens[j]++;
    }
}
