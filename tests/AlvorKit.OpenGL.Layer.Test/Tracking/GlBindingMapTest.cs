namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlBindingMapTest
{
    /// <summary>A fresh keyed binding accepts one object and reports it through direct and value lookups.</summary>
    [TestMethod]
    public void Bind_NewKey_Succeeds()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.IsTrue(map.TryGet(1, out var value));
        Assert.AreEqual(5u, value);
        Assert.AreEqual(5u, map.Bound[1]);
        Assert.IsTrue(map.ContainsValue(5));
        Assert.IsFalse(map.ContainsValue(0));
    }

    /// <summary>An occupied key rejects another object until that key is released.</summary>
    [TestMethod]
    public void Bind_OccupiedKey_Throws()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        Assert.Throws<GlAlreadyBoundException>(() => map.Bind("Fn", 1, 6));
    }

    /// <summary>Unbinding a live key removes its recorded object.</summary>
    [TestMethod]
    public void Unbind_BoundKey_Removes()
    {
        var map = new GlBindingMap<int>();
        map.Bind("Fn", 1, 5);
        map.Unbind("Fn", 1);
        Assert.IsFalse(map.TryGet(1, out _));
    }

    /// <summary>Unbinding an empty key reports that no object is bound there.</summary>
    [TestMethod]
    public void Unbind_UnboundKey_Throws()
    {
        var map = new GlBindingMap<int>();
        Assert.Throws<GlNotBoundException>(() => map.Unbind("Fn", 1));
    }

    /// <summary>Different keys can hold different objects without conflicting.</summary>
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

    /// <summary>A released key can accept a later object.</summary>
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

}
