namespace AlvorKit;

[TestClass]
public class EntToStringTest
{
    /// <summary>Verifies Ent ToString Works.</summary>
    [TestMethod]
    public void Ent_ToString_Works()
    {
        using var entPtr = new EntPtr();

        IEntMut[] entMuts = [entPtr, (EntMut)entPtr];

        IEnt[] ents =
        [
            entPtr, (Ent)entPtr, (EntMut)entPtr,
        ];

        foreach (var ent in ents)
            Assert.AreEqual("Ent { }", ent.ToString());

        foreach (var ent in entMuts)
            ent.First = 34;
        foreach (var ent in ents)
            Assert.AreEqual("Ent { First = 34 }", ent.ToString());

        // Does not appear in ToString because it lacks ComponentToString
        foreach (var ent in entMuts)
            ent.Second = 46;
        foreach (var ent in ents)
            Assert.AreEqual("Ent { First = 34 }", ent.ToString());

        foreach (var ent in entMuts)
            ent.Third = null;
        foreach (var ent in ents)
            Assert.AreEqual("Ent { First = 34, Third =  }", ent.ToString());

        EntPtr nullPtr = default;
        Assert.AreEqual("Ent Null", nullPtr.ToString());

        var ptr = new EntPtr();
        ptr.Dispose();
        Assert.AreEqual("Ent Disposed", ptr.ToString());
    }

    /// <summary>Verifies Ent ToString HandlesCycles.</summary>
    [TestMethod]
    public void Ent_ToString_HandlesCycles()
    {
        using var ent1 = new EntPtr();
        using var ent2 = new EntPtr();

        ent1.Mutate().MyEnt(ent2);
        ent2.Mutate().MyEnt(ent1);

        Assert.AreEqual("Ent { MyEnt = Ent { MyEnt = Ent { ... } } }", ent1.ToString());
    }
}
