namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private EcsArchBenchSample RunAddCached(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateAddCachedCase<RunArch>(options.Rows),
            () => CreateAddCachedCase<WarmArch>(Math.Min(options.Rows, 16)).Body());

    private EcsArchBenchCase CreateAddCachedCase<A>(int count)
    {
        var alloc = new EntArena();
        _ = EntArchColumn<int, FToggle, A>.FieldId;
        var ents = new EntMut[count];
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            EcsArchBenchShapes.SetSevenFillers<A>(ents[i]);
        }

        // Populate both sides once so the measured pass can rent pre-sized returned buffers.
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);
        for (int i = 0; i < ents.Length; i++)
            ents[i].UnsetArchetypal<int, FToggle, A>();

        var state = new EcsArchBenchState([alloc], ents);
        return new("move", ents.Length, Body, state);

        void Body()
        {
            for (int i = 0; i < ents.Length; i++)
                ents[i].SetArchetypal<int, FToggle, A>(i);
            longSink = ents[^1].GetArchetypal<int, FToggle, A>();
        }
    }

    private EcsArchBenchSample RunAddGrowth(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateAddGrowthCase<RunArch>(options.Rows),
            () => CreateAddGrowthCase<WarmArch>(Math.Min(options.Rows, 16)).Body());

    private EcsArchBenchCase CreateAddGrowthCase<A>(int count)
    {
        var alloc = new EntArena();
        EntMut warmEnt = Alloc(alloc);
        EcsArchBenchShapes.SetSevenFillers<A>(warmEnt);
        warmEnt.SetArchetypal<int, FToggle, A>(1);
        warmEnt.UnsetArchetypal<int, FToggle, A>();

        var ents = new EntMut[count];
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            EcsArchBenchShapes.SetSevenFillers<A>(ents[i]);
        }

        var state = new EcsArchBenchState([alloc], ents, warmEnt);
        return new("move", ents.Length, Body, state);

        void Body()
        {
            for (int i = 0; i < ents.Length; i++)
                ents[i].SetArchetypal<int, FToggle, A>(i);
            longSink = ents[^1].GetArchetypal<int, FToggle, A>();
        }
    }

    private EcsArchBenchSample RunAddUnknown(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateAddUnknownCase<RunArch>(UnknownTransitionCount()),
            () => CreateAddUnknownCase<WarmArch>(Math.Min(UnknownTransitionCount(), 16)).Body());

    private EcsArchBenchCase CreateAddUnknownCase<A>(int count)
    {
        var alloc = new EntArena();
        _ = EntArchColumn<int, FToggle, A>.FieldId;
        var ents = new EntMut[count];
        uint fields = (1u << 8) - 1;
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            EcsArchBenchShapes.SetMask<A>(ents[i], fields);
            fields = NextCombination(fields);
        }

        var state = new EcsArchBenchState([alloc], ents);
        return new("move", ents.Length, Body, state);

        void Body()
        {
            for (int i = 0; i < ents.Length; i++)
                ents[i].SetArchetypal<int, FToggle, A>(i);
            longSink = ents[^1].GetArchetypal<int, FToggle, A>();
        }
    }

    private EcsArchBenchSample RunRemoveCached(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateRemoveCachedCase<RunArch>(options.Rows),
            () => CreateRemoveCachedCase<WarmArch>(Math.Min(options.Rows, 16)).Body());

    private EcsArchBenchCase CreateRemoveCachedCase<A>(int count)
    {
        var alloc = new EntArena();
        var ents = new EntMut[count];
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            EcsArchBenchShapes.SetSevenFillers<A>(ents[i]);
        }

        // Grow the dst while it is populated, then move every Ent to src before timing.
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);

        var state = new EcsArchBenchState([alloc], ents);
        return new("move", ents.Length, Body, state);

        void Body()
        {
            long count = 0;
            for (int i = 0; i < ents.Length; i++)
            {
                if (ents[i].UnsetArchetypal<int, FToggle, A>())
                    count++;
            }
            longSink = count;
        }
    }

    private EcsArchBenchSample RunRemoveUnknown(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateRemoveUnknownCase<RunArch>(UnknownTransitionCount()),
            () => CreateRemoveUnknownCase<WarmArch>(Math.Min(UnknownTransitionCount(), 16)).Body());

    private EcsArchBenchCase CreateRemoveUnknownCase<A>(int count)
    {
        var alloc = new EntArena();
        var ents = new EntMut[count];
        uint fields = (1u << 7) - 1;
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            ents[i].SetArchetypal<int, FToggle, A>(i);
            EcsArchBenchShapes.SetMask<A>(ents[i], fields);
            fields = NextCombination(fields);
        }

        var state = new EcsArchBenchState([alloc], ents);
        return new("move", ents.Length, Body, state);

        void Body()
        {
            long removed = 0;
            for (int i = 0; i < ents.Length; i++)
            {
                if (ents[i].UnsetArchetypal<int, FToggle, A>())
                    removed++;
            }
            longSink = removed;
        }
    }

    private EcsArchBenchSample RunCompaction(string scenarioId, CompactionPosition position) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateCompactionCase<RunArch>(options.Rows, position),
            () => CreateCompactionCase<WarmArch>(Math.Min(options.Rows, 16), position).Body());

    private EcsArchBenchCase CreateCompactionCase<A>(int count, CompactionPosition position)
    {
        var alloc = new EntArena();
        var byRow = new EntMut[count];
        for (int i = 0; i < byRow.Length; i++)
        {
            byRow[i] = Alloc(alloc);
            EcsArchBenchShapes.SetSevenFillers<A>(byRow[i]);
        }

        // Pre-size both src and dst; the measured body isolates row movement and compaction.
        for (int i = 0; i < byRow.Length; i++)
            byRow[i].SetArchetypal<int, FToggle, A>(i);

        var state = new EcsArchBenchState([alloc], byRow);
        return new("move", byRow.Length, Body, state);

        void Body()
        {
            int live = byRow.Length;
            long removed = 0;
            while (live != 0)
            {
                int row = position switch
                {
                    CompactionPosition.First => 0,
                    CompactionPosition.Middle => live / 2,
                    CompactionPosition.Last => live - 1,
                    _ => throw new UnreachableException(),
                };

                EntMut ent = byRow[row];
                if (ent.UnsetArchetypal<int, FToggle, A>())
                    removed++;

                live--;
                if (row != live)
                    byRow[row] = byRow[live];
            }
            longSink = removed;
        }
    }

    private int UnknownTransitionCount() => Math.Min(options.Rows, options.Arches);

    private static uint NextCombination(uint value)
    {
        uint smallest = value & (0u - value);
        uint ripple = value + smallest;
        return ripple | (((value ^ ripple) >> 2) / smallest);
    }
}
