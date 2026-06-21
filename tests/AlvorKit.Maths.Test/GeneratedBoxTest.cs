namespace AlvorKit.Maths.Test;

/// <summary>Tests generated axis-aligned box types.</summary>
[TestClass]
public sealed class GeneratedBoxTest
{
    /// <summary>Generated box core members expose corners, dimensions, component refs, and span copies.</summary>
    [TestMethod]
    public void GeneratedBoxCoreMembers_Work()
    {
        var box = new Box2(new Vec2(1f, 2f), new Vec2(5f, 8f));
        Span<float> copied = stackalloc float[Box2.ComponentCount];
        box.CopyTo(copied);
        Box2.ComponentRef(ref box, 3) = 10f;
        box[0] = 0f;

        Assert.AreEqual(2, Box2.Dimension);
        Assert.AreEqual(4, Box2.ComponentCount);
        CollectionAssert.AreEqual(new[] { 1f, 2f, 5f, 8f }, copied.ToArray());
        Assert.AreEqual(new Vec2(0f, 2f), box.Min);
        Assert.AreEqual(new Vec2(5f, 10f), box.Max);
        Assert.AreEqual(new Vec2(5f, 8f), box.Size);
        Assert.AreEqual(new Vec2(2.5f, 6f), box.Center);
        Assert.AreEqual(new Vec2(2.5f, 4f), box.HalfSize);
        Assert.AreEqual(5f, box.Width);
        Assert.AreEqual(8f, box.Height);
        Assert.AreEqual(40f, box.Area);
        Assert.ThrowsException<ArgumentException>(() => _ = Box2.Create(new float[3]));
        Assert.ThrowsException<ArgumentException>(() => box.CopyTo(new float[3]));
    }

    /// <summary>Generated boxes support point containment, box containment, intersections, unions, and overlap boxes.</summary>
    [TestMethod]
    public void GeneratedBoxSpatialQueries_Work()
    {
        var box = new Box2(new Vec2(0f, 0f), new Vec2(10f, 8f));
        var nested = new Box2(new Vec2(2f, 3f), new Vec2(6f, 7f));
        var overlapping = new Box2(new Vec2(8f, 4f), new Vec2(12f, 10f));
        var separate = new Box2(new Vec2(11f, 0f), new Vec2(12f, 1f));
        var edgeTouching = new Box2(new Vec2(10f, 0f), new Vec2(12f, 1f));

        Assert.IsTrue(box.Contains(new Vec2(10f, 8f)));
        Assert.IsTrue(box.ContainsHalfOpen(new Vec2(0f, 0f)));
        Assert.IsFalse(box.ContainsHalfOpen(new Vec2(10f, 8f)));
        Assert.IsFalse(box.ContainsExclusive(new Vec2(10f, 8f)));
        Assert.IsTrue(box.Contains(nested));
        Assert.IsTrue(box.ContainsHalfOpen(nested));
        Assert.IsFalse(box.Contains(overlapping));
        Assert.IsTrue(box.Intersects(overlapping));
        Assert.IsFalse(box.Intersects(separate));
        Assert.IsTrue(box.Intersects(edgeTouching));
        Assert.IsFalse(box.IntersectsExclusive(edgeTouching));
        Assert.AreEqual(new Vec2(10f, 0f), box.ClosestPoint(new Vec2(11f, -1f)));
        Assert.AreEqual(2f, box.DistanceSquaredTo(new Vec2(11f, -1f)));
        Assert.AreEqual(new Box2(new Vec2(0f, 0f), new Vec2(12f, 10f)), Box2.Union(box, overlapping));
        Assert.AreEqual(new Box2(new Vec2(8f, 4f), new Vec2(10f, 8f)), Box2.Intersection(box, overlapping));
        Assert.IsTrue(Box2.Intersection(box, separate).IsEmpty);
    }

    /// <summary>Generated boxes expose construction, normalization, mutation, and scale helpers.</summary>
    [TestMethod]
    public void GeneratedBoxConstructionAndMutationHelpers_Work()
    {
        var normalized = Box3.CreateFromCorners(new Vec3(4f, 5f, -2f), new Vec3(1f, 2f, 3f));
        var centered = Box3.CreateFromCenterSize(new Vec3(5f, 6f, 7f), new Vec3(2f, 4f, 6f));
        var editable = new Box3(Vec3.Zero, Vec3.One)
        {
            Center = new Vec3(4f, 5f, 6f),
        };
        Assert.AreEqual(new Box3(new Vec3(3.5f, 4.5f, 5.5f), new Vec3(4.5f, 5.5f, 6.5f)), editable);

        editable.HalfSize = new Vec3(1f, 2f, 3f);
        Assert.AreEqual(new Box3(new Vec3(3f, 3f, 3f), new Vec3(5f, 7f, 9f)), editable);

        editable.Size = new Vec3(10f, 20f, 30f);
        editable.Translate(new Vec3(1f, 2f, 3f));
        editable.Inflate(new Vec3(1f));

        Assert.AreEqual(new Box3(new Vec3(3f, 4f, 5f), new Vec3(15f, 26f, 37f)), editable);
        Assert.AreEqual(new Box3(new Vec3(1f, 2f, -2f), new Vec3(4f, 5f, 3f)), normalized);
        Assert.AreEqual(new Box3(new Vec3(4f, 4f, 4f), new Vec3(6f, 8f, 10f)), centered);
        Assert.AreEqual(new Box3(new Vec3(3f, 2f, 1f), new Vec3(7f, 10f, 13f)), centered.Scaled(new Vec3(2f, 2f, 2f)));
        Assert.AreEqual(2f, centered.Width);
        Assert.AreEqual(4f, centered.Height);
        Assert.AreEqual(6f, centered.Depth);
        Assert.AreEqual(48f, centered.Volume);
    }

