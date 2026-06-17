namespace AlvorKit.ECS.Test;

[TestClass]
public class EntObjTest
{
    /// <summary>Verifies EntObj Equality Works.</summary>
    [TestMethod]
    public void EntObj_Equality_Works()
    {
        var ent1 = new EntObj();
        var ent2 = new EntObj();

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies EntObj Operator Works.</summary>
    [TestMethod]
    public void EntObj_Operator_Works()
    {
        var entObj = new EntObj();

        Ent ent = (Ent)entObj;
        EntMut entMut = (EntMut)entObj;
        EntRef entRef = entObj;
        EntRefMut entRefMut = entObj;

        Assert.IsTrue((Ent)entObj == ent);
        Assert.IsTrue((Ent)entObj == entMut);
        Assert.IsTrue(entObj == entRef);
        Assert.IsTrue(entObj == entRefMut);
    }

    /// <summary>Verifies EntObj GetSetUnsetHasClear Works.</summary>
    [TestMethod]
    public void EntObj_GetSetUnsetHasClear_Works()
    {
        var ent = new EntObj();

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(ent.UnsetFirst());

        ent.First = 42;
        Assert.IsTrue(ent.HasFirst);
        Assert.AreEqual(42, ent.First);
        Assert.IsTrue(ent.UnsetFirst());
        Assert.IsFalse(ent.HasFirst);

        ent.First = 3;
        Assert.AreEqual(3, ent.First);
        Assert.IsTrue(ent.HasFirst);

        ent.Clear();
        Assert.IsFalse(ent.HasFirst);
        Assert.AreEqual(default, ent.First);
    }

    /// <summary>Verifies EntObj Clear Works.</summary>
    [TestMethod]
    public void EntObj_Clear_Works()
    {
        var ent = new EntObj();

        ent.Set<int, FirstComponent>(343);
        ent.Set<int, SecondComponent>(65);

        Assert.AreEqual(343, ent.Get<int, FirstComponent>());
        Assert.AreEqual(65, ent.Get<int, SecondComponent>());

        ent.Clear();

        Assert.AreEqual(default, ent.Get<int, FirstComponent>());
        Assert.AreEqual(default, ent.Get<int, SecondComponent>());
    }

    /// <summary>Verifies EntObj GetUnsetComponent ReturnsDefault.</summary>
    [TestMethod]
    public void EntObj_GetUnsetComponent_ReturnsDefault()
    {
        var ent = new EntObj();

        Assert.AreEqual(default, ent.Get<int, FirstComponent>());
    }

    /// <summary>Verifies EntObj OutOfScope ComponentsAreCleared.</summary>
    [TestMethod]
    public void EntObj_OutOfScope_ComponentsAreCleared()
    {
        int pageIndex;
        int subIndex;

        WeakReference ctr()
        {
            var ent = new EntObj().Mutate().Set<string, FirstComponent>("Hello").Ent;

            pageIndex = ent.Index / EntReg.PageSize;
            subIndex = ent.Index % EntReg.PageSize;

            ref var storage = ref EntStorage<string, FirstComponent>.Sparse[pageIndex]![subIndex];
            Assert.AreNotEqual(0, storage.Generation);
            Assert.AreEqual("Hello", storage.Value);

            return new(ent);
        }

        var wr = ctr();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        ref var storage = ref EntStorage<string, FirstComponent>.Sparse[pageIndex]![subIndex];
        Assert.IsFalse(wr.IsAlive);
        Assert.AreEqual(0, storage.Generation);
        Assert.IsNull(storage.Value);
    }

    /// <summary>Verifies EntObj SetComponent OnlyAffectsCorrectEnt.</summary>
    [TestMethod]
    public void EntObj_SetComponent_OnlyAffectsCorrectEnt()
    {
        var ent1 = new EntObj();
        var ent2 = new EntObj();

        ent1.Set<int, FirstComponent>(100);
        ent2.Set<int, FirstComponent>(200);

        Assert.AreEqual(100, ent1.Get<int, FirstComponent>());
        Assert.AreEqual(200, ent2.Get<int, FirstComponent>());
    }

    /// <summary>Verifies EntObj SetComponent DoesNotAffectOtherComponentTypes.</summary>
    [TestMethod]
    public void EntObj_SetComponent_DoesNotAffectOtherComponentTypes()
    {
        var ent = new EntObj();
        ent.Set<int, FirstComponent>(42);
        ent.Set<float, SecondComponent>(3.14f);

        Assert.AreEqual(42, ent.Get<int, FirstComponent>());
        Assert.AreEqual(3.14f, ent.Get<float, SecondComponent>());
    }

    /// <summary>Verifies EntObj SetComponenReuseName Works.</summary>
    [TestMethod]
    public void EntObj_SetComponenReuseName_Works()
    {
        var ent = new EntObj();
        ent.Set<int, FirstComponent>(11);
        ent.Set<float, FirstComponent>(111);

        Assert.AreEqual(11, ent.Get<int, FirstComponent>());
        Assert.AreEqual(111, ent.Get<float, FirstComponent>());
    }

    /// <summary>Verifies EntObj SetComponenReuseType Works.</summary>
    [TestMethod]
    public void EntObj_SetComponenReuseType_Works()
    {
        var ent = new EntObj();
        ent.Set<float, FirstComponent>(22);
        ent.Set<float, SecondComponent>(222);

        Assert.AreEqual(22, ent.Get<float, FirstComponent>());
        Assert.AreEqual(222, ent.Get<float, SecondComponent>());
    }

    /// <summary>Verifies EntObj SetComponent NullableType Works.</summary>
    [TestMethod]
    public void EntObj_SetComponent_NullableType_Works()
    {
        var ent = new EntObj();

        ent.Set<int?, FirstComponent>(null);
        ent.Set<int?, SecondComponent>(42);

        Assert.IsNull(ent.Get<int?, FirstComponent>());
        Assert.AreEqual(42, ent.Get<int?, SecondComponent>());
    }

    /// <summary>Verifies EntObj SetComponent ValueTuple Works.</summary>
    [TestMethod]
    public void EntObj_SetComponent_ValueTuple_Works()
    {
        var ent = new EntObj();

        ent.Set<(int, float), FirstComponent>((10, 5.5f));

        Assert.AreEqual((10, 5.5f), ent.Get<(int, float), FirstComponent>());
    }
}
