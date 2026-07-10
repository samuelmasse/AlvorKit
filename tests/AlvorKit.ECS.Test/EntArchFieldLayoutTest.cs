namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchFieldLayoutTest
{
    /// <summary>Verifies mixed fields receive one canonical compact layout entry per field membership.</summary>
    [TestMethod]
    public void ArchetypalLayout_MixedFields_StayParallelAndCanonical()
    {
        int byteFieldId = EntArchColumn<byte, B, LayoutArch>.FieldId;
        int firstStringFieldId = EntArchColumn<string, S0, LayoutArch>.FieldId;
        int longFieldId = EntArchColumn<long, L, LayoutArch>.FieldId;
        int firstPayloadFieldId = EntArchColumn<RefPayload, R0, LayoutArch>.FieldId;
        int secondStringFieldId = EntArchColumn<string, S1, LayoutArch>.FieldId;
        int intFieldId = EntArchColumn<int, I, LayoutArch>.FieldId;
        int secondPayloadFieldId = EntArchColumn<RefPayload, R1, LayoutArch>.FieldId;
        int[] expectedFieldIds =
        [
            byteFieldId,
            firstStringFieldId,
            longFieldId,
            firstPayloadFieldId,
            secondStringFieldId,
            intFieldId,
            secondPayloadFieldId,
        ];

        Assert.AreEqual(8, Unsafe.SizeOf<EntArchField>());
        Assert.AreEqual(4, Unsafe.SizeOf<EntArchFieldLayout>());

        using var arena = new EntArena();
        EntMut ent = arena.Alloc();
        SetFieldsReverse(ent);

        int reverseArchId = ent.Get<EntArchLoc, LayoutArch>().ArchId;
        var reverseFieldIds = EntArchGraph<LayoutArch>.FieldIds(reverseArchId).ToArray();
        var reverseLayouts = EntArchGraph<LayoutArch>.FieldLayouts(reverseArchId).ToArray();

        CollectionAssert.AreEqual(expectedFieldIds, reverseFieldIds);
        Assert.AreEqual(reverseFieldIds.Length, reverseLayouts.Length);
        AssertReferenceFree(reverseLayouts[0], Unsafe.SizeOf<EntMut>());
        AssertReferenceContaining(reverseLayouts[1], 0);
        AssertReferenceFree(reverseLayouts[2], Unsafe.SizeOf<EntMut>() + Unsafe.SizeOf<byte>());
        AssertReferenceContaining(reverseLayouts[3], 0);
        AssertReferenceContaining(reverseLayouts[4], 1);
        AssertReferenceFree(
            reverseLayouts[5],
            Unsafe.SizeOf<EntMut>() + Unsafe.SizeOf<byte>() + Unsafe.SizeOf<long>());
        AssertReferenceContaining(reverseLayouts[6], 1);

        UnsetFields(ent);
        SetFieldsForward(ent);

        int forwardArchId = ent.Get<EntArchLoc, LayoutArch>().ArchId;
        Assert.AreEqual(reverseArchId, forwardArchId);
        CollectionAssert.AreEqual(reverseLayouts, EntArchGraph<LayoutArch>.FieldLayouts(forwardArchId).ToArray());

        UnsetFields(ent);
        Assert.IsFalse(ent.Has<EntArchLoc, LayoutArch>());
    }

    private static void SetFieldsReverse(EntMut ent)
    {
        ent.SetArchetypal<RefPayload, R1, LayoutArch>(new(new RefBox(), 6));
        ent.SetArchetypal<int, I, LayoutArch>(5);
        ent.SetArchetypal<string, S1, LayoutArch>("four");
        ent.SetArchetypal<RefPayload, R0, LayoutArch>(new(new RefBox(), 3));
        ent.SetArchetypal<long, L, LayoutArch>(2);
        ent.SetArchetypal<string, S0, LayoutArch>("one");
        ent.SetArchetypal<byte, B, LayoutArch>(0);
    }

    private static void SetFieldsForward(EntMut ent)
    {
        ent.SetArchetypal<byte, B, LayoutArch>(0);
        ent.SetArchetypal<string, S0, LayoutArch>("one");
        ent.SetArchetypal<long, L, LayoutArch>(2);
        ent.SetArchetypal<RefPayload, R0, LayoutArch>(new(new RefBox(), 3));
        ent.SetArchetypal<string, S1, LayoutArch>("four");
        ent.SetArchetypal<int, I, LayoutArch>(5);
        ent.SetArchetypal<RefPayload, R1, LayoutArch>(new(new RefBox(), 6));
    }

    private static void UnsetFields(EntMut ent)
    {
        ent.UnsetArchetypal<byte, B, LayoutArch>();
        ent.UnsetArchetypal<string, S0, LayoutArch>();
        ent.UnsetArchetypal<long, L, LayoutArch>();
        ent.UnsetArchetypal<RefPayload, R0, LayoutArch>();
        ent.UnsetArchetypal<string, S1, LayoutArch>();
        ent.UnsetArchetypal<int, I, LayoutArch>();
        ent.UnsetArchetypal<RefPayload, R1, LayoutArch>();
    }

    private static void AssertReferenceFree(EntArchFieldLayout layout, int bytePrefix)
    {
        Assert.IsFalse(layout.ContainsReferences);
        Assert.AreEqual(bytePrefix, layout.BytePrefix);
    }

    private static void AssertReferenceContaining(EntArchFieldLayout layout, int typeColumn)
    {
        Assert.IsTrue(layout.ContainsReferences);
        Assert.AreEqual(typeColumn, layout.TypeColumn);
    }

    private sealed class RefBox;

    private readonly record struct RefPayload(RefBox? Ref, int Value);
    private readonly record struct B;
    private readonly record struct S0;
    private readonly record struct L;
    private readonly record struct R0;
    private readonly record struct S1;
    private readonly record struct I;
    private readonly record struct R1;
    private readonly record struct LayoutArch;
}
