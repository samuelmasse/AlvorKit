namespace AlvorKit.ECS.Test;

[TestClass]
public class EntPtrTest
{
    /// <summary>Verifies EntPtr Equality Works.</summary>
    [TestMethod]
    public void EntPtr_Equality_Works()
    {
        using var ent1 = new EntPtr();
        using var ent2 = new EntPtr();

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies EntPtr Operator Works.</summary>
    [TestMethod]
    public void EntPtr_Operator_Works()
    {
        using var entPtr = new EntPtr();

        Ent ent = entPtr;
        EntMut entMut = entPtr;
        EntRef entRef = entPtr;
        EntRefMut entRefMut = entPtr;

        Assert.IsTrue(entPtr == ent);
        Assert.IsTrue(entPtr == entMut);
        Assert.IsTrue(entPtr == entRef);
        Assert.IsTrue(entPtr == entRefMut);
    }

    /// <summary>Verifies EntPtr GetSetUnsetHasClear Works.</summary>
    [TestMethod]
    public void EntPtr_GetSetUnsetHasClear_Works()
    {
        using var ent = new EntPtr();

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(ent.UnsetFirst());

        ent.Mutate().First(42);
        Assert.IsTrue(ent.HasFirst);
        Assert.AreEqual(42, ent.First);
        Assert.IsTrue(ent.UnsetFirst());
        Assert.IsFalse(ent.HasFirst);

        ent.Mutate().First(3);
        Assert.AreEqual(3, ent.First);
        Assert.IsTrue(ent.HasFirst);

        ent.Clear();
        Assert.IsFalse(ent.HasFirst);
        Assert.AreEqual(default, ent.First);
    }

    /// <summary>Verifies EntPtr Dispose ClearsAndMakeIndexAvailable.</summary>
    [TestMethod]
    public void EntPtr_Dispose_ClearsAndMakeIndexAvailable()
    {
        var ent = new EntPtr()
        {
            First = 21
        };
        Assert.AreEqual(21, ent.First);

        ent.Dispose();
        Assert.IsFalse(ent.HasFirst);

        EntPtr ent2 = default;
        for (int i = 0; i < 9000; i++)
        {
            ent2 = new EntPtr();
            if (ent.Index == ent2.Index)
                break;
        }

        Assert.AreEqual(ent2.Index, ent.Index); // index got reused
        Assert.IsFalse(ent2 == ent); // but they are not equal

        ent2.First = 36; // using new generation is allowed
        Assert.AreEqual(36, ent2.First);

        ent.Second = 25; // but old generation cannot be used
        Assert.AreEqual(0, ent.Second); // has no effect
    }

    /// <summary>Verifies EntPtr Default Does Not Throw.</summary>
    [TestMethod]
    public void EntPtr_Default_Does_Not_Throw()
    {
        EntPtr ent = default;

        Assert.IsFalse(ent.IsAlive);
        Assert.IsFalse(ent.UnsetFirst());
        ent.First = 343;
        Assert.AreEqual(0, ent.First); // edit has not effect

        // Readonly operations are allowed on null EntPtr
        Assert.AreEqual(0, ent.First);
        Assert.IsFalse(ent.HasFirst);
    }

    /// <summary>Verifies EntPtr UseAfterFree HasNoEffect.</summary>
    [TestMethod]
    public void EntPtr_UseAfterFree_HasNoEffect()
    {
        var ent = new EntPtr()
        {
            First = 124
        };

        ent.Dispose();

        ent.UnsetFirst();
        ent.First = 343;
        Assert.AreEqual(0, ent.First); // has no effect

        // Readonly operations are allowed on disposed EntPtr
        Assert.AreEqual(0, ent.First);
        Assert.IsFalse(ent.HasFirst);
    }
}
