namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Verifies the measured packed equality lowering for generated float vectors.</summary>
[TestClass]
public sealed class FloatVectorValueSemanticsGenerationTest
{
    /// <summary>Vec2, Vec3, and Vec4 use their exact private System view for typed equality.</summary>
    [TestMethod]
    public void FloatVectors_UsePackedSystemEquality()
    {
        for (var dimension = 2; dimension <= 4; dimension++)
        {
            var typeName = $"Vec{dimension}";
            var source = VectorFileEmitter.Emit(new(dimension, VectorCatalog.Float));

            StringAssert.Contains(source,
                $"public readonly bool Equals({typeName} other) =>{Environment.NewLine}" +
                "        packed.Equals(other.packed);");
        }
    }

    /// <summary>Double and integer vectors retain their component-wise equality contracts.</summary>
    [TestMethod]
    public void OtherVectors_RetainComponentEquality()
    {
        var doubleSource = VectorFileEmitter.Emit(new(4, VectorCatalog.Double));
        var integerSource = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));

        StringAssert.Contains(doubleSource,
            "EqualScalar(X, other.X) && EqualScalar(Y, other.Y) && EqualScalar(Z, other.Z) && EqualScalar(W, other.W)");
        StringAssert.Contains(integerSource,
            "EqualScalar(X, other.X) && EqualScalar(Y, other.Y) && EqualScalar(Z, other.Z) && EqualScalar(W, other.W)");
        Assert.IsFalse(doubleSource.Contains("packed.Equals(other.packed)", StringComparison.Ordinal));
        Assert.IsFalse(integerSource.Contains("packed.Equals(other.packed)", StringComparison.Ordinal));
    }
}
