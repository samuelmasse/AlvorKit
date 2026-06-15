namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlBindingMapTest
{
    [TestMethod]
    public void Bind_NewKey_Succeeds()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.IsTrue(map.TryGet(1, out var value));
        Assert.AreEqual(5u, value);
        Assert.AreEqual(5u, map.Bound[1]);
    }

    [TestMethod]
    public void Bind_OccupiedKey_Throws()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.Throws<GlAlreadyBoundException>(() => map.Bind("Fn", 1, 6));
    }

    [TestMethod]
    public void Unbind_BoundKey_Removes()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Unbind("Fn", 1);
        Assert.IsFalse(map.TryGet(1, out _));
    }

    [TestMethod]
    public void Unbind_UnboundKey_Throws()
    {
        var map = new GlBindingMap<int>();
        Assert.Throws<GlNotBoundException>(() => map.Unbind("Fn", 1));
    }

    [TestMethod]
    public void DifferentKeys_DoNotConflict()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Bind("Fn", 2, 6);
        map.TryGet(1, out var first);
        map.TryGet(2, out var second);
        Assert.AreEqual(5u, first);
        Assert.AreEqual(6u, second);
    }

    [TestMethod]
    public void Rebind_AfterUnbind_Succeeds()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Unbind("Fn", 1);
        map.Bind("Fn", 1, 6);
        Assert.IsTrue(map.TryGet(1, out var value));
        Assert.AreEqual(6u, value);
    }

    [TestMethod]
    public void UnbindWhere_WhenNoKeysMatch_Throws()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);

        Assert.Throws<GlNotBoundException>(() => map.UnbindWhere("Fn", key => key == 2));
    }
}
