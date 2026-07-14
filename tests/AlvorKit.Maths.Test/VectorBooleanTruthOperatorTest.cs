namespace AlvorKit.Maths.Test;

/// <summary>Tests Boolean-vector truth and short-circuit operators.</summary>
[TestClass]
public sealed class VectorBooleanTruthOperatorTest
{
    /// <summary>Truth operators preserve all/none semantics across dimensions and mixed masks.</summary>
    [TestMethod]
    public void TruthOperators_PreserveAllAndNoneSemantics()
    {
        Assert.IsTrue(new Vec2b(true, true) ? true : false);
        Assert.IsFalse(new Vec2b(true, false) ? true : false);
        Assert.IsTrue((new Vec3b(false, false, false) && new Vec3b(true)).None);
        Assert.IsFalse((new Vec3b(false, true, false) && new Vec3b(true)).None);
        Assert.IsTrue(new Vec4b(true, true, true, true) ? true : false);
        Assert.IsFalse(new Vec4b(false, false, false, false) ? true : false);
    }
}
