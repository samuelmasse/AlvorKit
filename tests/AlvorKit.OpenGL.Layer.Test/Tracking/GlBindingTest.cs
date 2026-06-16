namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlBindingTest
{
    /// <summary>A fresh binding accepts a nonzero object and reports only that object as bound.</summary>
    [TestMethod]
    public void Bind_FromZero_Succeeds()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        Assert.AreEqual(1u, binding.Current);
        Assert.IsTrue(binding.IsBound(1));
        Assert.IsFalse(binding.IsBound(0));
    }

    /// <summary>An occupied binding rejects another bind until it is released.</summary>
    [TestMethod]
    public void Bind_WhenOccupied_Throws()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        Assert.Throws<GlAlreadyBoundException>(() => binding.Bind("Fn", 2));
    }

    /// <summary>Unbinding a live slot returns it to the zero object.</summary>
    [TestMethod]
    public void Unbind_AfterBind_Releases()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        binding.Unbind("Fn");
        Assert.AreEqual(0u, binding.Current);
    }

    /// <summary>Unbinding an empty slot reports that no object is bound.</summary>
    [TestMethod]
    public void Unbind_WhenNothingBound_Throws()
    {
        var binding = new GlBinding();
        Assert.Throws<GlNotBoundException>(() => binding.Unbind("Fn"));
    }

    /// <summary>A released binding can accept a later object.</summary>
    [TestMethod]
    public void Rebind_AfterUnbind_Succeeds()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        binding.Unbind("Fn");
        binding.Bind("Fn", 2);
        Assert.AreEqual(2u, binding.Current);
    }
}
