namespace AlvorKit.ECS;

public readonly struct EntArena : IDisposable
{
    private readonly int index;
    private readonly int generation;

    public bool IsAlive => index != 0 && EntReg.Allocators[index].Generation == generation;

    public int Allocated => Capacity - Free;

    internal int Index => index;
    internal int Generation => generation;
    internal int Capacity => IsAlive ? Allocator.Capacity : 0;
    internal int Free => IsAlive ? Allocator.Free : 0;
    internal EntAllocator Allocator => EntReg.Allocators[index];
    internal EntRegView Registry => EntReg.View;

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

    public EntPtr Alloc()
    {
        if (!IsAlive)
            throw new EntArenaDisposedException();

        return new(index);
    }

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
