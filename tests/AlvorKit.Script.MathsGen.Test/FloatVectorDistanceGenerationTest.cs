namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests measured direct float-distance generation.</summary>
[TestClass]
public sealed class FloatVectorDistanceGenerationTest
{
    /// <summary>All float dimensions use direct ordered distance kernels.</summary>
    [TestMethod]
    public void FloatVectors_UseDirectOrderedDistanceKernels()
    {
        foreach (var dimension in new[] { 2, 3, 4 })
        {
            var source = VectorFileEmitter.Emit(new(dimension, VectorCatalog.Float));

            StringAssert.Contains(source, "[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            StringAssert.Contains(source, "var x = left.X - right.X;");
            StringAssert.Contains(source, "var y = left.Y - right.Y;");
            StringAssert.Contains(source, "ScalarMath.Sqrt(DistanceSquared(left, right))");
            Assert.IsFalse(source.Contains("(left - right).LengthSquared", StringComparison.Ordinal));
        }
    }

    /// <summary>Double and Int32 vectors retain their measured current composition.</summary>
    [TestMethod]
    public void OtherCoreVectors_RetainComposedDistance()
    {
        StringAssert.Contains(VectorFileEmitter.Emit(new(4, VectorCatalog.Double)), "(left - right).LengthSquared");
        StringAssert.Contains(VectorFileEmitter.Emit(new(4, VectorCatalog.Int)), "(left - right).LengthSquared");
    }
}
