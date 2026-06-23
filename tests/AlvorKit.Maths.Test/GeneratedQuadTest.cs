namespace AlvorKit.Maths.Test;

/// <summary>Tests generated 3D quad types.</summary>
[TestClass]
public sealed class GeneratedQuadTest
{
    /// <summary>Generated quads expose ordered corner components and span interop.</summary>
    [TestMethod]
    public void GeneratedQuadComponents_Work()
    {
        var quad = new Quad3((0f, 1f, 2f), (3f, 4f, 5f), (6f, 7f, 8f), (9f, 10f, 11f));

        Assert.AreEqual(12, Quad3.ComponentCount);
        Assert.AreEqual(48, Quad3.SizeInBytes);
        Assert.AreEqual(new Vec3(0f, 1f, 2f), quad.TopLeft);
        Assert.AreEqual(4f, quad[4]);

        quad[11] = 12f;
        ref var topLeftX = ref Quad3.ComponentRef(ref quad, 0);
        topLeftX = -1f;

        Span<float> values = stackalloc float[Quad3.ComponentCount];
        quad.CopyTo(values);

        Assert.AreEqual(-1f, values[0]);
        Assert.AreEqual(12f, values[11]);
        Assert.AreEqual(quad, new Quad3(values));
        Assert.IsTrue(quad.TryCopyTo(values));
        Assert.IsFalse(quad.TryCopyTo(values[..11]));
    }

    /// <summary>Generated quads format, parse, compare, and convert in corner order.</summary>
    [TestMethod]
    public void GeneratedQuadValueSemantics_Work()
    {
        var quad = new Quad3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.One);
        var text = quad.ToString();

        Assert.AreEqual(quad, Quad3.Parse(text, null));
        Assert.IsTrue(Quad3.TryParse(Encoding.UTF8.GetBytes(text), null, out var parsedUtf8));
        Assert.AreEqual(quad, parsedUtf8);
        Assert.IsTrue(quad.CompareTo(new Quad3(Vec3.One, Vec3.UnitX, Vec3.UnitY, Vec3.One)) < 0);

        Quad3d widened = quad;
        var narrowed = (Quad3)widened;
        Assert.AreEqual(quad, narrowed);
    }

    /// <summary>Generated quads expose unambiguous center and bounds helpers.</summary>
    [TestMethod]
    public void GeneratedQuadGeometryHelpers_Work()
    {
        var quad = new Quad3((0f, 2f, 1f), (2f, 2f, 1f), (0f, 0f, -1f), (2f, 0f, -1f));

        Assert.AreEqual(new Vec3(1f, 1f, 0f), quad.Center);
        Assert.AreEqual(new Box3(new Vec3(0f, 0f, -1f), new Vec3(2f, 2f, 1f)), quad.Bounds);

        var doubleQuad = new Quad3d(Vec3d.Zero, Vec3d.UnitX, Vec3d.UnitY, Vec3d.One);

        Assert.AreEqual(new Vec3d(0.5d, 0.5d, 0.25d), doubleQuad.Center);
    }
}
