namespace AlvorKit.Script.Bindgen;

/// <summary>Normalized form of a registry len attribute such as n, count*3, 4, or COMPSIZE(pname).</summary>
/// <param name="Kind">Recognized len expression shape.</param>
/// <param name="ParamIndex">Referenced parameter index, or -1 when none.</param>
/// <param name="Divisor">Literal size or multiplier associated with the len expression.</param>
/// <param name="CompSizeArgs">Arguments extracted from a COMPSIZE expression.</param>
internal readonly record struct GlExtensionLenInfo(
    GlExtensionLenKind Kind,
    int ParamIndex,
    int Divisor,
    string[] CompSizeArgs)
{
    /// <summary>Empty len metadata used for parameters without a len attribute.</summary>
    public static readonly GlExtensionLenInfo None = new(GlExtensionLenKind.None, -1, 1, []);
}
