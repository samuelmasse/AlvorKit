namespace AlvorKit;

[TestClass]
public class EntArchetypalApiTest
{
    /// <summary>Concrete and constrained generic Ent access route archetypal operations to the same component storage.</summary>
    [TestMethod]
    public void ArchetypalApi_AllEntShapes_SharePointAccessSemantics()
    {
        using var arena = new EntArena();
        EntPtr ptr = arena.Alloc();
        EntMut mut = ptr;
        Ent value = mut;

        ptr.SetArchetypal<int, ValueField, ApiArch>(10);
        Assert.IsTrue(ptr.HasArchetypal<int, ValueField, ApiArch>());
        Assert.AreEqual(10, value.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(value.HasArchetypal<int, ValueField, ApiArch>());

        mut.SetArchetypal<int, ValueField, ApiArch>(20);
        Assert.AreEqual(20, mut.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mut.HasArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mut.UnsetArchetypal<int, ValueField, ApiArch>());

        VerifyGenericAccess(ptr, value);
        VerifyGenericAccess(mut, value);
    }

    private static void VerifyGenericAccess<TMut, TRead>(TMut mut, TRead read) where TMut : IEntMut where TRead : IEnt
    {
        mut.SetArchetypal<int, ValueField, ApiArch>(30);
        Assert.AreEqual(30, read.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(read.HasArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mut.UnsetArchetypal<int, ValueField, ApiArch>());
    }

    private readonly record struct ValueField;
    private readonly record struct ApiArch;
}
