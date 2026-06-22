namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootTextTest
{
    /// <summary>Root text appends formatted spans until the transient buffer is cleared.</summary>
    [TestMethod]
    public void Format_AppendsUntilCleared()
    {
        var text = new RootText();

        Assert.AreEqual("A 1", text.Format("A {0}", 1).ToString());
        Assert.AreEqual("B 2", text.Format("B {0}", 2).ToString());
        text.Clear();

        Assert.AreEqual("C 3", text.Format("C {0}", 3).ToString());
    }

    /// <summary>Root text formats AlvorKit vector types as compact coordinate tuples.</summary>
    [TestMethod]
    public void Format_WithVector_UsesTupleShape()
    {
        var text = new RootText();

        Assert.AreEqual("pos=(1, 2, 3)", text.Format("pos={0}", new Vec3(1, 2, 3)).ToString());
    }

    /// <summary>Root text supports the old engine's two-through-eight argument formatting overloads.</summary>
    [TestMethod]
    public void Format_Overloads_AppendExpectedArguments()
    {
        var text = new RootText();

        Assert.AreEqual("1 2", text.Format("{0} {1}", 1, 2).ToString());
        Assert.AreEqual("1 2 3", text.Format("{0} {1} {2}", 1, 2, 3).ToString());
        Assert.AreEqual("1 2 3 4", text.Format("{0} {1} {2} {3}", 1, 2, 3, 4).ToString());
        Assert.AreEqual("1 2 3 4 5", text.Format("{0} {1} {2} {3} {4}", 1, 2, 3, 4, 5).ToString());
        Assert.AreEqual("1 2 3 4 5 6", text.Format("{0} {1} {2} {3} {4} {5}", 1, 2, 3, 4, 5, 6).ToString());
        Assert.AreEqual("1 2 3 4 5 6 7", text.Format("{0} {1} {2} {3} {4} {5} {6}", 1, 2, 3, 4, 5, 6, 7).ToString());
        Assert.AreEqual("1 2 3 4 5 6 7 8", text.Format("{0} {1} {2} {3} {4} {5} {6} {7}", 1, 2, 3, 4, 5, 6, 7, 8).ToString());
    }

    /// <summary>Root text preserves compatibility formatters for builders, memory, and vector scalar families.</summary>
    [TestMethod]
    public void Format_CustomFormatters_CoverEngineConvenienceTypes()
    {
        var text = new RootText();
        ReadOnlyMemory<char> memory = "mem".AsMemory();

        Assert.AreEqual("builder", text.Format("{0}", new StringBuilder("builder")).ToString());
        Assert.AreEqual("mem", text.Format("{0}", memory).ToString());
        Assert.AreEqual("(1, 2)", text.Format("{0}", new Vec2(1, 2)).ToString());
        Assert.AreEqual("(1, 2)", text.Format("{0}", new Vec2i(1, 2)).ToString());
        Assert.AreEqual("(1, 2)", text.Format("{0}", new Vec2d(1, 2)).ToString());
        Assert.AreEqual("(1, 2, 3)", text.Format("{0}", new Vec3i(1, 2, 3)).ToString());
        Assert.AreEqual("(1, 2, 3)", text.Format("{0}", new Vec3d(1, 2, 3)).ToString());
        Assert.AreEqual("(1, 2, 3, 4)", text.Format("{0}", new Vec4(1, 2, 3, 4)).ToString());
        Assert.AreEqual("(1, 2, 3, 4)", text.Format("{0}", new Vec4i(1, 2, 3, 4)).ToString());
        Assert.AreEqual("(1, 2, 3, 4)", text.Format("{0}", new Vec4d(1, 2, 3, 4)).ToString());
    }
}
