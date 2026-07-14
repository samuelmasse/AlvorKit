namespace AlvorKit.Script.MathsGen;

/// <summary>Builds measured fixed-width float-to-Int32 conversion expressions.</summary>
internal static class FloatToInt32Expression
{
    /// <summary>Gets whether a float vector has a retained packed conversion path.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Scalar.Kind == ScalarKind.Float && vector.Dimension is 3 or 4;

    /// <summary>Returns the retained conversion expression for a float vector value.</summary>
    public static string Convert(VectorSpec vector, string value) => vector.Dimension switch
    {
        3 => $"ConvertFloatToInt32({value})",
        4 => $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<int>, Vec4i>(" +
            $"System.Runtime.Intrinsics.Vector128.ConvertToInt32(" +
            $"Unsafe.BitCast<Vec4, System.Runtime.Intrinsics.Vector128<float>>({value})))",
        _ => throw new ArgumentOutOfRangeException(nameof(vector)),
    };

    /// <summary>Returns a packed rounding-then-conversion expression.</summary>
    public static string Round(VectorSpec vector, string method, string value) => vector.Dimension switch
    {
        3 => $"ConvertPackedFloatToInt32(System.Runtime.Intrinsics.Vector128.{method}(" +
            $"System.Runtime.Intrinsics.Vector128.Create({value}.X, {value}.Y, {value}.Z, 0f)))",
        4 => $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<int>, Vec4i>(" +
            $"System.Runtime.Intrinsics.Vector128.ConvertToInt32(System.Runtime.Intrinsics.Vector128.{method}(" +
            $"Unsafe.BitCast<Vec4, System.Runtime.Intrinsics.Vector128<float>>({value}))))",
        _ => throw new ArgumentOutOfRangeException(nameof(vector)),
    };

    /// <summary>Returns the private Vec3 conversion helper for the Int32 target type.</summary>
    public static string TargetHelper(VectorSpec target) =>
        MathsTemplate.Fragment("float-to-int32-vec3-helper.csfrag.tmpl",
            ("MethodName", "ConvertFloatToInt32"),
            ("InputType", VectorCatalog.Float.VectorName(target.Dimension)),
            ("PackedExpression", "System.Runtime.Intrinsics.Vector128.Create(value.X, value.Y, value.Z, 0f)"));

    /// <summary>Returns the private Vec3 packed-result helper for the float source type.</summary>
    public static string SourceHelper() =>
        MathsTemplate.Fragment("float-to-int32-vec3-helper.csfrag.tmpl",
            ("MethodName", "ConvertPackedFloatToInt32"),
            ("InputType", "System.Runtime.Intrinsics.Vector128<float>"),
            ("PackedExpression", "value"));
}
