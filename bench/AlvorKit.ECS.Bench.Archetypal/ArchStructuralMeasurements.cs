namespace AlvorKit;

internal static class ArchStructuralMeasurements
{
    internal static BenchResult AddCached<A>(int count)
    {
        using var alloc = new EntArena();
        GC.KeepAlive(EntArchColumn<int, FToggle, A>.FieldId);
        var ents = new EntMut[count];

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            ArchShapes.SetSevenFillers<A>(ents[i]);
        }

        // Populate both sides once so the measured pass can rent pre-sized returned buffers.
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);

        for (int i = 0; i < ents.Length; i++)
            ents[i].UnsetArchetypal<int, FToggle, A>();
        var timer = BenchTimer.Start();
        ArchAddCached.Run<A>(ents);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(ents[^1].GetArchetypal<int, FToggle, A>());
        return result;
    }

    internal static BenchResult AddGrowth<A>(int count)
    {
        using var alloc = new EntArena();
        EntMut warmEnt = alloc.Alloc();
        ArchShapes.SetSevenFillers<A>(warmEnt);
        warmEnt.SetArchetypal<int, FToggle, A>(1);
        warmEnt.UnsetArchetypal<int, FToggle, A>();
        var ents = new EntMut[count];

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            ArchShapes.SetSevenFillers<A>(ents[i]);
        }

        var timer = BenchTimer.Start();
        ArchAddGrowth.Run<A>(ents);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(ents[^1].GetArchetypal<int, FToggle, A>());
        return result;
    }

    internal static BenchResult AddUnknown<A>(int count)
    {
        using var alloc = new EntArena();
        GC.KeepAlive(EntArchColumn<int, FToggle, A>.FieldId);
        var ents = new EntMut[count];
        uint fields = (1u << 8) - 1;

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            ArchShapes.SetMask<A>(ents[i], fields);
            fields = ArchCombinations.Next(fields);
        }

        var timer = BenchTimer.Start();
        ArchAddUnknown.Run<A>(ents);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(ents[^1].GetArchetypal<int, FToggle, A>());
        return result;
    }

    internal static BenchResult RemoveCached<A>(int count)
    {
        using var alloc = new EntArena();
        var ents = new EntMut[count];

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            ArchShapes.SetSevenFillers<A>(ents[i]);
        }

        // Grow the dst while it is populated, then move every Ent to src before timing.
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);
        var timer = BenchTimer.Start();
        var observed = ArchRemoveCached.Run<A>(ents);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(observed);
        return result;
    }

    internal static BenchResult RemoveUnknown<A>(int count)
    {
        using var alloc = new EntArena();
        var ents = new EntMut[count];
        uint fields = (1u << 7) - 1;

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = alloc.Alloc();
            ents[i].SetArchetypal<int, FToggle, A>(i);
            ArchShapes.SetMask<A>(ents[i], fields);
            fields = ArchCombinations.Next(fields);
        }

        var timer = BenchTimer.Start();
        var observed = ArchRemoveUnknown.Run<A>(ents);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(observed);
        return result;
    }

    internal static BenchResult Compaction<A>(int count, CompactionPosition position)
    {
        using var alloc = new EntArena();
        var byRow = new EntMut[count];

        for (int i = 0; i < byRow.Length; i++)
        {
            byRow[i] = alloc.Alloc();
            ArchShapes.SetSevenFillers<A>(byRow[i]);
        }

        // Pre-size both src and dst; the measured body isolates row movement and compaction.
        for (int i = 0; i < byRow.Length; i++)
            byRow[i].SetArchetypal<int, FToggle, A>(i);
        var timer = BenchTimer.Start();
        var observed = ArchCompaction.Run<A>(byRow, position);
        var result = timer.Stop(count, "move");
        GC.KeepAlive(observed);
        return result;
    }

    internal static BenchResult UniqueSignature<A>(int archCount)
    {
        using var alloc = new EntArena();
        ArchShapes.RegisterFields<A>(32);
        EntMut ent = alloc.Alloc();
        var timer = BenchTimer.Start();
        ArchUniqueSignature.Run<A>(ent, archCount);
        var result = timer.Stop(archCount, "arch");
        GC.KeepAlive(ent.GetArchetypal<int, F00, A>());
        return result;
    }

    internal static BenchResult LowOccupancy<A>(int archCount)
    {
        using var alloc = new EntArena();
        ArchShapes.RegisterFields<A>(32);
        var ents = new EntMut[archCount];
        var timer = BenchTimer.Start();
        ArchLowOccupancy.Run<A>(alloc, ents);
        var result = timer.Stop(archCount, "arch");
        GC.KeepAlive(ents.Length);
        return result;
    }

    internal static BenchResult HighOccupancy<A>(int rowCount)
    {
        using var alloc = new EntArena();
        ArchShapes.RegisterFields<A>(16);
        var ents = new EntMut[rowCount];
        var timer = BenchTimer.Start();
        ArchHighOccupancy.Run<A>(alloc, ents);
        var result = timer.Stop(rowCount, "row");
        GC.KeepAlive(ents.Length);
        return result;
    }

    internal static BenchResult TransitionLookup<A>(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<A>(ent, width);
        int centerArchId = ent.Get<EntArchLoc, A>().ArchId;
        int[] fieldIds = EntArchGraph<A>.FieldIds(centerArchId).ToArray();

        for (int field = 0; field < width; field++)
        {
            ArchShapes.ToggleField<A>(ent, field, false);
            ArchShapes.ToggleField<A>(ent, field, true);
        }

        var timer = BenchTimer.Start();
        var observed = ArchTransitionLookup.Run<A>(fieldIds, centerArchId, width, 1048576);
        var result = timer.Stop(1048576, "lookup");
        GC.KeepAlive(observed);
        return result;
    }
}
