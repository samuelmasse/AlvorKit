namespace AlvorKit.Script.MathsGen;

/// <summary>Emits box interface source files for the primitives package.</summary>
internal static class BoxInterfaceFileEmitter
{
    /// <summary>Returns source files for box interfaces.</summary>
    public static IReadOnlyList<(string FileName, string Source)> EmitAll() =>
    [
        ("IBox.g.cs", MathsTemplate.Render("box-interface.cs.tmpl")),
        ("IBox2.g.cs", MathsTemplate.Render("box2-interface.cs.tmpl")),
        ("IBox3.g.cs", MathsTemplate.Render("box3-interface.cs.tmpl")),
    ];
}
