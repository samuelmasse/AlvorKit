namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchCompactionTest
{
    /// <summary>Verifies first-row removal swaps the last Ent into the hole and repairs its loc.</summary>
    [TestMethod]
    public void Archetypal_Compaction_FirstRow_SwapsLastAndRepairsLoc() =>
        AssertCompaction<FirstCompactionArch>(0);

    /// <summary>Verifies middle-row removal swaps the last Ent into the hole and repairs its loc.</summary>
    [TestMethod]
    public void Archetypal_Compaction_MiddleRow_SwapsLastAndRepairsLoc() =>
        AssertCompaction<MiddleCompactionArch>(1);

    /// <summary>Verifies last-row removal does not move or rewrite another Ent's loc.</summary>
    [TestMethod]
    public void Archetypal_Compaction_LastRow_LeavesOtherLocsUnchanged() =>
        AssertCompaction<LastCompactionArch>(3);

    /// <summary>Verifies a removed class value is collectible after retained-field movement and group exit.</summary>
    [TestMethod]
    public void Archetypal_ClassReference_IsCollectibleAfterMoveAndExit()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        var weak = StoreMoveAndRemoveClass<ClassCollectionArch>(ent);

        ForceFullCollection();

        Assert.IsFalse(weak.TryGetTarget(out _));
        Assert.IsFalse(ent.Has<EntArchLoc, ClassCollectionArch>());
    }

    /// <summary>Verifies a reference inside a value type is collectible after retained-field movement and group exit.</summary>
    [TestMethod]
    public void Archetypal_RefContainingStruct_IsCollectibleAfterMoveAndExit()
    {
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        var weak = StoreMoveAndRemoveStruct<StructCollectionArch>(ent);

        ForceFullCollection();

        Assert.IsFalse(weak.TryGetTarget(out _));
        Assert.IsFalse(ent.Has<EntArchLoc, StructCollectionArch>());
    }

    private static void AssertCompaction<A>(int removedRow)
    {
        using var arena = new EntArena();
        var ents = new EntMut[4];
        var boxes = new RefBox[ents.Length];

        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = arena.Alloc();
            boxes[i] = new(i);
            ents[i].SetArchetypal<int, ValueField, A>(100 + i);
            ents[i].SetArchetypal<long, RetainedField, A>(200L + i);
            ents[i].SetArchetypal<RefBox, RefField, A>(boxes[i]);
        }

        var initialLocs = ents.Select(static ent => ent.Get<EntArchLoc, A>()).ToArray();
        int srcArchId = initialLocs[0].ArchId;
        int allocId = initialLocs[0].AllocId;
        int lastRow = ents.Length - 1;

        for (int i = 0; i < initialLocs.Length; i++)
        {
            Assert.AreEqual(srcArchId, initialLocs[i].ArchId);
            Assert.AreEqual(allocId, initialLocs[i].AllocId);
            Assert.AreEqual(i, initialLocs[i].Row);
        }

        Assert.IsTrue(ents[removedRow].UnsetArchetypal<int, ValueField, A>());
        Assert.IsFalse(ents[removedRow].HasArchetypal<int, ValueField, A>());
        Assert.AreEqual(200L + removedRow, ents[removedRow].GetArchetypal<long, RetainedField, A>());
        Assert.AreSame(boxes[removedRow], ents[removedRow].GetArchetypal<RefBox, RefField, A>());

        for (int i = 0; i < ents.Length; i++)
        {
            if (i == removedRow)
                continue;

            Assert.AreEqual(100 + i, ents[i].GetArchetypal<int, ValueField, A>());
            Assert.AreEqual(200L + i, ents[i].GetArchetypal<long, RetainedField, A>());
            Assert.AreSame(boxes[i], ents[i].GetArchetypal<RefBox, RefField, A>());

            var loc = ents[i].Get<EntArchLoc, A>();
            Assert.AreEqual(srcArchId, loc.ArchId);
            Assert.AreEqual(i == lastRow && removedRow != lastRow ? removedRow : i, loc.Row);
        }

        Assert.AreEqual(
            100 + lastRow,
            EntArchColumn<int, ValueField, A>.Values[allocId][srcArchId][lastRow],
            "Reference-free tail storage should intentionally remain dirty.");
        Assert.IsNull(
            EntArchColumn<RefBox, RefField, A>.Values[allocId][srcArchId][lastRow],
            "Reference-containing tail storage must be cleared.");

        foreach (var ent in ents)
            ExitCompactionArch<A>(ent);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference<RefBox> StoreMoveAndRemoveClass<A>(EntMut ent)
    {
        var box = new RefBox(42);
        var weak = new WeakReference<RefBox>(box);

        ent.SetArchetypal<int, ValueField, A>(1);
        ent.SetArchetypal<RefBox, RefField, A>(box);
        ent.UnsetArchetypal<int, ValueField, A>();
        ent.UnsetArchetypal<RefBox, RefField, A>();

        return weak;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference<RefBox> StoreMoveAndRemoveStruct<A>(EntMut ent)
    {
        var box = new RefBox(43);
        var weak = new WeakReference<RefBox>(box);

        ent.SetArchetypal<int, ValueField, A>(1);
        ent.SetArchetypal<RefPayload, RefField, A>(new(box, 2));
        ent.UnsetArchetypal<int, ValueField, A>();
        ent.UnsetArchetypal<RefPayload, RefField, A>();

        return weak;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void ForceFullCollection()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    private static void ExitCompactionArch<A>(EntMut ent)
    {
        ent.UnsetArchetypal<int, ValueField, A>();
        ent.UnsetArchetypal<long, RetainedField, A>();
        ent.UnsetArchetypal<RefBox, RefField, A>();
        Assert.IsFalse(ent.Has<EntArchLoc, A>());
    }

    private sealed record RefBox(int Value);

    private readonly record struct RefPayload(RefBox? Ref, int Value);
    private readonly record struct ValueField;
    private readonly record struct RetainedField;
    private readonly record struct RefField;
    private readonly record struct FirstCompactionArch;
    private readonly record struct MiddleCompactionArch;
    private readonly record struct LastCompactionArch;
    private readonly record struct ClassCollectionArch;
    private readonly record struct StructCollectionArch;
}
