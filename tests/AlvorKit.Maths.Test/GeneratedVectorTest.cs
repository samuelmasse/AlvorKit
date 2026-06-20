namespace AlvorKit.Maths.Test;

/// <summary>Tests vector behavior that is available only after multi-dimension generation.</summary>
[TestClass]
public sealed class GeneratedVectorTest
{
    /// <summary>Generated Vec2 and Vec4 types expose aliases, indexing, tuple conversions, and arithmetic.</summary>
    [TestMethod]
    public void GeneratedDimensions_Work()
    {
        Vec2 fromTuple = (1f, 2f);
        var vec4 = new Vec4(1f, 2f, 3f, 4f)
        {
            A = 5f,
            Q = 6f,
        };
        vec4[0] = 7f;

        Assert.AreEqual(new Vec2(3f, 4f), fromTuple + Vec2.One * 2f);
        Assert.AreEqual(new Vec4(7f, 2f, 3f, 6f), vec4);
        Assert.AreEqual(6f, vec4.W);
        Assert.AreEqual(6f, vec4.A);
        Assert.AreEqual(6f, vec4[3]);
    }

    /// <summary>Generated vectors expose mutable component references through a C#-legal ref helper.</summary>
    [TestMethod]
    public void GeneratedComponentRefs_Work()
    {
        var value = new Vec3(1f, 2f, 3f);
        ref var y = ref Vec3.ComponentRef(ref value, 1);
        y = 9f;

        var mask = new Vec3b(false, false, false);
        ref var z = ref Vec3b.ComponentRef(ref mask, 2);
        z = true;

        Assert.AreEqual(new Vec3(1f, 9f, 3f), value);
        Assert.AreEqual(new Vec3b(false, false, true), mask);
    }

    /// <summary>Generated vectors copy to spans and arrays without changing component order.</summary>
    [TestMethod]
    public void GeneratedSpanInterop_Works()
    {
        ReadOnlySpan<float> source = [1f, 2f, 3f];
        var value = Vec3.Create(source);
        Span<float> destination = stackalloc float[3];
        var array = new float[5];

        Assert.AreEqual(new Vec3(1f, 2f, 3f), value);
        Assert.IsTrue(value.TryCopyTo(destination));
        Assert.AreEqual(1f, destination[0]);
        Assert.AreEqual(2f, destination[1]);
        Assert.AreEqual(3f, destination[2]);
        value.CopyTo(array, 1);
        Assert.AreEqual(0f, array[0]);
        Assert.AreEqual(1f, array[1]);
        Assert.AreEqual(2f, array[2]);
        Assert.AreEqual(3f, array[3]);
        Assert.IsFalse(value.TryCopyTo(stackalloc float[2]));
        Assert.ThrowsException<ArgumentException>(() => value.CopyTo(new float[2]));
        Assert.ThrowsException<ArgumentException>(() => _ = Vec3.Create([1f, 2f]));
    }

    /// <summary>Generated formatting uses tuple-style text and reports too-small span destinations without allocating.</summary>
    [TestMethod]
    public void GeneratedFormatting_UsesTupleStyleAndSpanDestination()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        var value = new Vec2(0f, 0f);
        Span<char> destination = stackalloc char[6];
        Span<char> tooSmall = stackalloc char[5];
        Span<byte> utf8Destination = stackalloc byte[6];
        Span<byte> utf8TooSmall = stackalloc byte[5];

        Assert.IsTrue(value.TryFormat(destination, out var charsWritten, default, formatProvider));
        Assert.AreEqual(6, charsWritten);
        Assert.AreEqual("(0, 0)", destination[..charsWritten].ToString());
        Assert.IsTrue(value.TryFormat(utf8Destination, out var bytesWritten, default, formatProvider));
        Assert.AreEqual(6, bytesWritten);
        Assert.AreEqual("(0, 0)", System.Text.Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
        Assert.AreEqual("(0.0, 0.0)", value.ToString("0.0", formatProvider));
        Assert.IsFalse(value.TryFormat(tooSmall, out var shortCharsWritten, default, formatProvider));
        Assert.AreEqual(0, shortCharsWritten);
        Assert.IsFalse(value.TryFormat(utf8TooSmall, out var shortBytesWritten, default, formatProvider));
        Assert.AreEqual(0, shortBytesWritten);
    }

    /// <summary>Generated parsing accepts tuple-style text from strings, spans, and UTF-8 byte spans.</summary>
    [TestMethod]
    public void GeneratedParsing_AcceptsTupleStyleText()
    {
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

        Assert.AreEqual(new Vec2(1f, 2.5f), Vec2.Parse("(1.0, 2.5)", formatProvider));
        Assert.AreEqual(new Vec2i(1, -2), ParseSpan<Vec2i>("(1, -2)".AsSpan()));
        Assert.AreEqual(new Vec2b(true, false), Vec2b.Parse("(true, False)", formatProvider));
        Assert.AreEqual(new Vec2b(true, false), ParseUtf8<Vec2b>("(True, False)"u8));
        Assert.IsTrue(Vec2.TryParse("(3.5, 4.5)"u8, formatProvider, out var utf8Parsed));
        Assert.AreEqual(new Vec2(3.5f, 4.5f), utf8Parsed);
        Assert.IsFalse(Vec2.TryParse((string?)null, formatProvider, out _));
        Assert.IsFalse(Vec2.TryParse("(1,2)", formatProvider, out _));
        Assert.ThrowsException<FormatException>(() => Vec2.Parse("(1,2)", formatProvider));
    }

