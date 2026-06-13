namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class BindingMapTest
{
    [TestMethod]
    public void Bind_NewKey_Succeeds()
    {
        var map = new BindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.IsTrue(map.TryGet(1, out var value));
        Assert.AreEqual(5u, value);
    }

    [TestMethod]
    public void Bind_OccupiedKey_Throws()
    {
        var map = new BindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.Throws<GlAlreadyBoundException>(() => map.Bind("Fn", 1, 6));
    }

    [TestMethod]
    public void BindZero_BoundKey_Removes()
    {
        var map = new BindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Bind("Fn", 1, 0);
        Assert.IsFalse(map.TryGet(1, out _));
    }

    [TestMethod]
    public void BindZero_UnboundKey_Throws()
    {
        var map = new BindingMap<int>();
        Assert.Throws<GlNotBoundException>(() => map.Bind("Fn", 1, 0));
    }

    [TestMethod]
    public void DifferentKeys_DoNotConflict()
    {
        var map = new BindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Bind("Fn", 2, 6);
        map.TryGet(1, out var first);
        map.TryGet(2, out var second);
        Assert.AreEqual(5u, first);
        Assert.AreEqual(6u, second);
    }

    [TestMethod]
    public void Begin_OccupiedKey_Throws()
    {
        var map = new BindingMap<int>();
        map.Begin("Fn", 1, 5);
        Assert.Throws<GlAlreadyBoundException>(() => map.Begin("Fn", 1, 6));
    }

    [TestMethod]
    public void End_UnboundKey_Throws()
    {
        var map = new BindingMap<int>();
        Assert.Throws<GlNotBoundException>(() => map.End("Fn", 1));
    }
}
