namespace AlvorKit;

[TestClass]
public class FnGraphSamplingTest
{
    private const int Seed = 7331;

    /// <summary>Proves vector-shaped wrapper calls forward every batch and single-sample shape exactly.</summary>
    [TestMethod]
    public void SamplingMethodsMatchRawBinding()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex).Float(FnFloatVariable.FeatureScale, 37f);

        VerifyUniformGrids(fn, node);
        VerifyTileable(fn, node);
        VerifyPositionArrays(fn, node);
        VerifySingles(fn, node);
    }

    private static void VerifyUniformGrids(Fn fn, FnGraphNode node)
    {
        var typed2 = new float[5 * 3];
        var raw2 = new float[typed2.Length];
        node.GenUniformGrid2D(typed2, (-2f, 7f), (5, 3), (0.5f, 1.25f), Seed);
        fn.GenUniformGrid2D(node.Native, raw2, -2f, 7f, 5, 3, 0.5f, 1.25f, Seed);
        AssertEqual(typed2, raw2);

        var typed3 = new float[4 * 3 * 2];
        var raw3 = new float[typed3.Length];
        var typedRange3 = new float[2];
        var rawRange3 = new float[2];
        node.GenUniformGrid3D(typed3, (-2f, 7f, 11f), (4, 3, 2), (0.5f, 1.25f, 0.75f), Seed, typedRange3);
        fn.GenUniformGrid3D(node.Native, raw3, -2f, 7f, 11f, 4, 3, 2, 0.5f, 1.25f, 0.75f, Seed, rawRange3);
        AssertEqual(typed3, raw3);
        AssertEqual(typedRange3, rawRange3);

        var typed4 = new float[3 * 2 * 2 * 2];
        var raw4 = new float[typed4.Length];
        node.GenUniformGrid4D(typed4, (-2f, 7f, 11f, 3f), (3, 2, 2, 2), (0.5f, 1.25f, 0.75f, 2f), Seed);
        fn.GenUniformGrid4D(
            node.Native, raw4, -2f, 7f, 11f, 3f, 3, 2, 2, 2, 0.5f, 1.25f, 0.75f, 2f, Seed);
        AssertEqual(typed4, raw4);
    }

    private static void VerifyTileable(Fn fn, FnGraphNode node)
    {
        var typed = new float[7 * 5];
        var raw = new float[typed.Length];
        var typedRange = new float[2];
        var rawRange = new float[2];
        node.GenTileable2D(typed, (7, 5), (0.75f, 1.5f), Seed, typedRange);
        fn.GenTileable2D(node.Native, raw, 7, 5, 0.75f, 1.5f, Seed, rawRange);
        AssertEqual(typed, raw);
        AssertEqual(typedRange, rawRange);
    }

    private static void VerifyPositionArrays(Fn fn, FnGraphNode node)
    {
        float[] x = [-3f, -1f, 2f, 5f];
        float[] y = [7f, 3f, -2f, 1f];
        float[] z = [11f, 13f, 17f, 19f];
        float[] w = [0.5f, 1.5f, 2.5f, 3.5f];
        var typed = new float[x.Length];
        var raw = new float[x.Length];
        var typedRange = new float[2];
        var rawRange = new float[2];

        node.GenPositionArray2D(typed, x, y, (1f, 2f), Seed, typedRange);
        fn.GenPositionArray2D(node.Native, raw, raw.Length, x, y, 1f, 2f, Seed, rawRange);
        AssertEqual(typed, raw);
        AssertEqual(typedRange, rawRange);

        node.GenPositionArray3D(typed, x, y, z, (1f, 2f, 3f), Seed);
        fn.GenPositionArray3D(node.Native, raw, raw.Length, x, y, z, 1f, 2f, 3f, Seed);
        AssertEqual(typed, raw);

        node.GenPositionArray4D(typed, x, y, z, w, (1f, 2f, 3f, 4f), Seed, typedRange);
        fn.GenPositionArray4D(node.Native, raw, raw.Length, x, y, z, w, 1f, 2f, 3f, 4f, Seed, rawRange);
        AssertEqual(typed, raw);
        AssertEqual(typedRange, rawRange);
    }

    private static void VerifySingles(Fn fn, FnGraphNode node)
    {
        Assert.AreEqual(fn.GenSingle2D(node.Native, 1f, 2f, Seed), node.GenSingle2D((1f, 2f), Seed));
        Assert.AreEqual(fn.GenSingle3D(node.Native, 1f, 2f, 3f, Seed), node.GenSingle3D((1f, 2f, 3f), Seed));
        Assert.AreEqual(
            fn.GenSingle4D(node.Native, 1f, 2f, 3f, 4f, Seed),
            node.GenSingle4D((1f, 2f, 3f, 4f), Seed));
    }

    private static void AssertEqual(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual) =>
        Assert.IsTrue(expected.SequenceEqual(actual));
}
