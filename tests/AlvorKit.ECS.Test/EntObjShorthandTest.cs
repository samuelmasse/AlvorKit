namespace AlvorKit.ECS.Test;

[TestClass]
public class EntObjShorthandTest
{
    /// <summary>Verifies EntObj Shorthand Works.</summary>
    [TestMethod]
    public void EntObj_Shorthand_Works()
    {
        var ent = new EntObj();

        Assert.IsNotNull(typeof(EntTestComponents));
        Assert.IsNotNull(typeof(EntTestComponents.First));
        Assert.IsNotNull(typeof(EntTestComponents.Second));
        Assert.IsNotNull(typeof(EntTestComponents.Third));

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(ent.HasSecond);
        Assert.IsFalse(ent.HasThird);

        Assert.AreEqual(default, ent.First);
        Assert.AreEqual(default, ent.Second);
        Assert.AreEqual(default, ent.Third);

        ent.First = 42;
        ent.Second = 6634;
        ent.Third = "SWDSD";

        Assert.IsTrue(ent.HasFirst);
        Assert.IsTrue(ent.HasSecond);
        Assert.IsTrue(ent.HasThird);

        Assert.AreEqual(42, ent.First);
        Assert.AreEqual(6634, ent.Second);
        Assert.AreEqual("SWDSD", ent.Third);

        ent.Mutate()
            .First(default)
            .Second(default)
            .Third(default);

        Assert.AreEqual(default, ent.First);
        Assert.AreEqual(default, ent.Second);
        Assert.AreEqual(default, ent.Third);

        Assert.IsTrue(ent.HasFirst);
        Assert.IsTrue(ent.HasSecond);
        Assert.IsTrue(ent.HasThird);

        ent.UnsetFirst();
        ent.UnsetSecond();
        ent.UnsetThird();

        Assert.IsFalse(ent.HasFirst);
        Assert.IsFalse(ent.HasSecond);
        Assert.IsFalse(ent.HasThird);
    }
}
