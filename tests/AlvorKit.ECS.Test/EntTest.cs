namespace AlvorKit.ECS.Test;

[TestClass]
public class EntTest
{
    /// <summary>Verifies Ent Equality Works.</summary>
    [TestMethod]
    public void Ent_Equality_Works()
    {
        using var ptr1 = new EntPtr();
        using var ptr2 = new EntPtr();

        Ent ent1 = ptr1;
        Ent ent2 = ptr2;

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies Ent GetHas Works.</summary>
    [TestMethod]
    public void Ent_GetHas_Works()
    {
        var entPtr = new EntPtr();
        Ent ent = entPtr;

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(entPtr.UnsetFirst());

        entPtr.First = 42;
        Assert.IsTrue(ent.HasFirst);
        Assert.AreEqual(42, ent.First);
        Assert.IsTrue(entPtr.UnsetFirst());
        Assert.IsFalse(ent.HasFirst);

        entPtr.First = 3;
        Assert.AreEqual(3, ent.First);
        Assert.IsTrue(ent.HasFirst);

        entPtr.Clear();
        Assert.IsFalse(ent.HasFirst);
        Assert.AreEqual(default, entPtr.First);

        entPtr.Dispose();
    }
}
