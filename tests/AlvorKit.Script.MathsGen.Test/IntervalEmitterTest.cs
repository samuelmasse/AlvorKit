namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated scalar interval source planning and emission.</summary>
[TestClass]
public sealed class IntervalEmitterTest
{
    /// <summary>Interval catalogs use the float and double interval names.</summary>
    [TestMethod]
    public void IntervalCatalog_UsesExpectedNames()
    {
        var names = IntervalCatalog.Intervals.Select(interval => interval.TypeName).ToArray();

        Assert.AreEqual(2, names.Length);
        CollectionAssert.AreEqual(new[] { "Intervalf", "Intervald" }, names);
        Assert.AreEqual("Intervalf", VectorCatalog.Float.IntervalName());
        Assert.AreEqual("Intervald", VectorCatalog.Double.IntervalName());
    }

    /// <summary>Interval source includes endpoint helpers, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void IntervalEmitter_EmitsExpectedIntervalFeatures()
    {
        var interval = IntervalFileEmitter.Emit(new(VectorCatalog.Float));
        var intervald = IntervalFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(interval, "/// <summary>Single-precision floating-point inclusive scalar interval.");
        StringAssert.Contains(interval, "public struct Intervalf(float min, float max)");
        StringAssert.Contains(interval, "IInterval<Intervalf, float>");
        StringAssert.Contains(interval, "public static Intervalf Empty => new(float.PositiveInfinity, float.NegativeInfinity);");
        StringAssert.Contains(interval, "public static Intervalf CreateFromEndpoints(float first, float second)");
        StringAssert.Contains(interval, "public static Intervalf Union(Intervalf left, Intervalf right)");
        StringAssert.Contains(interval, "public static implicit operator Intervald(Intervalf value)");
        StringAssert.Contains(interval, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Intervalf result)");
        StringAssert.Contains(intervald, "/// <summary>Double-precision floating-point inclusive scalar interval.");
        StringAssert.Contains(intervald, "IInterval<Intervald, double>");
        StringAssert.Contains(intervald, "public static explicit operator Intervalf(Intervald value)");
    }
}
