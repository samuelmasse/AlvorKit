namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated quad source planning and emission.</summary>
[TestClass]
public sealed class QuadEmitterTest
{
    /// <summary>Quad catalog specs use the expected floating-point 3D naming and layout metadata.</summary>
    [TestMethod]
    public void QuadCatalog_UsesExpectedNames()
    {
        var quad = new QuadSpec(VectorCatalog.Float);
        var quadd = new QuadSpec(VectorCatalog.Double);
        var names = QuadCatalog.Quads.Select(spec => spec.TypeName).ToArray();

        CollectionAssert.AreEqual(new[] { "Quad3", "Quad3d" }, names);
        CollectionAssert.AreEqual(new[] { VectorCatalog.Float, VectorCatalog.Double }, QuadCatalog.Scalars.ToArray());
        Assert.AreEqual("Quad3", VectorCatalog.Float.QuadName());
        Assert.AreEqual("Vec3", quad.Vector3TypeName);
        Assert.AreEqual("Box3", quad.Box3TypeName);
        Assert.AreEqual(48, quad.SizeBytes);
        Assert.AreEqual("4f", quad.FourLiteral);
        Assert.AreEqual("Quad3d", quadd.TypeName);
        Assert.AreEqual("4d", quadd.FourLiteral);
    }

    /// <summary>Quad source includes ordered corners, span interop, value semantics, helpers, and scalar conversions.</summary>
    [TestMethod]
    public void QuadEmitter_EmitsExpectedQuadFeatures()
    {
        var quad = QuadFileEmitter.Emit(new(VectorCatalog.Float));
        var quadd = QuadFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(quad, "public struct Quad3(");
        StringAssert.Contains(quad, "IQuad3<Quad3, float, Vec3, Box3>");
        StringAssert.Contains(quad, "public const int ComponentCount = 12;");
        StringAssert.Contains(quad, "public Vec3 TopLeft = topLeft;");
        StringAssert.Contains(quad, "public readonly Vec3 Center =>");
        StringAssert.Contains(quad, "Box3.CreateFromCorners(TopLeft, TopRight).Including(BottomLeft).Including(BottomRight)");
        StringAssert.Contains(quad, "return ref Vec3.ComponentRef(ref value.BottomRight, index - 9);");
        StringAssert.Contains(quad, "public static implicit operator Quad3d(Quad3 value)");
        StringAssert.Contains(quad, "public static implicit operator Quad3((");
        StringAssert.Contains(quad, "MathsComponentParseHelper.TryReadNextTopLevelComponent");
        StringAssert.Contains(quad, "out Vec3 bottomRight");
        StringAssert.Contains(quadd, "public static explicit operator Quad3(Quad3d value)");
        StringAssert.Contains(quadd, "public const int SizeInBytes = 96;");
    }
}
