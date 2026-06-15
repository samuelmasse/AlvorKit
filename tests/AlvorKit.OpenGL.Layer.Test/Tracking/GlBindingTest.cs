namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlBindingTest
{
    [TestMethod]
    public void Bind_FromZero_Succeeds()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        Assert.AreEqual(1u, binding.Current);
    }

    [TestMethod]
    public void Bind_WhenOccupied_Throws()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        Assert.Throws<GlAlreadyBoundException>(() => binding.Bind("Fn", 2));
    }

    [TestMethod]
    public void Unbind_AfterBind_Releases()
    {
        var binding = new GlBinding();
        binding.Bind("Fn", 1);
        binding.Unbind("Fn");
        Assert.AreEqual(0u, binding.Current);
    }

    [TestMethod]
    public void Unbind_WhenNothingBound_Throws()
    {
        var binding = new GlBinding();
        Assert.Throws<GlNotBoundException>(() => binding.Unbind("Fn"));
    }

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
