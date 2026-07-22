namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchTransitionIndexTest
{
    /// <summary>Verifies a high-degree arch migrates observed transitions to the shared index without changing inverse IDs.</summary>
    [TestMethod]
    public void ArchetypalGraph_HighDegreeArch_IndexesEveryObservedTransition()
    {
        const int fieldCount = 10;
        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        SetAll(ent);
        int centerArchId = ent.Get<EntArchLoc, HighDegreeArch>().ArchId;
        var fieldIds = new int[fieldCount];
        var dstArchIds = new int[fieldCount];

        for (int field = 0; field < fieldCount; field++)
        {
            fieldIds[field] = FieldIdAt(field);
            UnsetAt(ent, field);
            dstArchIds[field] = ent.Get<EntArchLoc, HighDegreeArch>().ArchId;
            SetAt(ent, field);
            Assert.AreEqual(centerArchId, ent.Get<EntArchLoc, HighDegreeArch>().ArchId);
        }

        var metrics = EntArchDiagnostics<HighDegreeArch>.Capture();
        Assert.AreEqual(1, metrics.HighDegreeArchCount);
        Assert.IsTrue(metrics.TransitionIndexCount >= fieldCount);
        Assert.IsTrue(metrics.TransitionIndexCapacity >= metrics.TransitionIndexCount * 2);

        for (int field = 0; field < fieldCount; field++)
        {
            Assert.AreEqual(
                dstArchIds[field],
                EntArchGraph<HighDegreeArch>.GetTransitionArchId(centerArchId, fieldIds[field]));
            Assert.AreEqual(
                centerArchId,
                EntArchGraph<HighDegreeArch>.GetTransitionArchId(dstArchIds[field], fieldIds[field]));
        }

        for (int field = 0; field < fieldCount; field++)
            UnsetAt(ent, field);
    }

    private static void SetAll(EntMut ent)
    {
        for (int field = 0; field < 10; field++)
            SetAt(ent, field);
    }

    private static void SetAt(EntMut ent, int field)
    {
        switch (field)
        {
            case 0: ent.SetArchetypal<int, F0, HighDegreeArch>(field); break;
            case 1: ent.SetArchetypal<int, F1, HighDegreeArch>(field); break;
            case 2: ent.SetArchetypal<int, F2, HighDegreeArch>(field); break;
            case 3: ent.SetArchetypal<int, F3, HighDegreeArch>(field); break;
            case 4: ent.SetArchetypal<int, F4, HighDegreeArch>(field); break;
            case 5: ent.SetArchetypal<int, F5, HighDegreeArch>(field); break;
            case 6: ent.SetArchetypal<int, F6, HighDegreeArch>(field); break;
            case 7: ent.SetArchetypal<int, F7, HighDegreeArch>(field); break;
            case 8: ent.SetArchetypal<int, F8, HighDegreeArch>(field); break;
            case 9: ent.SetArchetypal<int, F9, HighDegreeArch>(field); break;
        }
    }

    private static void UnsetAt(EntMut ent, int field)
    {
        switch (field)
        {
            case 0: ent.UnsetArchetypal<int, F0, HighDegreeArch>(); break;
            case 1: ent.UnsetArchetypal<int, F1, HighDegreeArch>(); break;
            case 2: ent.UnsetArchetypal<int, F2, HighDegreeArch>(); break;
            case 3: ent.UnsetArchetypal<int, F3, HighDegreeArch>(); break;
            case 4: ent.UnsetArchetypal<int, F4, HighDegreeArch>(); break;
            case 5: ent.UnsetArchetypal<int, F5, HighDegreeArch>(); break;
            case 6: ent.UnsetArchetypal<int, F6, HighDegreeArch>(); break;
            case 7: ent.UnsetArchetypal<int, F7, HighDegreeArch>(); break;
            case 8: ent.UnsetArchetypal<int, F8, HighDegreeArch>(); break;
            case 9: ent.UnsetArchetypal<int, F9, HighDegreeArch>(); break;
        }
    }

    private static int FieldIdAt(int field) => field switch
    {
        0 => EntArchColumn<int, F0, HighDegreeArch>.FieldId,
        1 => EntArchColumn<int, F1, HighDegreeArch>.FieldId,
        2 => EntArchColumn<int, F2, HighDegreeArch>.FieldId,
        3 => EntArchColumn<int, F3, HighDegreeArch>.FieldId,
        4 => EntArchColumn<int, F4, HighDegreeArch>.FieldId,
        5 => EntArchColumn<int, F5, HighDegreeArch>.FieldId,
        6 => EntArchColumn<int, F6, HighDegreeArch>.FieldId,
        7 => EntArchColumn<int, F7, HighDegreeArch>.FieldId,
        8 => EntArchColumn<int, F8, HighDegreeArch>.FieldId,
        _ => EntArchColumn<int, F9, HighDegreeArch>.FieldId,
    };

    private readonly record struct F0;
    private readonly record struct F1;
    private readonly record struct F2;
    private readonly record struct F3;
    private readonly record struct F4;
    private readonly record struct F5;
    private readonly record struct F6;
    private readonly record struct F7;
    private readonly record struct F8;
    private readonly record struct F9;
    private readonly record struct HighDegreeArch;
}
