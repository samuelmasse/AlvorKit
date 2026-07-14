namespace AlvorKit.Script.MathsGen;

/// <summary>Builds measured semantics-preserving double-to-Int32 conversion expressions.</summary>
internal static class DoubleToInt32Expression
{
    /// <summary>Gets whether a double vector has a retained x86 conversion path.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Scalar.Kind == ScalarKind.Double && vector.Dimension is 2 or 4;

    /// <summary>Returns a call to the internal conversion kernel for one compile-time mode.</summary>
    public static string Convert(VectorSpec vector, string value, int mode) =>
        $"{vector.TypeName}.ConvertToInt32Packed({value}, {mode.ToString(CultureInfo.InvariantCulture)})";

    /// <summary>Returns the shape-specific internal conversion kernel.</summary>
    public static string Helper(VectorSpec vector) =>
        MathsTemplate.Fragment(vector.Dimension == 2
            ? "double-to-int32-vec2-helper.csfrag.tmpl"
            : "double-to-int32-vec4-helper.csfrag.tmpl");
}
