namespace AlvorKit.ECS.Test;

[TestClass]
public class EntRefMutTest
{
    /// <summary>Verifies EntRefMut Equality Works.</summary>
    [TestMethod]
    public void EntRefMut_Equality_Works()
    {
        EntRefMut ent1 = new EntObj();
        EntRefMut ent2 = new EntObj();

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies EntRefMut OperatorObj Works.</summary>
    [TestMethod]
    public void EntRefMut_OperatorObj_Works()
    {
        var entObj = new EntObj();
        EntRefMut entRefMut = entObj;

        Ent ent = (Ent)entRefMut;
        EntMut entMut = (EntMut)entRefMut;
        EntRef entRef = entRefMut;

        Assert.IsTrue(entRefMut == entObj);
        Assert.IsTrue((Ent)entRefMut == ent);
        Assert.IsTrue((EntMut)entRefMut == entMut);
        Assert.IsTrue(entRefMut == entRef);
    }

    /// <summary>Verifies EntRefMut OperatorPtr Works.</summary>
    [TestMethod]
    public void EntRefMut_OperatorPtr_Works()
    {
        using var entPtr = new EntPtr();
        EntRefMut entRefMut = entPtr;

        Ent ent = (Ent)entRefMut;
        EntMut entMut = (EntMut)entRefMut;
        EntRef entRef = entRefMut;

        Assert.IsTrue(entRefMut == entPtr);
        Assert.IsTrue((Ent)entRefMut == ent);
        Assert.IsTrue((EntMut)entRefMut == entMut);
        Assert.IsTrue(entRefMut == entRef);
    }

    /// <summary>Verifies EntMut GetSetUnsetHasClear Works.</summary>
    [TestMethod]
    public void EntMut_GetSetUnsetHasClear_Works()
    {
        using var entPtr = new EntPtr();
        EntRefMut ent = entPtr;

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

    /// <summary>Verifies EntRefMut KeepsEntObjAlive.</summary>
    [TestMethod]
    public void EntRefMut_KeepsEntObjAlive()
    {
        EntMut entMut;
        EntRefMut entRefMut;

        WeakReference SetEntMut()
        {
            var entObj = new EntObj
            {
                First = 55
            };

            entMut = (EntMut)entObj;
            Assert.AreEqual(55, entMut.First);

            return new(entObj);
        }

        WeakReference SetEntRefMut()
        {
            var entObj = new EntObj
            {
                First = 56
            };

            entRefMut = entObj;
            Assert.AreEqual(56, entRefMut.First);

            return new(entObj);
        }

        var wr = SetEntMut();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(wr.IsAlive); // EntMut did not keep the object alive and it was collected
        Assert.IsFalse(entMut.HasFirst);

        wr = SetEntRefMut();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(wr.IsAlive); // Because EntRefMut keeps a reference the object was not collected
        Assert.AreEqual(56, entRefMut.First);
    }
}
