namespace AlvorKit;

[TestClass]
public class EntIdxArchetypalRegistrationTest
{
    /// <summary>Archetypal components cannot register write observers or act as bag markers or gates.</summary>
    [TestMethod]
    public void Registration_RejectsArchetypalStorage()
    {
        using var context = new EntIdxContext();
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.OnWrite<int, EntIdxTestComponents.ArchValue>(ent => Assert.Fail()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddIndex(ent => { }).OnWrite<int, EntIdxTestComponents.ArchValue>(ent => Assert.Fail()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddBag(new EntIdxBag<EntIdxTestComponents.IsArchThing>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddGatedBag(new EntIdxGatedBag<EntIdxTestComponents.IsArchThing, EntIdxTestComponents.IsReady>()));
        Assert.ThrowsExactly<EntIdxRegistrationException>(() =>
            context.AddGatedBag(new EntIdxGatedBag<EntIdxTestComponents.IsThing, EntIdxTestComponents.IsArchThing>()));
    }

    /// <summary>Mixed Ents retain unobserved archetypal access and expose both storage kinds before teardown.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void MixedEnt_TeardownReadsBothStorageKinds(bool dispose)
    {
        using var context = new EntIdxContext();
        int writes = 0;
        int observed = 0;
        context.OnWrite<int, EntIdxTestComponents.Value>(ent => writes++);
        context.OnClearing(ent => observed = ent.ArchValue + ent.Value);
        context.OnDisposing(ent => observed = ent.ArchValue + ent.Value);
        using var arena = new EntIdxArena(context);
        var ptr = arena.Alloc();
        EntMutIdx ent = ptr;
        ent.ArchValue = 10;
        Assert.IsTrue(ent.UnsetArchValue());
        ent.ArchValue = 20;
        ent.Value = 5;

        if (dispose)
            ptr.Dispose();
        else ClearGeneric(ent);

        Assert.AreEqual(25, observed);
        Assert.AreEqual(1, writes);
        Assert.AreEqual(!dispose, ent.IsAlive);
        Assert.IsFalse(ent.HasArchValue);
        Assert.IsFalse(ent.HasValue);
    }

    private static void ClearGeneric<T>(T ent) where T : IEntMut => ent.Clear();
}
