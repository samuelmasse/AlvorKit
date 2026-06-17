namespace AlvorKit.ECS.Test;

[TestClass]
public class EntToStringTest
{
    /// <summary>Verifies Ent ToString Works.</summary>
    [TestMethod]
    public void Ent_ToString_Works()
    {
        var entObj = new EntObj();
        using var entPtr = new EntPtr();

        IEntMut[] entMuts = [entObj, entPtr];

        IEnt[] ents =
        [
            entObj, (Ent)entObj, (EntMut)entObj, (EntRef)entObj, (EntRefMut)entObj,
            entPtr, (Ent)entPtr, (EntMut)entPtr, (EntRef)entPtr, (EntRefMut)entPtr,
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
        var ent1 = new EntObj();
        var ent2 = new EntObj();

        ent1.MyEnt = ent2;
        ent2.MyEnt = ent1;

        Assert.AreEqual("Ent { MyEnt = Ent { MyEnt = Ent { ... } } }", ent1.ToString());
    }
}
