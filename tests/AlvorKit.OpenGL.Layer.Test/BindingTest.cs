namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class BindingTest
{
    [TestMethod]
    public void Bind_FromZero_Succeeds()
    {
        var binding = new Binding();
        binding.Bind("Fn", 1);
        Assert.AreEqual(1u, binding.Current);
    }

    [TestMethod]
    public void Bind_WhenOccupied_Throws()
    {
        var binding = new Binding();
        binding.Bind("Fn", 1);
        Assert.Throws<GlAlreadyBoundException>(() => binding.Bind("Fn", 2));
    }

    [TestMethod]
    public void BindZero_AfterBind_Releases()
    {
        var binding = new Binding();
        binding.Bind("Fn", 1);
        binding.Bind("Fn", 0);
        Assert.AreEqual(0u, binding.Current);
    }

    [TestMethod]
    public void BindZero_WhenNothingBound_Throws()
    {
        var binding = new Binding();
        Assert.Throws<GlNotBoundException>(() => binding.Bind("Fn", 0));
    }

    [TestMethod]
    public void BindZero_WhenZeroIsValid_RepeatedlyAllowed()
    {
        var binding = new Binding(zeroIsValid: true);
        binding.Bind("Fn", 0);
        binding.Bind("Fn", 0);
        Assert.AreEqual(0u, binding.Current);
    }

    [TestMethod]
    public void Rebind_AfterRelease_Succeeds()
    {
        var binding = new Binding();
        binding.Bind("Fn", 1);
        binding.Bind("Fn", 0);
        binding.Bind("Fn", 2);
        Assert.AreEqual(2u, binding.Current);
    }

    [TestMethod]
    public void Begin_WhenActive_Throws()
    {
        var binding = new Binding();
        binding.Begin("Fn", 1);
        Assert.Throws<GlAlreadyBoundException>(() => binding.Begin("Fn", 2));
    }

    [TestMethod]
    public void End_WhenNotActive_Throws()
    {
        var binding = new Binding();
        Assert.Throws<GlNotBoundException>(() => binding.End("Fn"));
    }

    [TestMethod]
    public void BeginThenEnd_Succeeds()
    {
        var binding = new Binding();
        binding.Begin("Fn", 1);
        binding.End("Fn");
        Assert.AreEqual(0u, binding.Current);
    }
}
