namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated math source planning and focused source emission.</summary>
[TestClass]
public sealed class MathsGeneratorTest
{
    /// <summary>Configured type names follow the AlvorKit vector, matrix, quaternion, plane, and box suffix conventions.</summary>
    [TestMethod]
    public void Catalogs_UseExpectedNames()
    {
        var vectorNames = VectorCatalog.Vectors.Select(vector => vector.TypeName).ToArray();
        var matrixNames = MatrixCatalog.Matrices.Select(matrix => matrix.TypeName).ToArray();
        var quaternionNames = QuaternionCatalog.Quaternions.Select(quaternion => quaternion.TypeName).ToArray();
        var planeNames = PlaneCatalog.Planes.Select(plane => plane.TypeName).ToArray();
        var frustumNames = FrustumCatalog.Frustums.Select(frustum => frustum.TypeName).ToArray();
        var intervalNames = IntervalCatalog.Intervals.Select(interval => interval.TypeName).ToArray();
        var rayNames = RayCatalog.Rays.Select(ray => ray.TypeName).ToArray();
        var boxNames = BoxCatalog.Boxes.Select(box => box.TypeName).ToArray();

        Assert.AreEqual(42, vectorNames.Length);
        Assert.AreEqual(18, matrixNames.Length);
        Assert.AreEqual(2, quaternionNames.Length);
        Assert.AreEqual(2, planeNames.Length);
        Assert.AreEqual(2, frustumNames.Length);
        Assert.AreEqual(2, intervalNames.Length);
        Assert.AreEqual(2, rayNames.Length);
        Assert.AreEqual(6, boxNames.Length);
        CollectionAssert.Contains(vectorNames, "Vec3h");
        CollectionAssert.Contains(vectorNames, "Vec4u128");
        CollectionAssert.Contains(vectorNames, "Vec2b");
        CollectionAssert.Contains(matrixNames, "Mat4x3d");
        CollectionAssert.Contains(quaternionNames, "Quatd");
        CollectionAssert.Contains(planeNames, "Plane3d");
        CollectionAssert.Contains(frustumNames, "Frustum3d");
        CollectionAssert.Contains(intervalNames, "Intervald");
        CollectionAssert.Contains(rayNames, "Ray3d");
        CollectionAssert.Contains(boxNames, "Box3i");
    }