    /// <summary>Generated integer boxes cover pixel, tile, and chunk-style bounds.</summary>
    [TestMethod]
    public void GeneratedIntegerBoxes_Work()
    {
        var viewport = new Box2i(new Vec2i(16, 24), new Vec2i(80, 120));
        var tile = Box2i.CreateFromCenterHalfSize(new Vec2i(10, 10), new Vec2i(2, 3));
        var chunk = new Box3i(new Vec3i(0, 0, 0), new Vec3i(16, 8, 4));
        Box2 floatViewport = viewport;

        Assert.AreEqual(new Vec2i(64, 96), viewport.Size);
        Assert.AreEqual(6144, viewport.Area);
        Assert.AreEqual(new Box2i(new Vec2i(8, 7), new Vec2i(12, 13)), tile);
        Assert.AreEqual(new Vec3i(16, 8, 4), chunk.Size);
        Assert.AreEqual(512, chunk.Volume);
        Assert.AreEqual(1f, viewport.DistanceTo(new Vec2i(81, 120)));
        Assert.AreEqual(new Box2(new Vec2(16f, 24f), new Vec2(80f, 120f)), floatViewport);
        Assert.IsTrue(Box2i.Empty.IsEmpty);
    }

    /// <summary>Generated box formatting and parsing mirror vector-style nested component text.</summary>
    [TestMethod]
    public void GeneratedBoxFormattingAndParsing_UsesVectorStyle()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Box2(new Vec2(1f, 2f), new Vec2(3.5f, 4f));
        Span<char> destination = stackalloc char[24];
        Span<byte> utf8Destination = stackalloc byte[24];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("((1, 2), (3.5, 4))", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("((1, 2), (3.5, 4))", System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("((1.0, 2.0), (3.5, 4.0))", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Box2.Parse("((1, 2), (3.5, 4))", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Box2>("((1, 2), (3.5, 4))"u8));
        Assert.IsFalse(Box2.TryParse("not a box", formatProvider, out _));
        Assert.IsFalse(Box2.TryParse((string?)null, formatProvider, out _));
    }

    /// <summary>Generated box interfaces expose the types through static abstract members.</summary>
    [TestMethod]
    public void GeneratedBoxInterfaces_Work()
    {
        var box = CreateGeneric<Box2, float, Vec2>(new Vec2(1f, 2f), new Vec2(3f, 4f));
        var normalized = CreateFromCornersGeneric<Box2, float, Vec2>(new Vec2(3f, 4f), new Vec2(1f, 2f));
        var union = UnionGeneric<Box2, float, Vec2>(box, new Box2(new Vec2(-1f, 3f), new Vec2(2f, 5f)));

        Assert.AreEqual(new Box2(new Vec2(1f, 2f), new Vec2(3f, 4f)), box);
        Assert.AreEqual(box, normalized);
        Assert.AreEqual(new Box2(new Vec2(-1f, 2f), new Vec2(3f, 5f)), union);
        Assert.IsTrue(ContainsGeneric<Box2, float, Vec2>(box, new Vec2(2f, 3f)));
    }

    private static TBox CreateGeneric<TBox, TScalar, TVector>(TVector min, TVector max)
        where TBox : struct, IBox<TBox, TScalar, TVector> =>
        TBox.Create(min, max);

    private static TBox CreateFromCornersGeneric<TBox, TScalar, TVector>(TVector first, TVector second)
        where TBox : struct, IBox<TBox, TScalar, TVector> =>
        TBox.CreateFromCorners(first, second);

    private static TBox UnionGeneric<TBox, TScalar, TVector>(TBox left, TBox right)
        where TBox : struct, IBox<TBox, TScalar, TVector> =>
        TBox.Union(left, right);

    private static bool ContainsGeneric<TBox, TScalar, TVector>(TBox box, TVector point)
        where TBox : struct, IBox<TBox, TScalar, TVector> =>
        box.Contains(point);

    private static T ParseUtf8<T>(ReadOnlySpan<byte> text)
        where T : IUtf8SpanParsable<T> =>
        T.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