    /// <summary>Generated comparison uses lexicographic component order for sorted collections and Boolean masks.</summary>
    [TestMethod]
    public void GeneratedComparison_UsesLexicographicOrder()
    {
        var sorted = new SortedList<Vec2i, string>
        {
            [new(1, 0)] = "second-x",
            [new(0, 9)] = "first-x",
            [new(1, -1)] = "first-y",
        };
        var masks = new[] { new Vec2b(true, false), new Vec2b(false, true), new Vec2b(false, false) };

        Array.Sort(masks);

        Assert.IsTrue(new Vec2(0f, 9f).CompareTo(new Vec2(1f, 0f)) < 0);
        Assert.IsTrue(new Vec2(1f, 1f).CompareTo(new Vec2(1f, 0f)) > 0);
        Assert.AreEqual(0, new Vec2(1f, 2f).CompareTo(new Vec2(1f, 2f)));
        Assert.AreEqual(new Vec2i(0, 9), sorted.Keys[0]);
        Assert.AreEqual(new Vec2i(1, -1), sorted.Keys[1]);
        Assert.AreEqual(new Vec2i(1, 0), sorted.Keys[2]);
        CollectionAssert.AreEqual(
            new[] { new Vec2b(false, false), new Vec2b(false, true), new Vec2b(true, false) },
            masks);
    }