    /// <summary>Scalar specs expose naming and numeric metadata used across generated maths families.</summary>
    [TestMethod]
    public void ScalarSpecs_ExposeExpectedMetadata()
    {
        Assert.IsTrue(VectorCatalog.Bool.IsBool);
        Assert.IsTrue(VectorCatalog.Float.IsFloating);
        Assert.IsTrue(VectorCatalog.Int.IsInteger);
        Assert.IsTrue(VectorCatalog.Int.IsSigned);
        Assert.IsFalse(VectorCatalog.UInt.IsSigned);
        Assert.IsTrue(VectorCatalog.Half.RequiresArithmeticCast);
        Assert.IsFalse(VectorCatalog.Double.RequiresArithmeticCast);
        Assert.AreEqual(8, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int8).BitWidth);
        Assert.AreEqual(8, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt8).BitWidth);
        Assert.AreEqual(16, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int16).BitWidth);
        Assert.AreEqual(16, VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt16).BitWidth);
        Assert.AreEqual(32, VectorCatalog.Int.BitWidth);
        Assert.AreEqual(32, VectorCatalog.UInt.BitWidth);
        Assert.AreEqual(64, VectorCatalog.Int64.BitWidth);
        Assert.AreEqual(64, VectorCatalog.UInt64.BitWidth);
        Assert.AreEqual(128, VectorCatalog.Int128.BitWidth);
        Assert.AreEqual(128, VectorCatalog.UInt128.BitWidth);
        Assert.AreEqual(0, VectorCatalog.Float.BitWidth);
        Assert.AreEqual("(Half)0", VectorCatalog.Half.ZeroLiteral);
        Assert.AreEqual("(byte)0", VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.UInt8).ZeroLiteral);
        Assert.AreEqual("0", VectorCatalog.Bool.ZeroLiteral);
        Assert.AreEqual("Vec3", VectorCatalog.Float.VectorName(3));
        Assert.AreEqual("Mat2x3", VectorCatalog.Float.MatrixName(2, 3));
        Assert.AreEqual("Quatd", VectorCatalog.Double.QuaternionName());
        Assert.AreEqual("Plane3", VectorCatalog.Float.PlaneName());
        Assert.AreEqual("Plane3d", VectorCatalog.Double.PlaneName());
        Assert.AreEqual("Frustum3", VectorCatalog.Float.FrustumName());
        Assert.AreEqual("Frustum3d", VectorCatalog.Double.FrustumName());
        Assert.AreEqual("Intervalf", VectorCatalog.Float.IntervalName());
        Assert.AreEqual("Intervald", VectorCatalog.Double.IntervalName());
        Assert.AreEqual("Ray3", VectorCatalog.Float.RayName());
        Assert.AreEqual("Ray3d", VectorCatalog.Double.RayName());
        Assert.AreEqual("Box3i", VectorCatalog.Int.BoxName(3));
        Assert.AreEqual("(sbyte)(x + y)", VectorCatalog.Scalars.Single(scalar => scalar.Kind == ScalarKind.Int8).CastArithmetic("x + y"));
        Assert.AreEqual("x + y", VectorCatalog.Int.CastArithmetic("x + y"));
    }

    /// <summary>Floating vector source includes tuple conversions, System.Numerics interop, and parsing helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedFloatingVectorFeatures()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));

        StringAssert.Contains(vec3, "public partial struct Vec3(float x, float y, float z)");
        StringAssert.Contains(vec3, "IVec3Floating<Vec3, float, Vec3b>");
        StringAssert.Contains(vec3, "public static implicit operator Vec3((float X, float Y, float Z) value)");
        StringAssert.Contains(vec2, $"public static implicit operator Vec2(System.Numerics.Vector2 value) =>{Environment.NewLine}" +
            "        new(value);");
        StringAssert.Contains(vec2, $"public static implicit operator System.Numerics.Vector2(Vec2 value) =>{Environment.NewLine}" +
            "        value.packed;");
        StringAssert.Contains(vec3, $"public static implicit operator Vec3(System.Numerics.Vector3 value) =>{Environment.NewLine}" +
            "        new(value);");
        StringAssert.Contains(vec3, $"public static implicit operator System.Numerics.Vector3(Vec3 value) =>{Environment.NewLine}" +
            "        value.packed;");
        StringAssert.Contains(vec4, $"public static implicit operator Vec4(System.Numerics.Vector4 value) =>{Environment.NewLine}" +
            "        new(value);");
        StringAssert.Contains(vec4, $"public static implicit operator System.Numerics.Vector4(Vec4 value) =>{Environment.NewLine}" +
            "        value.packed;");
        StringAssert.Contains(vec4, $"[FieldOffset(0)]{Environment.NewLine}" +
            "    private System.Numerics.Vector4 packed;");
        StringAssert.Contains(vec3, "public static Vec3d operator /(double left, Vec3 right)");
        StringAssert.Contains(vec3, "MathsParseHelper.TryParseComponent(body.Trim(), formatProvider, out float z)");
    }

    /// <summary>Interpolation uses only the measured exact formula for each scalar and layout.</summary>
    [TestMethod]
    public void VectorEmitter_UsesExactOperatorFormulasOnlyForFloatInterpolation()
    {
        AssertFloatInterpolation(VectorFileEmitter.Emit(new(2, VectorCatalog.Float)), "Vec2");
        AssertFloatInterpolation(VectorFileEmitter.Emit(new(3, VectorCatalog.Float)), "Vec3");
        AssertFloatInterpolation(VectorFileEmitter.Emit(new(4, VectorCatalog.Float)), "Vec4");

        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        var vec3h = VectorFileEmitter.Emit(new(3, VectorCatalog.Half));
        StringAssert.Contains(vec3d, $"public static Vec3d Lerp(Vec3d from, Vec3d to, double amount) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Lerp(from.X, to.X, amount),{Environment.NewLine}" +
            $"            ScalarMath.Lerp(from.Y, to.Y, amount),{Environment.NewLine}" +
            "            ScalarMath.Lerp(from.Z, to.Z, amount));");
        StringAssert.Contains(vec3h, $"public static Vec3h Lerp(Vec3h from, Vec3h to, Half amount) =>{Environment.NewLine}" +
            "        from + ((to - from) * amount);");
        StringAssert.Contains(vec3h, $"[MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}" +
            "    public static Vec3h Lerp(Vec3h from, Vec3h to, Half amount)");
        StringAssert.Contains(vec3h, $"public static Vec3h Barycentric(Vec3h a, Vec3h b, Vec3h c, Half u, Half v) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (Half)(ScalarMath.Barycentric(a.X, b.X, c.X, u, v)),{Environment.NewLine}" +
            $"            (Half)(ScalarMath.Barycentric(a.Y, b.Y, c.Y, u, v)),{Environment.NewLine}" +
            "            (Half)(ScalarMath.Barycentric(a.Z, b.Z, c.Z, u, v)));");
        Assert.IsFalse(vec3d.Contains("from + ((to - from) * amount)", StringComparison.Ordinal));
        Assert.IsFalse(vec3h.Contains("a + ((b - a) * u) + ((c - a) * v)", StringComparison.Ordinal));
    }

    /// <summary>Supported single-precision vector-pair arithmetic delegates directly to the matching System.Numerics vector.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsForSupportedFloatVectorPairArithmetic()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        StringAssert.Contains(vec2, $"public static Vec2 operator +(Vec2 left, Vec2 right) =>{Environment.NewLine}" +
            "        new(left.packed + right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator +(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            "        new(left.packed + right.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator +(Vec4 left, Vec4 right) =>{Environment.NewLine}" +
            "        new(left.packed + right.packed);");
        StringAssert.Contains(vec2, $"public static Vec2 operator -(Vec2 left, Vec2 right) =>{Environment.NewLine}" +
            "        new(left.packed - right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator -(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            "        new(left.packed - right.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator -(Vec4 left, Vec4 right) =>{Environment.NewLine}" +
            "        new(left.packed - right.packed);");
        StringAssert.Contains(vec2, $"public static Vec2 operator *(Vec2 left, Vec2 right) =>{Environment.NewLine}" +
            "        new(left.packed * right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator *(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            "        new(left.packed * right.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator *(Vec4 left, Vec4 right) =>{Environment.NewLine}" +
            "        new(left.packed * right.packed);");
        StringAssert.Contains(vec2, $"public static Vec2 operator /(Vec2 left, Vec2 right) =>{Environment.NewLine}" +
            "        new(left.packed / right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator /(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            "        new(left.packed / right.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator /(Vec4 left, Vec4 right) =>{Environment.NewLine}" +
            "        new(left.packed / right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator +(Vec3 left, float right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (float)left.X + (float)right,{Environment.NewLine}" +
            $"            (float)left.Y + (float)right,{Environment.NewLine}" +
            "            (float)left.Z + (float)right);");
        StringAssert.Contains(vec3, $"public static Vec3 operator %(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (float)left.X % (float)right.X,{Environment.NewLine}" +
            $"            (float)left.Y % (float)right.Y,{Environment.NewLine}" +
            "            (float)left.Z % (float)right.Z);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator +(Vec3d left, Vec3d right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (double)left.X + (double)right.X,{Environment.NewLine}" +
            $"            (double)left.Y + (double)right.Y,{Environment.NewLine}" +
            "            (double)left.Z + (double)right.Z);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator /(Vec3d left, Vec3d right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (double)left.X / (double)right.X,{Environment.NewLine}" +
            $"            (double)left.Y / (double)right.Y,{Environment.NewLine}" +
            "            (double)left.Z / (double)right.Z);");
    }

    /// <summary>Supported single-precision vector-right-scalar arithmetic delegates directly to the matching System.Numerics vector.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsForSupportedFloatVectorScalarArithmetic()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        StringAssert.Contains(vec2, $"public static Vec2 operator /(Vec2 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed / right);");
        StringAssert.Contains(vec3, $"public static Vec3 operator /(Vec3 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed / right);");
        StringAssert.Contains(vec4, $"public static Vec4 operator /(Vec4 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed / right);");
        StringAssert.Contains(vec2, $"public static Vec2 operator *(Vec2 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed * right);");
        StringAssert.Contains(vec3, $"public static Vec3 operator *(Vec3 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed * right);");
        StringAssert.Contains(vec4, $"public static Vec4 operator *(Vec4 left, float right) =>{Environment.NewLine}" +
            "        new(left.packed * right);");
        StringAssert.Contains(vec3, $"public static Vec3 operator /(float left, Vec3 right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (float)left / (float)right.X,{Environment.NewLine}" +
            $"            (float)left / (float)right.Y,{Environment.NewLine}" +
            "            (float)left / (float)right.Z);");
        StringAssert.Contains(vec2, $"public static Vec2 operator *(float left, Vec2 right) =>{Environment.NewLine}" +
            "        new(left * right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator *(float left, Vec3 right) =>{Environment.NewLine}" +
            "        new(left * right.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator *(float left, Vec4 right) =>{Environment.NewLine}" +
            "        new(left * right.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator %(float left, Vec3 right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (float)left % (float)right.X,{Environment.NewLine}" +
            $"            (float)left % (float)right.Y,{Environment.NewLine}" +
            "            (float)left % (float)right.Z);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator /(Vec3d left, double right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (double)left.X / (double)right,{Environment.NewLine}" +
            $"            (double)left.Y / (double)right,{Environment.NewLine}" +
            "            (double)left.Z / (double)right);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator *(Vec3d left, double right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (double)left.X * (double)right,{Environment.NewLine}" +
            $"            (double)left.Y * (double)right,{Environment.NewLine}" +
            "            (double)left.Z * (double)right);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator *(double left, Vec3d right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (double)left * (double)right.X,{Environment.NewLine}" +
            $"            (double)left * (double)right.Y,{Environment.NewLine}" +
            "            (double)left * (double)right.Z);");
    }

    /// <summary>Single-precision unary negation alone delegates directly to the matching System.Numerics vector.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatUnaryNegation()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        StringAssert.Contains(vec2, $"public static Vec2 operator -(Vec2 value) =>{Environment.NewLine}" +
            "        new(-value.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator -(Vec3 value) =>{Environment.NewLine}" +
            "        new(-value.packed);");
        StringAssert.Contains(vec4, $"public static Vec4 operator -(Vec4 value) =>{Environment.NewLine}" +
            "        new(-value.packed);");
        StringAssert.Contains(vec3, $"public static Vec3 operator +(Vec3 value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            +value.X,{Environment.NewLine}" +
            $"            +value.Y,{Environment.NewLine}" +
            "            +value.Z);");
        StringAssert.Contains(vec3, $"public static Vec3 operator ++(Vec3 value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            value.X + 1f,{Environment.NewLine}" +
            $"            value.Y + 1f,{Environment.NewLine}" +
            "            value.Z + 1f);");
        StringAssert.Contains(vec3, $"public static Vec3 operator --(Vec3 value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            value.X - 1f,{Environment.NewLine}" +
            $"            value.Y - 1f,{Environment.NewLine}" +
            "            value.Z - 1f);");
        StringAssert.Contains(vec3d, $"public static Vec3d operator -(Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            -value.X,{Environment.NewLine}" +
            $"            -value.Y,{Environment.NewLine}" +
            "            -value.Z);");
    }

    /// <summary>Single-precision rounding delegates to matching System.Numerics operations while other floating families stay component-wise.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatRounding()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        AssertRounding(vec2, "Vec2", "Vector2");
        AssertRounding(vec3, "Vec3", "Vector3");
        AssertRounding(vec4, "Vec4", "Vector4");
        StringAssert.Contains(vec3d, $"public static Vec3d Truncate(Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Truncate(value.X),{Environment.NewLine}" +
            $"            ScalarMath.Truncate(value.Y),{Environment.NewLine}" +
            "            ScalarMath.Truncate(value.Z));");
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.Round", StringComparison.Ordinal));
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.Truncate", StringComparison.Ordinal));
    }

    /// <summary>Single-precision three-component cross product alone delegates to System.Numerics with the scalar formula as its fallback.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatVec3Cross()
    {
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        StringAssert.Contains(vec3, $"public static Vec3 Cross(Vec3 left, Vec3 right) =>{Environment.NewLine}" +
            $"        System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated{Environment.NewLine}" +
            $"            ? new(System.Numerics.Vector3.Cross(left.packed, right.packed)){Environment.NewLine}" +
            $"            : new({Environment.NewLine}" +
            $"                (left.Y * right.Z) - (left.Z * right.Y),{Environment.NewLine}" +
            $"                (left.Z * right.X) - (left.X * right.Z),{Environment.NewLine}" +
            "                (left.X * right.Y) - (left.Y * right.X));");
        StringAssert.Contains(vec3d, $"public static Vec3d Cross(Vec3d left, Vec3d right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (left.Y * right.Z) - (left.Z * right.Y),{Environment.NewLine}" +
            $"            (left.Z * right.X) - (left.X * right.Z),{Environment.NewLine}" +
            "            (left.X * right.Y) - (left.Y * right.X));");
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.Cross", StringComparison.Ordinal));
    }

    /// <summary>Single-precision min, max, and clamp follow regular System.Numerics semantics.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsFunctionsForFloatMinMaxAndClamp()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        AssertSystemMinMaxClamp(vec2, "Vec2", "Vector2");
        AssertSystemMinMaxClamp(vec3, "Vec3", "Vector3");
        AssertSystemMinMaxClamp(vec4, "Vec4", "Vector4");
        StringAssert.Contains(vec3d, $"public static Vec3d Min(Vec3d left, Vec3d right) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Min(left.X, right.X),{Environment.NewLine}" +
            $"            ScalarMath.Min(left.Y, right.Y),{Environment.NewLine}" +
            "            ScalarMath.Min(left.Z, right.Z));");
        Assert.IsFalse(vec3d.Contains("MinNative", StringComparison.Ordinal));
        Assert.IsFalse(vec3d.Contains("MaxNative", StringComparison.Ordinal));
    }

    /// <summary>Single-precision fused multiply-add uses System.Numerics in every supported dimension while other floating families stay scalar.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatFusedMultiplyAdd()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3h = VectorFileEmitter.Emit(new(3, VectorCatalog.Half));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        AssertFusedMultiplyAdd(vec2, "Vec2", "Vector2");
        AssertFusedMultiplyAdd(vec3, "Vec3", "Vector3");
        AssertFusedMultiplyAdd(vec4, "Vec4", "Vector4");
        StringAssert.Contains(vec3h,
            $"public static Vec3h FusedMultiplyAdd(Vec3h left, Vec3h right, Vec3h addend) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (Half)(ScalarMath.FusedMultiplyAdd(left.X, right.X, addend.X)),{Environment.NewLine}" +
            $"            (Half)(ScalarMath.FusedMultiplyAdd(left.Y, right.Y, addend.Y)),{Environment.NewLine}" +
            "            (Half)(ScalarMath.FusedMultiplyAdd(left.Z, right.Z, addend.Z)));");
        StringAssert.Contains(vec3d,
            $"public static Vec3d FusedMultiplyAdd(Vec3d left, Vec3d right, Vec3d addend) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.FusedMultiplyAdd(left.X, right.X, addend.X),{Environment.NewLine}" +
            $"            ScalarMath.FusedMultiplyAdd(left.Y, right.Y, addend.Y),{Environment.NewLine}" +
            "            ScalarMath.FusedMultiplyAdd(left.Z, right.Z, addend.Z));");
        Assert.IsFalse(vec3h.Contains("System.Numerics.Vector3.FusedMultiplyAdd", StringComparison.Ordinal));
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.FusedMultiplyAdd", StringComparison.Ordinal));
    }

    /// <summary>Single-precision square-root helpers use System.Numerics while other floating families keep their scalar formulas.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatSquareRoots()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec3h = VectorFileEmitter.Emit(new(3, VectorCatalog.Half));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));

        AssertSquareRoots(vec2, "Vec2", "Vector2");
        AssertSquareRoots(vec3, "Vec3", "Vector3");
        AssertSquareRoots(vec4, "Vec4", "Vector4");
        StringAssert.Contains(vec3h, $"public static Vec3h Sqrt(Vec3h value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (Half)(ScalarMath.Sqrt(value.X)),{Environment.NewLine}" +
            $"            (Half)(ScalarMath.Sqrt(value.Y)),{Environment.NewLine}" +
            "            (Half)(ScalarMath.Sqrt(value.Z)));");
        StringAssert.Contains(vec3h, $"public static Vec3h InverseSqrt(Vec3h value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            (Half)(ScalarMath.InverseSqrt(value.X)),{Environment.NewLine}" +
            $"            (Half)(ScalarMath.InverseSqrt(value.Y)),{Environment.NewLine}" +
            "            (Half)(ScalarMath.InverseSqrt(value.Z)));");
        StringAssert.Contains(vec3d, $"public static Vec3d Sqrt(Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Sqrt(value.X),{Environment.NewLine}" +
            $"            ScalarMath.Sqrt(value.Y),{Environment.NewLine}" +
            "            ScalarMath.Sqrt(value.Z));");
        StringAssert.Contains(vec3d, $"public static Vec3d InverseSqrt(Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.InverseSqrt(value.X),{Environment.NewLine}" +
            $"            ScalarMath.InverseSqrt(value.Y),{Environment.NewLine}" +
            "            ScalarMath.InverseSqrt(value.Z));");
        Assert.IsFalse(vec3h.Contains("System.Numerics.Vector3.SquareRoot", StringComparison.Ordinal));
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.SquareRoot", StringComparison.Ordinal));
    }

    /// <summary>Unsigned integer vector source includes cross-scalar arithmetic helpers without ambiguous vector-pair helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedUnsignedIntegerVectorFeatures()
    {
        var vec3u = VectorFileEmitter.Emit(new(3, VectorCatalog.UInt));

        StringAssert.Contains(vec3u, "public static Vec3d operator /(Vec3u left, double right)");
        StringAssert.Contains(vec3u, "public static Vec3i64 operator +(Vec3u left, int right)");
        Assert.IsFalse(vec3u.Contains("public static Vec3i64 operator +(Vec3u left, Vec3i right)", StringComparison.Ordinal));
    }

    /// <summary>Signed integer vector source includes count-vector shift helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedSignedIntegerVectorFeatures()
    {
        var vec3i = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));

        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, Vec3i right)");
    }

    /// <summary>Boolean vector source includes mask selection helpers.</summary>
    [TestMethod]
    public void VectorEmitter_EmitsExpectedMaskVectorFeatures()
    {
        var vec3b = VectorFileEmitter.Emit(new(3, VectorCatalog.Bool));

        StringAssert.Contains(vec3b, "public readonly Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse)");
    }

    /// <summary>Swizzle source is emitted outside the primary vector file.</summary>
    [TestMethod]
    public void SwizzleEmitter_EmitsExpectedSwizzleFeatures()
    {
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec3Swizzles = SwizzleFileEmitter.Emit(new(3, VectorCatalog.Float));

        StringAssert.Contains(vec3Swizzles, "public Vec3 YXZ");
        Assert.IsFalse(vec3.Contains("public Vec3 YXZ", StringComparison.Ordinal));
    }

    /// <summary>Vector interface source includes shape-specific floating interfaces.</summary>
    [TestMethod]
    public void VectorInterfaceEmitter_EmitsExpectedInterfaceFeatures()
    {
        var vectorInterfaces = VectorInterfaceFileEmitter.EmitAll().ToDictionary(file => file.FileName, file => file.Source);

        StringAssert.Contains(vectorInterfaces["IVec3Floating.g.cs"], "IVec3FloatingToInteger<Vec3i>");
    }

    /// <summary>Matrix source includes column-major layout, algebra, transform, relation, and quaternion helpers.</summary>
    [TestMethod]
    public void MatrixEmitter_EmitsExpectedMatrixFeatures()
    {
        var mat2 = MatrixFileEmitter.Emit(new(2, 2, VectorCatalog.Float));
        var mat3 = MatrixFileEmitter.Emit(new(3, 3, VectorCatalog.Float));
        var mat3x2 = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Float));
        var mat3x2d = MatrixFileEmitter.Emit(new(3, 2, VectorCatalog.Double));
        var mat4 = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float));
        var mat4d = MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Double));

        StringAssert.Contains(mat2, "public struct Mat2(Vec2 column0, Vec2 column1)");
        StringAssert.Contains(mat2, "IMat2<Mat2, float, Vec2, Vec2, Mat2>");
        StringAssert.Contains(mat2, "public static Mat2 CreateOuterProduct(Vec2 columnVector, Vec2 rowVector)");
        StringAssert.Contains(mat3, "IMat3QuaternionRotation<Mat3, float, Vec3, Quat, Mat4>");
        StringAssert.Contains(mat3x2, "IMat3x2Transform2D<Mat3x2, float, Vec2, Vec3, Mat2x3>");
        StringAssert.Contains(mat3x2, "IMat3x2SystemNumerics<Mat3x2>");
        StringAssert.Contains(mat3x2d, "IMat3x2Transform2D<Mat3x2d, double, Vec2d, Vec3d, Mat2x3d>");
        Assert.IsFalse(mat3x2d.Contains("IMat3x2SystemNumerics<Mat3x2d>", StringComparison.Ordinal));
        StringAssert.Contains(mat4, "IMat4Transform<Mat4, float, Vec2, Vec3, Vec4>");
        StringAssert.Contains(mat4, "public static Mat4 CreateRotation(Quat rotation)");
        StringAssert.Contains(mat4, "IMat4PlaneTransform<Mat4, float, Vec3, Vec4, Plane3>");
        StringAssert.Contains(mat4, "public static Mat4 CreateReflection(Plane3 plane)");
        StringAssert.Contains(mat4, "public static Mat4 CreatePerspectiveFieldOfView");
        StringAssert.Contains(mat4d, "IMat4QuaternionRotation<Mat4d, double, Vec3d, Vec4d, Quatd, Mat3d>");
        Assert.IsFalse(mat4d.Contains("IMat4SystemNumerics<Mat4d>", StringComparison.Ordinal));

        var nonFloatingMembers = new MemberBlock();
        MatrixTransformEmitter.Emit(new MatrixSpec(4, 4, VectorCatalog.Int), nonFloatingMembers);
        Assert.AreEqual("", nonFloatingMembers.ToString());
    }

    /// <summary>Quaternion source includes rotation helpers, interpolation, System.Numerics interop, and scalar conversions.</summary>
    [TestMethod]
    public void QuaternionEmitter_EmitsExpectedQuaternionFeatures()
    {
        var quat = QuaternionFileEmitter.Emit(new(VectorCatalog.Float));
        var quatd = QuaternionFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(quat, "/// <summary>Single-precision floating-point quaternion");
        StringAssert.Contains(quat, "public struct Quat(float x, float y, float z, float w)");
        StringAssert.Contains(quat, "IQuat<Quat, float, Vec3, Vec4, Vec4b, Mat3, Mat4>");
        StringAssert.Contains(quat, "IQuatSystemNumerics<Quat>");
        StringAssert.Contains(quat, "public static Quat CreateFromAxisAngle(Vec3 axis, float radians)");
        StringAssert.Contains(quat, "public static Quat CreateFromRotationMatrix(Mat4 matrix)");
        StringAssert.Contains(quat, "public static Quat Slerp(Quat from, Quat to, float amount)");
        StringAssert.Contains(quat, "public static implicit operator System.Numerics.Quaternion(Quat value)");
        StringAssert.Contains(quat, "public static implicit operator Quat(System.Numerics.Quaternion value)");
        StringAssert.Contains(quat, "public static implicit operator Quatd(Quat value)");
        StringAssert.Contains(quatd, "/// <summary>Double-precision floating-point quaternion");
        StringAssert.Contains(quatd, "IQuat<Quatd, double, Vec3d, Vec4d, Vec4b, Mat3d, Mat4d>");
        Assert.IsFalse(quatd.Contains("IQuatSystemNumerics<Quatd>", StringComparison.Ordinal));
    }

    /// <summary>Plane source includes point queries, transforms, System.Numerics interop, and scalar conversions.</summary>
    [TestMethod]
    public void PlaneEmitter_EmitsExpectedPlaneFeatures()
    {
        var plane = PlaneFileEmitter.Emit(new(VectorCatalog.Float));
        var planed = PlaneFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(plane, "/// <summary>Single-precision floating-point 3D plane");
        StringAssert.Contains(plane, "public struct Plane3(Vec3 normal, float offset)");
        StringAssert.Contains(plane, "IPlane3Transform<Plane3, float, Vec3, Vec4, Mat4, Quat>");
        StringAssert.Contains(plane, "IPlane3SystemNumerics<Plane3>");
        StringAssert.Contains(plane, "public static Plane3 Create(Vec3 normal, float offset)");
        StringAssert.Contains(plane, "public static Plane3 CreateFromPointNormal(Vec3 point, Vec3 normal)");
        StringAssert.Contains(plane, "public readonly Vec3 ProjectPoint(Vec3 point)");
        StringAssert.Contains(plane, "public static bool TryTransform(Plane3 plane, Mat4 matrix, out Plane3 result)");
        StringAssert.Contains(plane, "public static implicit operator System.Numerics.Plane(Plane3 value)");
        StringAssert.Contains(plane, "public static implicit operator Plane3(System.Numerics.Plane value)");
        StringAssert.Contains(plane, "Unsafe.BitCast<Plane3, System.Numerics.Plane>(value)");
        StringAssert.Contains(plane, "Unsafe.BitCast<System.Numerics.Plane, Plane3>(value)");
        StringAssert.Contains(plane, "public static implicit operator Plane3d(Plane3 value)");
        StringAssert.Contains(planed, "/// <summary>Double-precision floating-point 3D plane");
        StringAssert.Contains(planed, "IPlane3Transform<Plane3d, double, Vec3d, Vec4d, Mat4d, Quatd>");
        Assert.IsFalse(planed.Contains("IPlane3SystemNumerics<Plane3d>", StringComparison.Ordinal));
        Assert.IsFalse(plane.Contains("public Plane3(float", StringComparison.Ordinal));
        Assert.IsFalse(plane.Contains("public static Plane3 Create(float", StringComparison.Ordinal));
    }

    /// <summary>Frustum source includes clip extraction, box queries, finite corners, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void FrustumEmitter_EmitsExpectedFrustumFeatures()
    {
        var frustum = FrustumFileEmitter.Emit(new(VectorCatalog.Float));
        var frustumd = FrustumFileEmitter.Emit(new(VectorCatalog.Double));

        StringAssert.Contains(frustum, "/// <summary>Single-precision floating-point 3D frustum volume.");
        StringAssert.Contains(frustum, "public struct Frustum3(");
        StringAssert.Contains(frustum, "IFrustum3Transform<Frustum3, float, Vec3, Vec4, Mat4, Plane3, Box3>");
        StringAssert.Contains(frustum, "public static Frustum3 CreateFromPlanes(ReadOnlySpan<Plane3> planes)");
        StringAssert.Contains(frustum, "public static Frustum3 CreateFromClipTransform(Mat4 clipFromSource)");
        StringAssert.Contains(frustum, "ProjectionDepthRange.NegativeOneToOne");
        StringAssert.Contains(frustum, "Left, Right, Bottom, Top, Near, Far order");
        StringAssert.Contains(frustum, "Near bottom-left, Near bottom-right, Near top-left, Near top-right");
        StringAssert.Contains(frustum, "conservative culling test");
        StringAssert.Contains(frustum, "public readonly bool Contains(Vec3 point)");
        StringAssert.Contains(frustum, "public readonly ContainmentKind Classify(Box3 box)");
        StringAssert.Contains(frustum, "public readonly bool TryCopyCornersTo(Span<Vec3> destination)");
        StringAssert.Contains(frustum, "public readonly bool TryCopyNormalizedPlanesTo(Span<Plane3> destination)");
        StringAssert.Contains(frustum, "public readonly bool TryCreateBoundingBox(out Box3 box)");
        StringAssert.Contains(frustum, "public readonly ContainmentKind ClassifyPrecise(Box3 box)");
        StringAssert.Contains(frustum, "public readonly bool TryClassify(Frustum3 other, out ContainmentKind result)");
        StringAssert.Contains(frustum, "public static implicit operator Frustum3d(Frustum3 value)");
        StringAssert.Contains(frustumd, "/// <summary>Double-precision floating-point 3D frustum volume.");
        StringAssert.Contains(frustumd, "IFrustum3Transform<Frustum3d, double, Vec3d, Vec4d, Mat4d, Plane3d, Box3d>");
        StringAssert.Contains(frustumd, "public static Frustum3d CreateFromPlanes(ReadOnlySpan<Plane3d> planes)");
        Assert.IsFalse(frustum.Contains("public Frustum3(float", StringComparison.Ordinal));
        Assert.IsFalse(frustum.Contains("public static Frustum3 Create(float", StringComparison.Ordinal));
    }

    /// <summary>Box source includes 2D and 3D spatial helpers, formatting, parsing, and scalar conversions.</summary>
    [TestMethod]
    public void BoxEmitter_EmitsExpectedBoxFeatures()
    {
        var box2 = BoxFileEmitter.Emit(new(2, VectorCatalog.Float));
        var box2d = BoxFileEmitter.Emit(new(2, VectorCatalog.Double));
        var box2i = BoxFileEmitter.Emit(new(2, VectorCatalog.Int));
        var box3 = BoxFileEmitter.Emit(new(3, VectorCatalog.Float));
        var box3d = BoxFileEmitter.Emit(new(3, VectorCatalog.Double));
        var box3i = BoxFileEmitter.Emit(new(3, VectorCatalog.Int));

        StringAssert.Contains(box2, "public partial struct Box2(Vec2 min, Vec2 max)");
        StringAssert.Contains(box2, "IBox2<Box2, float, Vec2>");
        StringAssert.Contains(box2, "public static Box2 CreateFromCorners(Vec2 first, Vec2 second)");
        StringAssert.Contains(box2, "public readonly bool ContainsInclusive(Vec2 point)");
        StringAssert.Contains(box2, "public readonly bool TryFormat(");
        StringAssert.Contains(box2, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Box2 result)");
        StringAssert.Contains(box2, "public static implicit operator Box2d(Box2 value)");
        StringAssert.Contains(box2d, "public static explicit operator Box2(Box2d value)");
        StringAssert.Contains(box2i, "public static implicit operator Box2(Box2i value)");
        StringAssert.Contains(box3, "IBox3Sphere<Box3, float, Vec3, Sphere3>");
        StringAssert.Contains(box3, "public readonly bool Contains(Sphere3 sphere)");
        StringAssert.Contains(box3, "public static bool Intersects(Box3 box, Sphere3 sphere)");
        StringAssert.Contains(box3d, "IBox3Sphere<Box3d, double, Vec3d, Sphere3d>");
        StringAssert.Contains(box3i, "public readonly int Volume => Width * Height * Depth;");
        StringAssert.Contains(box3i, "public readonly float DistanceTo(Vec3i point)");
        Assert.IsFalse(box2.Contains("public Box2(float minX, float minY, float maxX, float maxY)", StringComparison.Ordinal));
        Assert.IsFalse(box3i.Contains("Sphere3i", StringComparison.Ordinal));
        Assert.IsFalse(box3i.Contains("Box3d operator Box3i", StringComparison.Ordinal));
    }

    /// <summary>Generated XML documentation summaries use sentence-case descriptions without broad marketing phrases.</summary>
    [TestMethod]
    public void GeneratedDocumentation_UsesClearDescriptionWording()
    {
        foreach (var (fileName, source) in DocumentationSampleSources())
        {
            foreach (var line in source.Split(Environment.NewLine))
            {
                var description = XmlDescription(line, "<summary>")
                    ?? XmlDescription(line, "<remarks>")
                    ?? XmlDescription(line, "<returns>");
                if (description is null || description.Length == 0)
                    continue;

                Assert.IsFalse(char.IsAsciiLetterLower(description[0]), $"{fileName}: {line.Trim()}");
                AssertDescriptionDoesNotContain(description, "game engine", fileName, line);
                AssertDescriptionDoesNotContain(description, "game math", fileName, line);
                AssertDescriptionDoesNotContain(description, "graphics APIs", fileName, line);
            }
        }
    }

    private static IEnumerable<(string FileName, string Source)> DocumentationSampleSources()
    {
        foreach (var (fileName, source) in VectorInterfaceFileEmitter.EmitAll())
            yield return (fileName, source);

        yield return ("Vec2.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Float)));
        yield return ("Vec2h.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Half)));
        yield return ("Vec2i64.g.cs", VectorFileEmitter.Emit(new(2, VectorCatalog.Int64)));
        yield return ("Vec4.g.cs", VectorFileEmitter.Emit(new(4, VectorCatalog.Float)));
        yield return ("Vec4.Swizzles.g.cs", SwizzleFileEmitter.Emit(new(4, VectorCatalog.Float)));
        yield return ("Mat4.g.cs", MatrixFileEmitter.Emit(new(4, 4, VectorCatalog.Float)));
        yield return ("Quat.g.cs", QuaternionFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Quatd.g.cs", QuaternionFileEmitter.Emit(new(VectorCatalog.Double)));
        yield return ("Plane3.g.cs", PlaneFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Plane3d.g.cs", PlaneFileEmitter.Emit(new(VectorCatalog.Double)));
        yield return ("Frustum3.g.cs", FrustumFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Frustum3d.g.cs", FrustumFileEmitter.Emit(new(VectorCatalog.Double)));
        yield return ("Intervalf.g.cs", IntervalFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Ray3.g.cs", RayFileEmitter.Emit(new(VectorCatalog.Float)));
        yield return ("Box3i.g.cs", BoxFileEmitter.Emit(new(3, VectorCatalog.Int)));
    }

    private static void AssertSystemMinMaxClamp(string source, string vectorType, string systemType)
    {
        StringAssert.Contains(source, $"public static {vectorType} Min({vectorType} left, {vectorType} right) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Min(left.packed, right.packed));");
        StringAssert.Contains(source, $"public static {vectorType} Max({vectorType} left, {vectorType} right) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Max(left.packed, right.packed));");
        StringAssert.Contains(source,
            $"public static {vectorType} Clamp({vectorType} value, {vectorType} min, {vectorType} max) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Clamp(value.packed, min.packed, max.packed));");
        StringAssert.Contains(source,
            $"public static {vectorType} Clamp({vectorType} value, float min, float max) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Clamp(value.packed, new System.Numerics.{systemType}(min), " +
            $"new System.Numerics.{systemType}(max)));");
    }

    private static void AssertFloatInterpolation(string source, string vectorType)
    {
        StringAssert.Contains(source,
            $"public static {vectorType} Lerp({vectorType} from, {vectorType} to, float amount) =>{Environment.NewLine}" +
            "        from + ((to - from) * amount);");
        StringAssert.Contains(source,
            $"public static {vectorType} Lerp({vectorType} from, {vectorType} to, {vectorType} amount) =>{Environment.NewLine}" +
            "        from + ((to - from) * amount);");
        StringAssert.Contains(source,
            $"public static {vectorType} Barycentric({vectorType} a, {vectorType} b, {vectorType} c, float u, float v) =>{Environment.NewLine}" +
            "        a + ((b - a) * u) + ((c - a) * v);");
        Assert.IsFalse(source.Contains("System.Numerics.Vector2.Lerp", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("System.Numerics.Vector3.Lerp", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("System.Numerics.Vector4.Lerp", StringComparison.Ordinal));
    }

    private static void AssertFusedMultiplyAdd(string source, string vectorType, string systemType) =>
        StringAssert.Contains(source,
            $"public static {vectorType} FusedMultiplyAdd({vectorType} left, {vectorType} right, {vectorType} addend) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.FusedMultiplyAdd(left.packed, right.packed, addend.packed));");

    private static void AssertSquareRoots(string source, string vectorType, string systemType)
    {
        StringAssert.Contains(source, $"public static {vectorType} Sqrt({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.SquareRoot(value.packed));");
        StringAssert.Contains(source, $"public static {vectorType} InverseSqrt({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.One / System.Numerics.{systemType}.SquareRoot(value.packed));");
    }

    private static void AssertRounding(string source, string vectorType, string systemType)
    {
        StringAssert.Contains(source, $"public static {vectorType} Floor({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Round(value.packed, MidpointRounding.ToNegativeInfinity));");
        StringAssert.Contains(source, $"public static {vectorType} Ceiling({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Round(value.packed, MidpointRounding.ToPositiveInfinity));");
        StringAssert.Contains(source, $"public static {vectorType} Round({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Round(value.packed));");
        StringAssert.Contains(source,
            $"public static {vectorType} Round({vectorType} value, MidpointRounding mode) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Round(value.packed, mode));");
        StringAssert.Contains(source, $"public static {vectorType} Truncate({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Truncate(value.packed));");
    }

    private static string? XmlDescription(string line, string tag)
    {
        var prefix = $"/// {tag}";
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
            return null;

        return trimmed[prefix.Length..].TrimStart();
    }

    private static void AssertDescriptionDoesNotContain(string description, string phrase, string fileName, string line) =>
        Assert.IsFalse(description.Contains(phrase, StringComparison.OrdinalIgnoreCase), $"{fileName}: {line.Trim()}");
}
