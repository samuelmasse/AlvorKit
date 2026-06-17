namespace AlvorKit.ECS.Test;

[TestClass]
public class EntMutTest
{
    /// <summary>Verifies EntMut Equality Works.</summary>
    [TestMethod]
    public void EntMut_Equality_Works()
    {
        using var ptr1 = new EntPtr();
        using var ptr2 = new EntPtr();

        EntMut ent1 = ptr1;
        EntMut ent2 = ptr2;

        Assert.AreNotEqual(ent1, ent2);
        Assert.IsFalse(ent1.Equals(ent2));
        Assert.IsFalse(ent1.Equals((object)ent2));
        Assert.IsFalse(ent1.Equals(new object()));
        Assert.AreNotEqual(ent1.GetHashCode(), ent2.GetHashCode());
    }

    /// <summary>Verifies EntMut Operator Works.</summary>
    [TestMethod]
    public void EntMut_Operator_Works()
    {
        using var entPtr = new EntPtr();
        EntMut entMut = entPtr;

        Ent ent = (Ent)entMut;

        Assert.IsTrue(entMut == ent);
    }

    /// <summary>Verifies EntMut GetSetUnsetHasClear Works.</summary>
    [TestMethod]
    public void EntMut_GetSetUnsetHasClear_Works()
    {
        using var entPtr = new EntPtr();
        EntMut ent = entPtr;

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
}
