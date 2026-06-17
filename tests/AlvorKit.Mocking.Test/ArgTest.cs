namespace AlvorKit.Mocking.Test;

[TestClass]
public class ArgTest
{
    /// <summary>Verifies Arg Any ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_Any_ReturnsEmpty()
    {
        Assert.AreEqual(0, Arg.Any<int>());
        Assert.IsNull(Arg.Any<string>());
        Assert.IsNull(Arg.Any<List<string>>());
    }

    /// <summary>Verifies Arg Match ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_Match_ReturnsEmpty()
    {
        Assert.AreEqual(0, Arg.Match<int>((x) => true));
        Assert.IsNull(Arg.Match<string>((x) => true));
        Assert.IsNull(Arg.Match<List<string>>((x) => true));
    }

    /// <summary>Verifies Arg RefAny ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_RefAny_ReturnsEmpty()
    {
        ref var i = ref Arg<int>.Any();
        ref var s = ref Arg<string>.Any();
        ref var l = ref Arg<List<string>>.Any();

        Assert.AreEqual(0, i);
        Assert.IsNull(s);
        Assert.IsNull(l);
    }

    /// <summary>Verifies Arg RefAnyModified StillReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_RefAnyModified_StillReturnsEmpty()
    {
        ref var i = ref Arg<int>.Any();
        Assert.AreEqual(0, i);

        i = 35;

        ref var i2 = ref Arg<int>.Any();
        Assert.AreEqual(0, i);
        Assert.AreEqual(0, i2);
    }

    /// <summary>Verifies Arg RefMatch ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_RefMatch_ReturnsEmpty()
    {
        ref var i = ref Arg<int>.Match((x) => true);
        ref var s = ref Arg<string>.Match((x) => true);
        ref var l = ref Arg<List<string>>.Match((x) => true);

        Assert.AreEqual(0, i);
        Assert.IsNull(s);
        Assert.IsNull(l);
    }
}
