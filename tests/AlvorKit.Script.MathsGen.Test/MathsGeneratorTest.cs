namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated vector source planning and filesystem output.</summary>
[TestClass]
public sealed class MathsGeneratorTest
{
    /// <summary>Generation recreates the output directory and emits every configured scalar/dimension vector.</summary>
    [TestMethod]
    public void GenerateTo_RecreatesOutputAndEmitsConfiguredVectors()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");
        var project = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName);
        Directory.CreateDirectory(project);
        var staleFile = Path.Combine(project, "stale.txt");
        File.WriteAllText(staleFile, "old");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var vecDirectory = Path.Combine(project, MathsGenerator.VecDirectoryName);
        string VecFile(string fileName) => Path.Combine(vecDirectory, fileName);
        Assert.IsFalse(File.Exists(staleFile));
        Assert.IsFalse(Directory.Exists(Path.Combine(project, "Generated")));
        Assert.AreEqual(124, Directory.GetFiles(vecDirectory, "*.cs").Length);
        Assert.IsTrue(File.Exists(Path.Combine(project, "AlvorKit.Maths.Primitives.csproj")));
        Assert.IsTrue(File.Exists(Path.Combine(project, "ScalarMath.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecMask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4Mask.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecRelationalOperators.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecMetric.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecNumeric.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecSignedNumeric.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecIntegerCountShiftOperators.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecFloating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecFloatingGeometry.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecFloatingScalarFunctions.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecFloatingVectorInterpolation.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Cross.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecScalarArithmeticOperators.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVecScalarIntegerOperators.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Axes.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Axes.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4Axes.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Planar.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2FloatingToInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3FloatingToInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4FloatingToInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2SystemNumerics.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3SystemNumerics.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4SystemNumerics.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4Floating.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4SignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec2UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec3UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("IVec4UnsignedInteger.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec2.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec2.Swizzles.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec4u128.g.cs")));
        Assert.IsTrue(File.Exists(VecFile("Vec4u128.Swizzles.g.cs")));
        StringAssert.Contains(File.ReadAllText(Path.Combine(project, "AlvorKit.Maths.Primitives.csproj")), "<Version>9.8.7</Version>");
        StringAssert.Contains(File.ReadAllText(Path.Combine(project, "AlvorKit.Maths.Primitives.csproj")),
            "<Using Include=\"System.Diagnostics\" />");
    }

    /// <summary>Generated source includes tuple conversions, cross-scalar conversions, and read/write swizzles.</summary>
    [TestMethod]
    public void GenerateTo_EmitsExpectedVectorFeatures()
    {
        using var workspace = TempWorkspace.Create();
        var outputRoot = workspace.CreateDirectory("generated");

        MathsGenerator.GenerateTo(outputRoot, "9.8.7");

        var projectDirectory = Path.Combine(outputRoot, MathsGenerator.PrimitivesProjectName);
        var vecDirectory = Path.Combine(projectDirectory, MathsGenerator.VecDirectoryName);
        var scalarMath = File.ReadAllText(Path.Combine(projectDirectory, "ScalarMath.g.cs"));
        var vectorInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec.g.cs"));
        var vec3Interface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3.g.cs"));
        var numericInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecNumeric.g.cs"));
        var maskInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecMask.g.cs"));
        var vec3MaskInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3Mask.g.cs"));
        var integerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecInteger.g.cs"));
        var integerCountShiftInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecIntegerCountShiftOperators.g.cs"));
        var floatingInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecFloating.g.cs"));
        var floatingGeometryInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecFloatingGeometry.g.cs"));
        var floatingScalarInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecFloatingScalarFunctions.g.cs"));
        var floatingVectorInterpolationInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecFloatingVectorInterpolation.g.cs"));
        var crossInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3Cross.g.cs"));
        var scalarArithmeticInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecScalarArithmeticOperators.g.cs"));
        var scalarIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecScalarIntegerOperators.g.cs"));
        var vec2AxesInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec2Axes.g.cs"));
        var vec2PlanarInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec2Planar.g.cs"));
        var vec3FloatingInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3Floating.g.cs"));
        var vec3SignedIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3SignedInteger.g.cs"));
        var vec3UnsignedIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3UnsignedInteger.g.cs"));
        var vec3FloatingToIntegerInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3FloatingToInteger.g.cs"));
        var vec3SystemNumericsInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVec3SystemNumerics.g.cs"));
        var relationalInterface = File.ReadAllText(Path.Combine(vecDirectory, "IVecRelationalOperators.g.cs"));
        var vec2 = File.ReadAllText(Path.Combine(vecDirectory, "Vec2.g.cs"));
        var vec3 = File.ReadAllText(Path.Combine(vecDirectory, "Vec3.g.cs"));
        var vec3i = File.ReadAllText(Path.Combine(vecDirectory, "Vec3i.g.cs"));
        var vec3u = File.ReadAllText(Path.Combine(vecDirectory, "Vec3u.g.cs"));
        var vec3b = File.ReadAllText(Path.Combine(vecDirectory, "Vec3b.g.cs"));
        var vec4 = File.ReadAllText(Path.Combine(vecDirectory, "Vec4.g.cs"));
        var vec3Swizzles = File.ReadAllText(Path.Combine(vecDirectory, "Vec3.Swizzles.g.cs"));
        StringAssert.Contains(scalarMath, "public static class ScalarMath");
        StringAssert.Contains(scalarMath, "public static T Saturate<T>(T value)");
        StringAssert.Contains(scalarMath, "where T : IFloatingPointIeee754<T>");
        StringAssert.Contains(scalarMath, "public static int FindMostSignificantBit<T>(T value)");
        StringAssert.Contains(scalarMath, "Unsafe.SizeOf<T>()");
        StringAssert.Contains(vectorInterface, "public interface IVec<TSelf, TScalar>");
        StringAssert.Contains(vectorInterface, "IComparable<TSelf>");
        StringAssert.Contains(vectorInterface, "IEqualityOperators<TSelf, TSelf, bool>");
        StringAssert.Contains(vectorInterface, "ISpanFormattable");
        StringAssert.Contains(vectorInterface, "IUtf8SpanFormattable");
        StringAssert.Contains(vectorInterface, "ISpanParsable<TSelf>");
        StringAssert.Contains(vectorInterface, "IUtf8SpanParsable<TSelf>");
        StringAssert.Contains(vectorInterface, "Applies to all vector types");
        StringAssert.Contains(vectorInterface, "<see cref=\"Vec2\" />, <see cref=\"Vec3i\" />, <see cref=\"Vec4d\" />");
        StringAssert.Contains(vec3Interface, "static abstract implicit operator TSelf((TScalar X, TScalar Y, TScalar Z) value);");
        StringAssert.Contains(vec3Interface, "void Deconstruct(out TScalar x, out TScalar y, out TScalar z);");
        StringAssert.Contains(numericInterface, "public interface IVecNumeric<TSelf, TScalar, TMask, TLength>");
        StringAssert.Contains(numericInterface, "Applies to all non-Boolean numeric vector types");
        StringAssert.Contains(maskInterface, "public interface IVecMask<TSelf>");
        StringAssert.Contains(maskInterface, "IBitwiseOperators<TSelf, TSelf, TSelf>");
        StringAssert.Contains(maskInterface, "Applies to all Boolean mask vector types");
        StringAssert.Contains(maskInterface, "static abstract TSelf False { get; }");
        StringAssert.Contains(maskInterface, "static abstract TSelf True { get; }");
        StringAssert.Contains(maskInterface, "static abstract TSelf operator ~(TSelf value);");
        StringAssert.Contains(vec3MaskInterface, "Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse);");
        StringAssert.Contains(integerInterface, "public interface IVecInteger<TSelf, TScalar, TMask, TCount, TLength>");
        StringAssert.Contains(integerInterface, "IShiftOperators<TSelf, int, TSelf>");
        StringAssert.Contains(integerInterface, "static abstract TSelf operator <<(TSelf left, TSelf right);");
        StringAssert.Contains(integerInterface, "static abstract TSelf operator >>>(TSelf left, int right);");
        StringAssert.Contains(integerInterface, "static abstract TSelf operator >>>(TSelf left, TSelf right);");
        StringAssert.Contains(integerCountShiftInterface, "public interface IVecIntegerCountShiftOperators<TSelf, TCount>");
        StringAssert.Contains(integerCountShiftInterface, "static abstract TSelf operator >>>(TSelf left, TCount right);");
        StringAssert.Contains(floatingInterface, "public interface IVecFloating<TSelf, TScalar, TMask>");
        StringAssert.Contains(floatingGeometryInterface, "public interface IVecFloatingGeometry<TSelf, TScalar>");
        StringAssert.Contains(floatingScalarInterface, "public interface IVecFloatingScalarFunctions<TSelf, TScalar>");
        StringAssert.Contains(floatingVectorInterpolationInterface, "public interface IVecFloatingVectorInterpolation<TSelf>");
        StringAssert.Contains(crossInterface, "public interface IVec3Cross<TSelf, TScalar>");
        StringAssert.Contains(scalarArithmeticInterface, "public interface IVecScalarArithmeticOperators<TSelf, TScalar>");
        StringAssert.Contains(scalarArithmeticInterface, "static abstract TSelf Clamp(TSelf value, TScalar min, TScalar max);");
        StringAssert.Contains(scalarIntegerInterface, "public interface IVecScalarIntegerOperators<TSelf, TScalar>");
        StringAssert.Contains(vec2AxesInterface, "public interface IVec2Axes<TSelf>");
        StringAssert.Contains(vec2PlanarInterface, "public interface IVec2Planar<TSelf, TScalar>");
        StringAssert.Contains(vec3FloatingInterface, "public interface IVec3Floating<TSelf, TScalar, TMask>");
        StringAssert.Contains(vec3FloatingInterface, "IVecFloatingScalarFunctions<TSelf, TScalar>");
        StringAssert.Contains(vec3FloatingInterface, "IVec3FloatingToInteger<Vec3i>");
        StringAssert.Contains(vec3SignedIntegerInterface, "public interface IVec3SignedInteger<TSelf, TScalar, TMask, TCount, TLength>");
        StringAssert.Contains(vec3UnsignedIntegerInterface, "public interface IVec3UnsignedInteger<TSelf, TScalar, TMask, TCount, TLength>");
        StringAssert.Contains(vec3UnsignedIntegerInterface, "IVecIntegerCountShiftOperators<TSelf, TCount>");
        StringAssert.Contains(vec3FloatingToIntegerInterface, "public interface IVec3FloatingToInteger<TInteger>");
        StringAssert.Contains(vec3SystemNumericsInterface, "public interface IVec3SystemNumerics<TSelf>");
        StringAssert.Contains(relationalInterface, "public interface IVecRelationalOperators<TSelf, TMask>");
        StringAssert.Contains(relationalInterface, "Applies to all numeric vector types with comparison operators");
        StringAssert.Contains(vec2, "IVec2Floating<Vec2, float, Vec2b>");
        StringAssert.Contains(vec2, "IVecScalarArithmeticOperators<Vec2, float>");
        StringAssert.Contains(vec2, "IVec2SystemNumerics<Vec2>");
        StringAssert.Contains(vec3, "IVec3Floating<Vec3, float, Vec3b>");
        StringAssert.Contains(vec3, "[DebuggerDisplay(\"{ToString(),nq}\")]");
        StringAssert.Contains(vec3, "IVecScalarArithmeticOperators<Vec3, float>");
        StringAssert.Contains(vec3i, "IVec3SignedInteger<Vec3i, int, Vec3b, Vec3i, float>");
        StringAssert.Contains(vec3i, "IVecScalarIntegerOperators<Vec3i, int>");
        StringAssert.Contains(vec3u, "IVec3UnsignedInteger<Vec3u, uint, Vec3b, Vec3i, float>");
        StringAssert.Contains(vec3b, "IVec3Mask<Vec3b>");
        StringAssert.Contains(vec3, "public static Vec3 Create(float value)");
        StringAssert.Contains(vec3, "public static Vec3 Create(float x, float y, float z)");
        StringAssert.Contains(vec3, "public readonly void CopyTo(Span<float> destination)");
        StringAssert.Contains(vec3, "public readonly bool TryFormat(");
        StringAssert.Contains(vec3, "public readonly int CompareTo(Vec3 other)");
        StringAssert.Contains(vec3, "public static Vec3 Parse(ReadOnlySpan<char> s, IFormatProvider? formatProvider)");
        StringAssert.Contains(vec3, "public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? formatProvider, out Vec3 result)");
        StringAssert.Contains(vec3, "var xComparison = X.CompareTo(other.X);");
        StringAssert.Contains(vec3, "var zComparison = Z.CompareTo(other.Z);");
        StringAssert.Contains(vec3, "if (!TryAppendUtf8(\"(\"u8, ref remainder, ref bytesWritten))");
        StringAssert.Contains(vec3, "return float.TryParse(source, formatProvider, out value);");
        StringAssert.Contains(vec3, "TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider)");
        StringAssert.Contains(vec3, "if (!TryAppend(\"(\".AsSpan(), ref remainder, ref charsWritten))");
        StringAssert.Contains(vec3, "if (!TryAppendComponent(X, ref remainder, ref charsWritten, format, formatProvider))");
        StringAssert.Contains(vec3, "public static implicit operator Vec3((float X, float Y, float Z) value)");
        StringAssert.Contains(vec3, "public static implicit operator Vec3(System.Numerics.Vector3 value)");
        StringAssert.Contains(vec3, "public static implicit operator System.Numerics.Vector3(Vec3 value)");
        StringAssert.Contains(vec3, "public static implicit operator Vec3(Vec3i value)");
        StringAssert.Contains(vec3, "public Vec3(Vec2 xy, float z)");
        StringAssert.Contains(vec4, "public Vec4(Vec2 xy, Vec2 zw)");
        StringAssert.Contains(vec4, "public static explicit operator Vec4(Vec4d value)");
        StringAssert.Contains(vec3, "public static Vec3b operator <(Vec3 left, Vec3 right)");
        StringAssert.Contains(vec3, "public static Vec3 Sqrt(Vec3 value)");
        StringAssert.Contains(vec3, "ScalarMath.Sqrt(value.X)");
        StringAssert.Contains(vec3, "ScalarMath.SmoothStep(edge0.X, edge1.X, value.X)");
        StringAssert.Contains(vec3, "ScalarMath.Modulo(left.X, right.X)");
        StringAssert.Contains(vec3, "ScalarMath.IsFinite(value.X)");
        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, int right)");
        StringAssert.Contains(vec3i, "public static Vec3i operator >>>(Vec3i left, Vec3i right)");
        StringAssert.Contains(vec3i, "ScalarMath.BitCount(value.X)");
        Assert.IsFalse(vec3i.Contains("BitCountScalar", StringComparison.Ordinal));
        StringAssert.Contains(vec3u, "public static Vec3u operator >>>(Vec3u left, Vec3i right)");
        StringAssert.Contains(vec3b, "IVec3Mask<Vec3b>");
        StringAssert.Contains(vec3b, "public static Vec3b operator ~(Vec3b value)");
        StringAssert.Contains(vec3b, "var text = value ? \"True\" : \"False\";");
        StringAssert.Contains(vec3b, "var text = value ? \"True\"u8 : \"False\"u8;");
        StringAssert.Contains(vec3b, "if (source.SequenceEqual(\"True\"u8))");
        StringAssert.Contains(vec3b, "public static bool operator true(Vec3b value)");
        StringAssert.Contains(vec3b, "public readonly Vec3u128 Select(Vec3u128 whenTrue, Vec3u128 whenFalse)");
        StringAssert.Contains(vec3b, "ScalarMath.Select(X, whenTrue.X, whenFalse.X)");
        Assert.IsFalse(vec3.Contains("public Vec3 YXZ", StringComparison.Ordinal));
        StringAssert.Contains(vec3Swizzles, "public partial struct Vec3");
        StringAssert.Contains(vec3Swizzles, "public Vec3 YXZ");
        StringAssert.Contains(vec3Swizzles, "public readonly Vec3 XXX");
        StringAssert.Contains(vec3Swizzles, "/// <summary>Gets or sets the YXZ swizzle.</summary>");
        StringAssert.Contains(vec3Swizzles, "[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        Assert.IsFalse(vec3Swizzles.Contains("EditorBrowsable", StringComparison.Ordinal));
    }

    /// <summary>Configured type names follow the AlvorKit vector suffix convention.</summary>
    [TestMethod]
    public void VectorCatalog_UsesExpectedNames()
    {
        var names = VectorCatalog.Vectors.Select(vector => vector.TypeName).ToArray();

        CollectionAssert.Contains(names, "Vec2");
        CollectionAssert.Contains(names, "Vec3h");
        CollectionAssert.Contains(names, "Vec3i8");
        CollectionAssert.Contains(names, "Vec3u8");
        CollectionAssert.Contains(names, "Vec3i16");
        CollectionAssert.Contains(names, "Vec3u16");
        CollectionAssert.Contains(names, "Vec3i");
        CollectionAssert.Contains(names, "Vec3u");
        CollectionAssert.Contains(names, "Vec4d");
        CollectionAssert.Contains(names, "Vec4i64");
        CollectionAssert.Contains(names, "Vec4u64");
        CollectionAssert.Contains(names, "Vec4i128");
        CollectionAssert.Contains(names, "Vec4u128");
        CollectionAssert.Contains(names, "Vec2b");
    }
}
