namespace AlvorKit;

[TestClass]
public class EntOwnershipTest
{
    /// <summary>Explicit disposal releases sparse and archetypal payloads even when they point back to their owning Ent.</summary>
    [TestMethod]
    public void Dispose_ReleasesPayloadsThatReferenceTheirOwner()
    {
        using var ptr = new EntPtr();
        var retained = AttachOwner(ptr);
        GC.Collect();
        Assert.IsTrue(retained.IsAlive);

        ptr.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(ptr.IsAlive);
        Assert.IsFalse(retained.IsAlive);
        Assert.AreEqual(0, EntArchDiagnostics<OwnerArch>.Capture().ActiveRowCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachOwner(EntPtr ptr)
    {
        var owner = new Owner(ptr);
        ptr.Set<Owner, OwnerField>(owner);
        ptr.SetArchetypal<Owner, OwnerField, OwnerArch>(owner);
        return new(owner);
    }

    private class Owner(EntPtr ent)
    {
        public EntPtr Ent => ent;
    }

    private readonly record struct OwnerField;
    private readonly record struct OwnerArch;
}
