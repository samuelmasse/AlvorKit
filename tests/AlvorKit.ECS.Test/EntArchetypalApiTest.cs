namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchetypalApiTest
{
    /// <summary>Every public Ent shape and the interface defaults route archetypal operations to the same component storage.</summary>
    [TestMethod]
    public void ArchetypalApi_AllEntShapes_SharePointAccessSemantics()
    {
        using var arena = new EntArena();
        EntPtr ptr = arena.Alloc();
        EntMut mut = ptr;
        Ent value = mut;
        EntRef readRef = ptr;
        EntRefMut mutRef = ptr;

        ptr.SetArchetypal<int, ValueField, ApiArch>(10);
        Assert.IsTrue(ptr.HasArchetypal<int, ValueField, ApiArch>());
        Assert.AreEqual(10, value.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(value.HasArchetypal<int, ValueField, ApiArch>());
        Assert.AreEqual(10, readRef.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(readRef.HasArchetypal<int, ValueField, ApiArch>());

        mutRef.SetArchetypal<int, ValueField, ApiArch>(20);
        Assert.AreEqual(20, mutRef.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mutRef.HasArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mutRef.UnsetArchetypal<int, ValueField, ApiArch>());

        IEntMut mutAdapter = new EntMutAdapter(ptr.Handle);
        mutAdapter.SetArchetypal<int, ValueField, ApiArch>(30);
        IEnt readAdapter = new EntReadAdapter(ptr.Handle);
        Assert.AreEqual(30, readAdapter.GetArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(readAdapter.HasArchetypal<int, ValueField, ApiArch>());
        Assert.IsTrue(mutAdapter.UnsetArchetypal<int, ValueField, ApiArch>());

        var obj = new EntObj();
        obj.SetArchetypal<int, ValueField, ObjApiArch>(40);
        Assert.AreEqual(40, obj.GetArchetypal<int, ValueField, ObjApiArch>());
        Assert.IsTrue(obj.HasArchetypal<int, ValueField, ObjApiArch>());
        Assert.IsTrue(obj.UnsetArchetypal<int, ValueField, ObjApiArch>());
        GC.KeepAlive(obj);
    }

    private readonly record struct EntReadAdapter(EntHandle Handle) : IEnt
    {
        public T? Get<T, N>() => new EntMut(Handle.Index, Handle.Generation).Get<T, N>();

        public bool Has<T, N>() => new EntMut(Handle.Index, Handle.Generation).Has<T, N>();
    }

    private readonly record struct EntMutAdapter(EntHandle Handle) : IEntMut
    {
        public bool IsAlive => Handle.IsAlive;

        public T? Get<T, N>() => new EntMut(Handle.Index, Handle.Generation).Get<T, N>();

        public bool Has<T, N>() => new EntMut(Handle.Index, Handle.Generation).Has<T, N>();

        public void Set<T, N>(in T value) => new EntMut(Handle.Index, Handle.Generation).Set<T, N>(value);

        public bool Unset<T, N>() => new EntMut(Handle.Index, Handle.Generation).Unset<T, N>();
    }

    private readonly record struct ValueField;
    private readonly record struct ApiArch;
    private readonly record struct ObjApiArch;
}