    /// <summary>Generated vectors support GLM-style composition constructors and truncating dimension conversions.</summary>
    [TestMethod]
    public void GeneratedCompositionConstructors_Work()
    {
        var fromXy = new Vec3(new Vec2(1f, 2f), 3f);
        var fromYz = new Vec3(1f, new Vec2(2f, 3f));
        var fromCrossScalarXy = new Vec3(new Vec2i(1, 2), 3f);
        var vec4FromTwoVec2 = new Vec4(new Vec2(1f, 2f), new Vec2(3f, 4f));
        var vec4FromXyz = new Vec4(new Vec3(1f, 2f, 3f), 4f);
        var vec4FromYzw = new Vec4(1f, new Vec3(2f, 3f, 4f));
        var vec4FromMiddle = new Vec4(1f, new Vec2(2f, 3f), 4f);
        var vec4FromZw = new Vec4(1f, 2f, new Vec2(3f, 4f));
        var truncated2 = new Vec2(new Vec4(1f, 2f, 3f, 4f));
        var truncated3 = (Vec3)new Vec4i(1, 2, 3, 4);

        Assert.AreEqual(new Vec3(1f, 2f, 3f), fromXy);
        Assert.AreEqual(new Vec3(1f, 2f, 3f), fromYz);
        Assert.AreEqual(new Vec3(1f, 2f, 3f), fromCrossScalarXy);
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), vec4FromTwoVec2);
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), vec4FromXyz);
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), vec4FromYzw);
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), vec4FromMiddle);
        Assert.AreEqual(new Vec4(1f, 2f, 3f, 4f), vec4FromZw);
        Assert.AreEqual(new Vec2(1f, 2f), truncated2);
        Assert.AreEqual(new Vec3(1f, 2f, 3f), truncated3);
    }

    /// <summary>Generated swizzles read repeated components and write distinct component groups.</summary>
    [TestMethod]
    public void GeneratedSwizzles_Work()
    {
        var value = new Vec3(1f, 2f, 3f)
        {
            YX = new Vec2(8f, 9f),
        };
        var afterYx = value.XY;
        value.BGR = new Vec3(4f, 5f, 6f);

        Assert.AreEqual(new Vec2(9f, 8f), afterYx);
        Assert.AreEqual(new Vec3(6f, 5f, 4f), value.RGB);
        Assert.AreEqual(new Vec4(6f, 6f, 5f, 4f), value.RRGB);
        Assert.AreEqual(new Vec3(4f, 4f, 4f), value.BBB);
    }

    /// <summary>Generated scalar families keep suffix naming and cross-type conversions coherent.</summary>
    [TestMethod]
    public void GeneratedScalarFamilies_Work()
    {
        Vec3d asDouble = new Vec3(1f, 2f, 3f);
        Vec3 asFloat = new Vec3i(1, 2, 3);
        Vec3i128 asInt128 = new Vec3u64(1UL, 2UL, 3UL);
        var asLong = (Vec3i64)new Vec3u64(1UL, 2UL, 3UL);
        var boolMask = (Vec4b)new Vec4u(1u, 0u, 2u, 0u);

        Assert.AreEqual(new Vec3d(1d, 2d, 3d), asDouble);
        Assert.AreEqual(new Vec3(1f, 2f, 3f), asFloat);
        Assert.AreEqual(new Vec3i128((Int128)1, (Int128)2, (Int128)3), asInt128);
        Assert.AreEqual(new Vec3i64(1L, 2L, 3L), asLong);
        Assert.AreEqual(new Vec4b(true, false, true, false), boolMask);
    }

    /// <summary>Generated small, half, and 128-bit scalar families use the expected suffixes and arithmetic behavior.</summary>
    [TestMethod]
    public void GeneratedExpandedScalarFamilies_Work()
    {
        var half = new Vec3h((Half)1, (Half)2, (Half)3) + new Vec3h((Half)1);
        var wrappedByte = new Vec3u8(250, 1, 2) + new Vec3u8(10, 2, 3);
        var signedByte = -new Vec3i8(1, 2, 3);
        var unsigned16 = new Vec3u16(1, 2, 3) << 2;
        var shiftedByVector = new Vec3u16(8, 16, 32) >> new Vec3i(0, 1, 2);
        var unsignedRightShift = new Vec3i(-2, -4, -8) >>> 1;
        var big = new Vec3u128((UInt128)1, (UInt128)2, (UInt128)4);

        Assert.AreEqual(new Vec3h((Half)2, (Half)3, (Half)4), half);
        Assert.AreEqual(new Vec3u8(4, 3, 5), wrappedByte);
        Assert.AreEqual(new Vec3i8(-1, -2, -3), signedByte);
        Assert.AreEqual(new Vec3u16(4, 8, 12), unsigned16);
        Assert.AreEqual(new Vec3u16(8, 8, 8), shiftedByVector);
        Assert.AreEqual(new Vec3i(int.MaxValue, 2147483646, 2147483644), unsignedRightShift);
        Assert.AreEqual(new Vec3i(1, 1, 1), Vec3u128.BitCount(big));
    }

    /// <summary>Generated numeric vectors support simple component-wise addition across every size and scalar family.</summary>
    [TestMethod]
    public void GeneratedAdditionOperators_WorkForEveryNumericVectorType()
    {
        Assert.AreEqual(new Vec2(4f, 6f), new Vec2(1f, 2f) + new Vec2(3f, 4f));
        Assert.AreEqual(new Vec3(5f, 7f, 9f), new Vec3(1f, 2f, 3f) + new Vec3(4f, 5f, 6f));
        Assert.AreEqual(new Vec4(6f, 8f, 10f, 12f), new Vec4(1f, 2f, 3f, 4f) + new Vec4(5f, 6f, 7f, 8f));

        Assert.AreEqual(new Vec2d(4d, 6d), new Vec2d(1d, 2d) + new Vec2d(3d, 4d));
        Assert.AreEqual(new Vec3d(5d, 7d, 9d), new Vec3d(1d, 2d, 3d) + new Vec3d(4d, 5d, 6d));
        Assert.AreEqual(new Vec4d(6d, 8d, 10d, 12d), new Vec4d(1d, 2d, 3d, 4d) + new Vec4d(5d, 6d, 7d, 8d));

        Assert.AreEqual(new Vec2h((Half)4, (Half)6), new Vec2h((Half)1, (Half)2) + new Vec2h((Half)3, (Half)4));
        Assert.AreEqual(
            new Vec3h((Half)5, (Half)7, (Half)9),
            new Vec3h((Half)1, (Half)2, (Half)3) + new Vec3h((Half)4, (Half)5, (Half)6));
        Assert.AreEqual(
            new Vec4h((Half)6, (Half)8, (Half)10, (Half)12),
            new Vec4h((Half)1, (Half)2, (Half)3, (Half)4) + new Vec4h((Half)5, (Half)6, (Half)7, (Half)8));

        Assert.AreEqual(new Vec2i8(4, 6), new Vec2i8(1, 2) + new Vec2i8(3, 4));
        Assert.AreEqual(new Vec3i8(5, 7, 9), new Vec3i8(1, 2, 3) + new Vec3i8(4, 5, 6));
        Assert.AreEqual(new Vec4i8(6, 8, 10, 12), new Vec4i8(1, 2, 3, 4) + new Vec4i8(5, 6, 7, 8));

        Assert.AreEqual(new Vec2u8(4, 6), new Vec2u8(1, 2) + new Vec2u8(3, 4));
        Assert.AreEqual(new Vec3u8(5, 7, 9), new Vec3u8(1, 2, 3) + new Vec3u8(4, 5, 6));
        Assert.AreEqual(new Vec4u8(6, 8, 10, 12), new Vec4u8(1, 2, 3, 4) + new Vec4u8(5, 6, 7, 8));

        Assert.AreEqual(new Vec2i16(4, 6), new Vec2i16(1, 2) + new Vec2i16(3, 4));
        Assert.AreEqual(new Vec3i16(5, 7, 9), new Vec3i16(1, 2, 3) + new Vec3i16(4, 5, 6));
        Assert.AreEqual(new Vec4i16(6, 8, 10, 12), new Vec4i16(1, 2, 3, 4) + new Vec4i16(5, 6, 7, 8));

        Assert.AreEqual(new Vec2u16(4, 6), new Vec2u16(1, 2) + new Vec2u16(3, 4));
        Assert.AreEqual(new Vec3u16(5, 7, 9), new Vec3u16(1, 2, 3) + new Vec3u16(4, 5, 6));
        Assert.AreEqual(new Vec4u16(6, 8, 10, 12), new Vec4u16(1, 2, 3, 4) + new Vec4u16(5, 6, 7, 8));

        Assert.AreEqual(new Vec2i(4, 6), new Vec2i(1, 2) + new Vec2i(3, 4));
        Assert.AreEqual(new Vec3i(5, 7, 9), new Vec3i(1, 2, 3) + new Vec3i(4, 5, 6));
        Assert.AreEqual(new Vec4i(6, 8, 10, 12), new Vec4i(1, 2, 3, 4) + new Vec4i(5, 6, 7, 8));

        Assert.AreEqual(new Vec2u(4u, 6u), new Vec2u(1u, 2u) + new Vec2u(3u, 4u));
        Assert.AreEqual(new Vec3u(5u, 7u, 9u), new Vec3u(1u, 2u, 3u) + new Vec3u(4u, 5u, 6u));
        Assert.AreEqual(new Vec4u(6u, 8u, 10u, 12u), new Vec4u(1u, 2u, 3u, 4u) + new Vec4u(5u, 6u, 7u, 8u));

        Assert.AreEqual(new Vec2i64(4L, 6L), new Vec2i64(1L, 2L) + new Vec2i64(3L, 4L));
        Assert.AreEqual(new Vec3i64(5L, 7L, 9L), new Vec3i64(1L, 2L, 3L) + new Vec3i64(4L, 5L, 6L));
        Assert.AreEqual(new Vec4i64(6L, 8L, 10L, 12L), new Vec4i64(1L, 2L, 3L, 4L) + new Vec4i64(5L, 6L, 7L, 8L));

        Assert.AreEqual(new Vec2u64(4UL, 6UL), new Vec2u64(1UL, 2UL) + new Vec2u64(3UL, 4UL));
        Assert.AreEqual(new Vec3u64(5UL, 7UL, 9UL), new Vec3u64(1UL, 2UL, 3UL) + new Vec3u64(4UL, 5UL, 6UL));
        Assert.AreEqual(new Vec4u64(6UL, 8UL, 10UL, 12UL), new Vec4u64(1UL, 2UL, 3UL, 4UL) + new Vec4u64(5UL, 6UL, 7UL, 8UL));

        Assert.AreEqual(
            new Vec2i128((Int128)4, (Int128)6),
            new Vec2i128((Int128)1, (Int128)2) + new Vec2i128((Int128)3, (Int128)4));
        Assert.AreEqual(
            new Vec3i128((Int128)5, (Int128)7, (Int128)9),
            new Vec3i128((Int128)1, (Int128)2, (Int128)3) + new Vec3i128((Int128)4, (Int128)5, (Int128)6));
        Assert.AreEqual(
            new Vec4i128((Int128)6, (Int128)8, (Int128)10, (Int128)12),
            new Vec4i128((Int128)1, (Int128)2, (Int128)3, (Int128)4) + new Vec4i128((Int128)5, (Int128)6, (Int128)7, (Int128)8));

        Assert.AreEqual(
            new Vec2u128((UInt128)4, (UInt128)6),
            new Vec2u128((UInt128)1, (UInt128)2) + new Vec2u128((UInt128)3, (UInt128)4));
        Assert.AreEqual(
            new Vec3u128((UInt128)5, (UInt128)7, (UInt128)9),
            new Vec3u128((UInt128)1, (UInt128)2, (UInt128)3) + new Vec3u128((UInt128)4, (UInt128)5, (UInt128)6));
        Assert.AreEqual(
            new Vec4u128((UInt128)6, (UInt128)8, (UInt128)10, (UInt128)12),
            new Vec4u128((UInt128)1, (UInt128)2, (UInt128)3, (UInt128)4) + new Vec4u128((UInt128)5, (UInt128)6, (UInt128)7, (UInt128)8));
    }

    /// <summary>Generated Boolean vector masks are present for every size even though masks intentionally do not support addition.</summary>
    [TestMethod]
    public void GeneratedBooleanVectorTypes_ArePresentForEverySize()
    {
        Assert.AreEqual(new Vec2b(true, true), new Vec2b(true, false) | new Vec2b(false, true));
        Assert.AreEqual(new Vec3b(true, true, true), new Vec3b(true, false, true) | new Vec3b(false, true, false));
        Assert.AreEqual(new Vec4b(true, true, true, true), new Vec4b(true, false, true, false) | new Vec4b(false, true, false, true));
        Assert.AreEqual(new Vec3b(false, true, false), ~new Vec3b(true, false, true));
    }

    /// <summary>Generated vector interfaces expose construction, mutation, copying, identities, operators, and masks to generic code.</summary>
    [TestMethod]
    public void GeneratedVectorInterfaces_Work()
    {
        var scalar = CreateScalar<Vec3, float>(2f);
        var components = CreateVec3<Vec3, float>(1f, 2f, 3f);
        var mask = CreateVec3<Vec3b, bool>(true, false, true);
        Span<float> copied = stackalloc float[3];
        SetComponent(ref components, 1, 9f);
        CopyGeneric<Vec3, float>(components, copied);

        Assert.AreEqual(3, ComponentCount<Vec3, float>());
        Assert.AreEqual(12, SizeInBytes<Vec3, float>());
        Assert.AreEqual(new Vec3(2f), scalar);
        Assert.AreEqual(new Vec3(1f, 9f, 3f), components);
        Assert.AreEqual(new Vec3b(true, false, true), mask);
        Assert.AreEqual(9f, copied[1]);
        Assert.AreEqual((1f, 9f, 3f), Tuple3<Vec3, float>(components));
        Assert.AreEqual((1f, 9f, 3f), Deconstruct3<Vec3, float>(components));
        Assert.AreEqual(Vec3.Zero, AdditiveIdentity<Vec3>());
        Assert.AreEqual(Vec3.One, MultiplicativeIdentity<Vec3>());
        Assert.AreEqual(Vec3.UnitZ, UnitZGeneric<Vec3>());
        Assert.AreEqual(Vec3b.True, TrueMask<Vec3b>());
        Assert.AreEqual(Vec3b.False, FalseMask<Vec3b>());
        Assert.AreEqual(new Vec3(1f, 0f, 3f), SelectVec3Mask<Vec3b>(mask, new Vec3(1f, 2f, 3f), Vec3.Zero));
        Assert.AreEqual(new Vec3(3f), Add<Vec3, float>(scalar, 1f));
        Assert.AreEqual(new Vec3(3f, 11f, 5f), Add<Vec3, Vec3>(components, scalar));
        Assert.AreEqual(new Vec3b(true, false, false), LessThan<Vec3, Vec3b>(components, new Vec3(2f, 9f, 3f)));
        Assert.AreEqual(new Vec3b(true, false, true), EqualMask<Vec3, Vec3b>(components, new Vec3(1f, 0f, 3f)));
        Assert.IsTrue(EqualBaseVector<Vec3, float>(components, components));
        Assert.AreEqual(new Vec3i(0, 1, 2), Modulo<Vec3i, int>(new Vec3i(3, 4, 5), 3));
        Assert.AreEqual(new Vec3i(2, 4, 8), ShiftSelf<Vec3i, int, Vec3b, Vec3i, float>(new Vec3i(1, 1, 1), new Vec3i(1, 2, 3)));
        Assert.AreEqual(new Vec3u(2u, 4u, 8u), ShiftCount<Vec3u, Vec3i>(new Vec3u(1u, 1u, 1u), new Vec3i(1, 2, 3)));
        Assert.AreEqual(
            new Vec3i(int.MaxValue, 2147483646, 1073741822),
            UnsignedShiftSelf<Vec3i, int, Vec3b, Vec3i, float>(new Vec3i(-2, -4, -8), new Vec3i(1, 1, 2)));
        Assert.AreEqual(new Vec3u(4u, 4u, 4u), UnsignedShiftCount<Vec3u, Vec3i>(new Vec3u(8u, 16u, 32u), new Vec3i(1, 2, 3)));
        Assert.AreEqual(new Vec3i(0b1000, 0b1010, 0b0010), BitwiseAnd<Vec3i, int>(new Vec3i(0b1100, 0b1010, 0b0110), 0b1010));
        Assert.AreEqual(new Vec3b(false, true, false), BitwiseComplementMask(mask));
        Assert.IsTrue(EqualVector(new Vec3b(true, false, true), mask));
        Assert.AreEqual(Vec3.Zero, Zero<Vec3, float, Vec3b, float>());
        Assert.AreEqual(new Vec3(3f), AddNumeric<Vec3, float, Vec3b, float>(scalar, Vec3.One));
        Assert.AreEqual(new Vec3(3f), AddScalarLeft<Vec3, float>(1f, scalar));
        Assert.AreEqual(new Vec3(1f, 4f, 9f), ClampNumeric<Vec3, float, Vec3b, float>(new Vec3(-1f, 4f, 12f), Vec3.One, new Vec3(9f)));
        Assert.AreEqual(new Vec3(1f, 4f, 9f), ClampScalar<Vec3, float>(new Vec3(-1f, 4f, 12f), 1f, 9f));
        Assert.AreEqual(91f, DotMetric<Vec3, float, float>(components, components));
        Assert.AreEqual(91f, LengthSquaredMetric<Vec3, float, float>(components));
        Assert.AreEqual(new Vec3(0f, -3f, 9f), CrossGeneric<Vec3, float>(Vec3.UnitX, components));
        Assert.AreEqual(1f, PerpDotPlanar<Vec2, float>(Vec2.UnitX, Vec2.UnitY));
        Assert.AreEqual(new Vec3i(1, 2, 3), AbsSigned<Vec3i, int, Vec3b, float>(new Vec3i(-1, -2, -3)));
        Assert.AreEqual(new Vec3i(2, 0, 1), BitCountGeneric<Vec3u, uint, Vec3b, Vec3i, float>(new Vec3u(3u, 0u, 8u)));
        Assert.AreEqual(new Vec3u(1u, 0u, 2u), BitwiseScalarLeft<Vec3u, uint>(3u, new Vec3u(5u, 8u, 6u)));
        Assert.AreEqual(new Vec3(2f, 2f, 3f), FloorFloating<Vec3, float, Vec3b>(new Vec3(2.75f, 2.25f, 3.5f)));
        Assert.AreEqual(new Vec3(2f, 1f, 0f), ModScalarFloating<Vec3, float>(new Vec3(5f, 4f, 3f), 3f));
        Assert.AreEqual(new Vec3(4f, 9f, 16f), PowScalarFloating<Vec3, float>(new Vec3(2f, 3f, 4f), 2f));
        Assert.AreEqual(new Vec3(1f, 5f, 9f), LerpVectorInterpolation<Vec3>(Vec3.Zero, new Vec3(10f), new Vec3(0.1f, 0.5f, 0.9f)));
        Assert.AreEqual(new Vec3i(2, 2, 3), FloorToInt<Vec3, Vec3i>(new Vec3(2.75f, 2.25f, 3.5f)));
        Assert.AreEqual(new Vec3(1f, 0f, 0f), NormalizeVec3Floating<Vec3, float, Vec3b>(new Vec3(4f, 0f, 0f)));
        Assert.AreEqual(new Vec3b(true, true, true), IsFiniteFloating<Vec3, float, Vec3b>(components));
        Assert.AreEqual(new Vec3(1f, 0f, 0f), NormalizeGeometry<Vec3, float>(new Vec3(4f, 0f, 0f)));
        Assert.AreEqual(new System.Numerics.Vector3(1f, 9f, 3f), ToSystemNumerics3<Vec3>(components));
        Assert.AreEqual(new Vec3(1f, 9f, 3f), FromSystemNumerics3<Vec3>(new System.Numerics.Vector3(1f, 9f, 3f)));
        Assert.IsTrue(AllVec3Mask<Vec3b>(new Vec3b(true, true, true)));
        Assert.AreEqual(new Vec3b(false, true, false), NotMask<Vec3b>(mask));
    }

    /// <summary>Generated Boolean masks support C# conditional logical operators with component-wise results.</summary>
    [TestMethod]
    public void GeneratedBooleanConditionalOperators_Work()
    {
        var left = new Vec3b(true, false, false);
        var right = new Vec3b(false, true, true);
        var all = new Vec3b(true, true, true);
        var none = new Vec3b(false, false, false);

        Assert.AreEqual(new Vec3b(false, false, false), left && right);
        Assert.AreEqual(new Vec3b(true, true, true), left || right);
        Assert.IsTrue(all ? true : false);
        Assert.IsFalse(none ? true : false);
    }

    /// <summary>Generated two-component vectors include planar helpers common in game math APIs.</summary>
    [TestMethod]
    public void GeneratedPlanarHelpers_Work()
    {
        var value = new Vec2(2f, 3f);

        Assert.AreEqual(new Vec2(-3f, 2f), value.PerpendicularLeft);
        Assert.AreEqual(new Vec2(3f, -2f), value.PerpendicularRight);
        Assert.AreEqual(1f, Vec2.Cross(Vec2.UnitX, Vec2.UnitY));
        Assert.AreEqual(1f, Vec2.PerpDot(Vec2.UnitX, Vec2.UnitY));
    }

    private static int ComponentCount<TVector, TScalar>()
        where TVector : struct, IVec<TVector, TScalar> =>
        TVector.ComponentCount;

    private static TVector ParseSpan<TVector>(ReadOnlySpan<char> source)
        where TVector : ISpanParsable<TVector> =>
        TVector.Parse(source, System.Globalization.CultureInfo.InvariantCulture);

    private static TVector ParseUtf8<TVector>(ReadOnlySpan<byte> source)
        where TVector : IUtf8SpanParsable<TVector> =>
        TVector.Parse(source, System.Globalization.CultureInfo.InvariantCulture);

    private static int SizeInBytes<TVector, TScalar>()
        where TVector : struct, IVec<TVector, TScalar> =>
        TVector.SizeInBytes;

    private static TVector CreateScalar<TVector, TScalar>(TScalar value)
        where TVector : struct, IVec<TVector, TScalar> =>
        TVector.Create(value);

    private static TVector CreateVec3<TVector, TScalar>(TScalar x, TScalar y, TScalar z)
        where TVector : struct, IVec3<TVector, TScalar> =>
        TVector.Create(x, y, z);

    private static (TScalar X, TScalar Y, TScalar Z) Tuple3<TVector, TScalar>(TVector value)
        where TVector : struct, IVec3<TVector, TScalar>
    {
        (TScalar X, TScalar Y, TScalar Z) tuple = value;
        return tuple;
    }

    private static (TScalar X, TScalar Y, TScalar Z) Deconstruct3<TVector, TScalar>(TVector value)
        where TVector : struct, IVec3<TVector, TScalar>
    {
        value.Deconstruct(out var x, out var y, out var z);
        return (x, y, z);
    }

    private static void SetComponent<TVector, TScalar>(ref TVector value, int index, TScalar component)
        where TVector : struct, IVec<TVector, TScalar> =>
        TVector.ComponentRef(ref value, index) = component;

    private static void CopyGeneric<TVector, TScalar>(TVector value, Span<TScalar> destination)
        where TVector : struct, IVec<TVector, TScalar> =>
        value.CopyTo(destination);

    private static TVector AdditiveIdentity<TVector>()
        where TVector : IAdditiveIdentity<TVector, TVector> =>
        TVector.AdditiveIdentity;

    private static TVector MultiplicativeIdentity<TVector>()
        where TVector : IMultiplicativeIdentity<TVector, TVector> =>
        TVector.MultiplicativeIdentity;

    private static TVector UnitZGeneric<TVector>()
        where TVector : struct, IVec3Axes<TVector> =>
        TVector.UnitZ;

    private static TMask TrueMask<TMask>()
        where TMask : struct, IVecMask<TMask> =>
        TMask.True;

    private static TMask FalseMask<TMask>()
        where TMask : struct, IVecMask<TMask> =>
        TMask.False;

    private static Vec3 SelectVec3Mask<TMask>(TMask mask, Vec3 whenTrue, Vec3 whenFalse)
        where TMask : struct, IVec3Mask<TMask> =>
        mask.Select(whenTrue, whenFalse);

    private static TVector Add<TVector, TValue>(TVector left, TValue right)
        where TVector : IAdditionOperators<TVector, TValue, TVector> =>
        left + right;

    private static TMask LessThan<TVector, TMask>(TVector left, TVector right)
        where TVector : struct, IVecRelationalOperators<TVector, TMask>
        where TMask : struct =>
        left < right;

    private static TMask EqualMask<TVector, TMask>(TVector left, TVector right)
        where TVector : struct, IVecRelationalOperators<TVector, TMask>
        where TMask : struct =>
        TVector.Equal(left, right);

    private static bool EqualBaseVector<TVector, TScalar>(TVector left, TVector right)
        where TVector : struct, IVec<TVector, TScalar> =>
        left == right;

    private static TVector Modulo<TVector, TValue>(TVector left, TValue right)
        where TVector : IModulusOperators<TVector, TValue, TVector> =>
        left % right;

    private static TVector ShiftSelf<TVector, TScalar, TMask, TCount, TLength>(TVector left, TVector right)
        where TVector : struct, IVecInteger<TVector, TScalar, TMask, TCount, TLength>
        where TMask : struct, IVecMask<TMask>
        where TCount : struct, IVec<TCount, int> =>
        left << right;

    private static TVector ShiftCount<TVector, TCount>(TVector left, TCount right)
        where TVector : struct, IVecIntegerCountShiftOperators<TVector, TCount>
        where TCount : struct, IVec<TCount, int> =>
        left << right;

    private static TVector UnsignedShiftSelf<TVector, TScalar, TMask, TCount, TLength>(TVector left, TVector right)
        where TVector : struct, IVecInteger<TVector, TScalar, TMask, TCount, TLength>
        where TMask : struct, IVecMask<TMask>
        where TCount : struct, IVec<TCount, int> =>
        left >>> right;

    private static TVector UnsignedShiftCount<TVector, TCount>(TVector left, TCount right)
        where TVector : struct, IVecIntegerCountShiftOperators<TVector, TCount>
        where TCount : struct, IVec<TCount, int> =>
        left >>> right;

    private static TVector BitwiseAnd<TVector, TValue>(TVector left, TValue right)
        where TVector : IBitwiseOperators<TVector, TValue, TVector> =>
        left & right;

    private static TVector BitwiseComplementMask<TVector>(TVector value)
        where TVector : IBitwiseOperators<TVector, TVector, TVector> =>
        ~value;

    private static bool EqualVector<TVector>(TVector left, TVector right)
        where TVector : IEqualityOperators<TVector, TVector, bool> =>
        left == right;

    private static TVector Zero<TVector, TScalar, TMask, TLength>()
        where TVector : struct, IVecNumeric<TVector, TScalar, TMask, TLength>
        where TMask : struct, IVecMask<TMask> =>
        TVector.Zero;

    private static TVector AddNumeric<TVector, TScalar, TMask, TLength>(TVector left, TVector right)
        where TVector : struct, IVecNumeric<TVector, TScalar, TMask, TLength>
        where TMask : struct, IVecMask<TMask> =>
        left + right;

    private static TVector AddScalarLeft<TVector, TScalar>(TScalar left, TVector right)
        where TVector : struct, IVecScalarArithmeticOperators<TVector, TScalar> =>
        left + right;

    private static TVector ClampScalar<TVector, TScalar>(TVector value, TScalar min, TScalar max)
        where TVector : struct, IVecScalarArithmeticOperators<TVector, TScalar> =>
        TVector.Clamp(value, min, max);

    private static TVector ClampNumeric<TVector, TScalar, TMask, TLength>(TVector value, TVector min, TVector max)
        where TVector : struct, IVecNumeric<TVector, TScalar, TMask, TLength>
        where TMask : struct, IVecMask<TMask> =>
        TVector.Clamp(value, min, max);

    private static TScalar DotMetric<TVector, TScalar, TLength>(TVector left, TVector right)
        where TVector : struct, IVecMetric<TVector, TScalar, TLength> =>
        TVector.Dot(left, right);

    private static TScalar LengthSquaredMetric<TVector, TScalar, TLength>(TVector value)
        where TVector : struct, IVecMetric<TVector, TScalar, TLength> =>
        value.LengthSquared;

    private static TVector CrossGeneric<TVector, TScalar>(TVector left, TVector right)
        where TVector : struct, IVec3Cross<TVector, TScalar> =>
        TVector.Cross(left, right);

    private static TScalar PerpDotPlanar<TVector, TScalar>(TVector left, TVector right)
        where TVector : struct, IVec2Planar<TVector, TScalar> =>
        TVector.PerpDot(left, right);

    private static TVector AbsSigned<TVector, TScalar, TMask, TLength>(TVector value)
        where TVector : struct, IVecSignedNumeric<TVector, TScalar, TMask, TLength>
        where TMask : struct, IVecMask<TMask> =>
        TVector.Abs(value);

    private static TCount BitCountGeneric<TVector, TScalar, TMask, TCount, TLength>(TVector value)
        where TVector : struct, IVecInteger<TVector, TScalar, TMask, TCount, TLength>
        where TMask : struct, IVecMask<TMask>
        where TCount : struct, IVec<TCount, int> =>
        TVector.BitCount(value);

    private static TVector BitwiseScalarLeft<TVector, TScalar>(TScalar left, TVector right)
        where TVector : struct, IVecScalarIntegerOperators<TVector, TScalar> =>
        left & right;

    private static TVector FloorFloating<TVector, TScalar, TMask>(TVector value)
        where TVector : struct, IVecFloating<TVector, TScalar, TMask>
        where TMask : struct, IVecMask<TMask> =>
        TVector.Floor(value);

    private static TVector ModScalarFloating<TVector, TScalar>(TVector value, TScalar right)
        where TVector : struct, IVecFloatingScalarFunctions<TVector, TScalar> =>
        TVector.Mod(value, right);

    private static TVector PowScalarFloating<TVector, TScalar>(TVector value, TScalar exponent)
        where TVector : struct, IVecFloatingScalarFunctions<TVector, TScalar> =>
        TVector.Pow(value, exponent);

    private static TVector LerpVectorInterpolation<TVector>(TVector from, TVector to, TVector amount)
        where TVector : struct, IVecFloatingVectorInterpolation<TVector> =>
        TVector.Lerp(from, to, amount);

    private static TInteger FloorToInt<TVector, TInteger>(TVector value)
        where TVector : struct, IVec3FloatingToInteger<TInteger>
        where TInteger : struct, IVec3<TInteger, int> =>
        value.FloorToVec3i();

    private static TVector NormalizeVec3Floating<TVector, TScalar, TMask>(TVector value)
        where TVector : struct, IVec3Floating<TVector, TScalar, TMask>
        where TMask : struct, IVec3Mask<TMask> =>
        TVector.Normalize(value);

    private static TMask IsFiniteFloating<TVector, TScalar, TMask>(TVector value)
        where TVector : struct, IVecFloating<TVector, TScalar, TMask>
        where TMask : struct, IVecMask<TMask> =>
        TVector.IsFinite(value);

    private static TVector NormalizeGeometry<TVector, TScalar>(TVector value)
        where TVector : struct, IVecFloatingGeometry<TVector, TScalar> =>
        TVector.Normalize(value);

    private static System.Numerics.Vector3 ToSystemNumerics3<TVector>(TVector value)
        where TVector : struct, IVec3SystemNumerics<TVector>
    {
        System.Numerics.Vector3 systemVector = value;
        return systemVector;
    }

    private static TVector FromSystemNumerics3<TVector>(System.Numerics.Vector3 value)
        where TVector : struct, IVec3SystemNumerics<TVector>
    {
        TVector vector = value;
        return vector;
    }

    private static bool AllVec3Mask<TMask>(TMask value)
        where TMask : struct, IVec3Mask<TMask> =>
        value.All;

    private static TMask NotMask<TMask>(TMask value)
        where TMask : struct, IVecMask<TMask> =>
        !value;
}
