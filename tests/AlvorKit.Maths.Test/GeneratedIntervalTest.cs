namespace AlvorKit.Maths.Test;

/// <summary>Tests generated scalar interval types.</summary>
[TestClass]
public sealed class GeneratedIntervalTest
{
    /// <summary>Generated interval core members expose endpoints, component refs, and span copies.</summary>
    [TestMethod]
    public void GeneratedIntervalCoreMembers_Work()
    {
        var interval = new Intervalf(1f, 5f);
        Span<float> copied = stackalloc float[Intervalf.ComponentCount];
        interval.CopyTo(copied);
        Intervalf.ComponentRef(ref interval, 1) = 6f;
        interval[0] = 0f;
        var fromSpan = Intervalf.Create(copied);
        var fromEndpoints = Intervalf.CreateFromEndpoints(5f, 1f);

        Assert.AreEqual(2, Intervalf.ComponentCount);
        Assert.AreEqual(8, Intervalf.SizeInBytes);
        CollectionAssert.AreEqual(new[] { 1f, 5f }, copied.ToArray());
        Assert.AreEqual(new Intervalf(0f, 6f), interval);
        Assert.AreEqual(new Intervalf(1f, 5f), fromSpan);
        Assert.AreEqual(new Intervalf(1f, 5f), fromEndpoints);
        Assert.AreEqual(6f, interval.Length);
        Assert.AreEqual(3f, interval.Center);
        Assert.ThrowsException<ArgumentException>(() => _ = Intervalf.Create(new float[1]));
        Assert.ThrowsException<ArgumentException>(() => interval.CopyTo(new float[1]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = interval[2]);
    }

    /// <summary>Generated intervals treat minimum greater than maximum as empty.</summary>
    [TestMethod]
    public void GeneratedIntervalEmptyHandling_Work()
    {
        var empty = new Intervalf(3f, 2f);
        var full = new Intervalf(0f, 2f);

        Assert.IsTrue(Intervalf.Empty.IsEmpty);
        Assert.IsTrue(empty.IsEmpty);
        Assert.AreEqual(0f, empty.Length);
        Assert.IsFalse(empty.Contains(2.5f));
        Assert.IsFalse(full.Contains(empty));
        Assert.IsFalse(empty.Intersects(full));
        Assert.AreEqual(full, Intervalf.Union(empty, full));
        Assert.AreEqual(Intervalf.Empty, Intervalf.Intersection(empty, full));
    }

    /// <summary>Generated intervals support containment, intersections, unions, and overlap intervals.</summary>
    [TestMethod]
    public void GeneratedIntervalSpatialQueries_Work()
    {
        var interval = new Intervalf(0f, 10f);
        var nested = new Intervalf(2f, 8f);
        var overlapping = new Intervalf(8f, 12f);
        var touching = new Intervalf(10f, 12f);
        var separate = new Intervalf(11f, 12f);

        Assert.IsTrue(interval.Contains(10f));
        Assert.IsFalse(interval.Contains(10.1f));
        Assert.IsTrue(interval.Contains(nested));
        Assert.IsFalse(interval.Contains(overlapping));
        Assert.IsTrue(interval.Intersects(overlapping));
        Assert.IsTrue(interval.Intersects(touching));
        Assert.IsFalse(interval.Intersects(separate));
        Assert.AreEqual(new Intervalf(0f, 12f), Intervalf.Union(interval, overlapping));
        Assert.AreEqual(new Intervalf(8f, 10f), Intervalf.Intersection(interval, overlapping));
        Assert.AreEqual(new Intervalf(10f, 10f), Intervalf.Intersection(interval, touching));
    }

    /// <summary>Generated interval formatting and parsing use flat endpoint text.</summary>
    [TestMethod]
    public void GeneratedIntervalFormattingAndParsing_Work()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Intervalf(1f, 3.5f);
        Span<char> destination = stackalloc char[8];
        Span<byte> utf8Destination = stackalloc byte[8];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual("(1, 3.5)", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual("(1, 3.5)", Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("(1.0, 3.5)", value.ToString("0.0", formatProvider));
        Assert.AreEqual(value, Intervalf.Parse("(1, 3.5)", formatProvider));
        Assert.AreEqual(value, ParseUtf8<Intervalf>("(1, 3.5)"u8));
        Assert.IsFalse(Intervalf.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Intervalf.TryParse("not an interval", formatProvider, out _));
    }

    /// <summary>Generated intervals convert across scalar families and support endpoint tuple conversions.</summary>
    [TestMethod]
    public void GeneratedIntervalConversions_Work()
    {
        var value = new Intervalf(1f, 2f);
        Intervald doubleValue = value;
        var floatValue = (Intervalf)doubleValue;
        (float min, float max) = value;
        Intervalf tupleValue = (min, max);

        Assert.AreEqual(new Intervald(1d, 2d), doubleValue);
        Assert.AreEqual(value, floatValue);
        Assert.AreEqual(value, tupleValue);
    }

    /// <summary>Generated double-precision intervals mirror the single-precision API.</summary>
    [TestMethod]
    public void GeneratedIntervaldHelpers_Work()
    {
        var interval = new Intervald(1d, 5d);

        Assert.IsTrue(interval.Contains(3d));
        Assert.AreEqual(4d, interval.Length);
        Assert.AreEqual(3d, interval.Center);
        Assert.AreEqual(new Intervald(1d, 3d), Intervald.Intersection(interval, new Intervald(-1d, 3d)));
        Assert.AreEqual(interval, ParseUtf8<Intervald>("(1, 5)"u8));
    }

    /// <summary>Generated interval interfaces expose the types through static abstract members.</summary>
    [TestMethod]
    public void GeneratedIntervalInterfaces_Work()
    {
        var interval = CreateGeneric<Intervalf, float>(1f, 3f);
        var endpoints = CreateFromEndpointsGeneric<Intervalf, float>(3f, 1f);
        var union = UnionGeneric<Intervalf, float>(interval, new Intervalf(2f, 5f));

        Assert.AreEqual(new Intervalf(1f, 3f), interval);
        Assert.AreEqual(interval, endpoints);
        Assert.AreEqual(new Intervalf(1f, 5f), union);
        Assert.IsTrue(ContainsGeneric<Intervalf, float>(interval, 2f));
    }

    private static TInterval CreateGeneric<TInterval, TScalar>(TScalar min, TScalar max)
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        TInterval.Create(min, max);

    private static TInterval CreateFromEndpointsGeneric<TInterval, TScalar>(TScalar first, TScalar second)
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        TInterval.CreateFromEndpoints(first, second);

    private static TInterval UnionGeneric<TInterval, TScalar>(TInterval left, TInterval right)
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        TInterval.Union(left, right);

    private static bool ContainsGeneric<TInterval, TScalar>(TInterval interval, TScalar value)
        where TInterval : struct, IInterval<TInterval, TScalar> =>
        interval.Contains(value);

    private static T ParseUtf8<T>(ReadOnlySpan<byte> text)
        where T : IUtf8SpanParsable<T> =>
        T.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
