namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests the keyed strict state tracker.
/// </summary>
[TestClass]
public class GlStateMapTest
{
    [TestMethod]
    public void NewMap_HasNoEntries()
    {
        var map = new GlStateMap<int, int>();

        Assert.IsFalse(map.HasAny);
        Assert.IsFalse(map.IsSet(1));
    }

    [TestMethod]
    public void Set_NewKey_StoresValue()
    {
        var map = new GlStateMap<int, int>();

        map.Set("Fn", 1, 5);

        Assert.IsTrue(map.HasAny);
        Assert.IsTrue(map.IsSet(1));
    }

    [TestMethod]
    public void Set_ExistingKey_Throws()
    {
        var map = new GlStateMap<int, int>();
        map.Set("Fn", 1, 5);

        Assert.Throws<GlAlreadySetException>(() => map.Set("Fn", 1, 6));
    }

    [TestMethod]
    public void Reset_SetKey_RemovesValue()
    {
        var map = new GlStateMap<int, int>();
        map.Set("Fn", 1, 5);

        map.Reset("Fn", 1);

        Assert.IsFalse(map.HasAny);
        Assert.IsFalse(map.IsSet(1));
    }

    [TestMethod]
    public void Reset_MissingKey_Throws()
    {
        var map = new GlStateMap<int, int>();

        Assert.Throws<GlAlreadyUnsetException>(() => map.Reset("Fn", 1));
    }

    [TestMethod]
    public void Set_DifferentKeys_DoNotConflict()
    {
        var map = new GlStateMap<int, int>();

        map.Set("Fn", 1, 5);
        map.Set("Fn", 2, 6);

        Assert.IsTrue(map.IsSet(1));
        Assert.IsTrue(map.IsSet(2));
    }
}
