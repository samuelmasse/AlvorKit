namespace AlvorKit.Script.MathsGen;

/// <summary>Emits quaternion interface source files for the primitives package.</summary>
internal static class QuaternionInterfaceFileEmitter
{
    /// <summary>Returns source files for quaternion interfaces.</summary>
    public static IReadOnlyList<(string FileName, string Source)> EmitAll() =>
    [
        ("IQuat.g.cs", MathsTemplate.Render("quat-interface.cs.tmpl")),
        ("IQuatRotation.g.cs", MathsTemplate.Render("quat-rotation-interface.cs.tmpl")),
        ("IQuatInterpolation.g.cs", MathsTemplate.Render("quat-interpolation-interface.cs.tmpl")),
        ("IQuatSystemNumerics.g.cs", MathsTemplate.Render("quat-system-numerics-interface.cs.tmpl")),
    ];
}
