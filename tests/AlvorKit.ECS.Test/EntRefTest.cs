namespace AlvorKit.ECS.Test;

[TestClass]
public class EntRefTest
{
    /// <summary>Verifies EntRef Equality Works.</summary>
    [TestMethod]
    public void EntRef_Equality_Works()
    {
        EntRef ent1 = new EntObj();
        EntRef ent2 = new EntObj();

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies EntRef OperatorObj Works.</summary>
    [TestMethod]
    public void EntRef_OperatorObj_Works()
    {
        var entObj = new EntObj();
        EntRef entRef = entObj;

        Ent ent = (Ent)entRef;

        Assert.IsTrue(entRef == entObj);
        Assert.IsTrue((Ent)entRef == ent);
    }

    /// <summary>Verifies EntRef OperatorPtr Works.</summary>
    [TestMethod]
    public void EntRef_OperatorPtr_Works()
    {
        using var entPtr = new EntPtr();
        EntRef entRef = entPtr;

        Ent ent = (Ent)entRef;

        Assert.IsTrue(entRef == entPtr);
        Assert.IsTrue((Ent)entRef == ent);
    }

    /// <summary>Verifies EntRef GetHas Works.</summary>
    [TestMethod]
    public void EntRef_GetHas_Works()
    {
        using var entPtr = new EntPtr();
        EntRef ent = entPtr;

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(entPtr.UnsetFirst());

        entPtr.Mutate().First(42);
        Assert.IsTrue(ent.HasFirst);
        Assert.AreEqual(42, ent.First);
        Assert.IsTrue(entPtr.UnsetFirst());
        Assert.IsFalse(ent.HasFirst);

        entPtr.Mutate().First(3);
        Assert.AreEqual(3, ent.First);
        Assert.IsTrue(ent.HasFirst);

        entPtr.Clear();
        Assert.IsFalse(ent.HasFirst);
        Assert.AreEqual(default, entPtr.First);
    }

    /// <summary>Verifies EntRef KeepsEntObjAlive.</summary>
    [TestMethod]
    public void EntRef_KeepsEntObjAlive()
    {
        Ent ent;
        EntRef entRef;

        WeakReference SetEnt()
        {
            var entObj = new EntObj()
            {
                First = 55
            };

            ent = (EntMut)entObj;
            Assert.AreEqual(55, ent.First);

            return new(entObj);
        }

        WeakReference SetEntRef()
        {
            var entObj = new EntObj()
            {
                First = 56
            };

            entRef = entObj;
            Assert.AreEqual(56, entRef.First);

            return new(entObj);
        }

        var wr = SetEnt();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(wr.IsAlive); // Ent did not keep the object alive and it was collected
        Assert.AreEqual(default, ent.First);

        wr = SetEntRef();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(wr.IsAlive); // Because EntRef keeps a reference the object was not collected
        Assert.AreEqual(56, entRef.First);
    }
}
