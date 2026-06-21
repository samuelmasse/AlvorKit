namespace AlvorKit.Script.MathsGen;

/// <summary>Emits matrix interface source files for the primitives package.</summary>
internal static class MatrixInterfaceFileEmitter
{
    /// <summary>Returns source files for matrix interfaces.</summary>
    public static IReadOnlyList<(string FileName, string Source)> EmitAll()
    {
        var files = new List<(string FileName, string Source)>
        {
            ("ProjectionHandedness.g.cs", MathsTemplate.Render("projection-handedness.cs.tmpl")),
            ("ProjectionDepthRange.g.cs", MathsTemplate.Render("projection-depth-range.cs.tmpl")),
            ("IMat.g.cs", MathsTemplate.Render("matrix-interface.cs.tmpl")),
            ("IMatNumeric.g.cs", MathsTemplate.Render("matrix-numeric-interface.cs.tmpl")),
            ("IMatScalarArithmeticOperators.g.cs", MathsTemplate.Render("matrix-scalar-arithmetic-operators-interface.cs.tmpl")),
            ("IMatRelationalOperators.g.cs", MathsTemplate.Render("matrix-relational-operators-interface.cs.tmpl")),
            ("IMatQuery.g.cs", MathsTemplate.Render("matrix-query-interface.cs.tmpl")),
            ("IMatSquare.g.cs", MathsTemplate.Render("matrix-square-interface.cs.tmpl")),
            ("IMat3Transform2D.g.cs", MathsTemplate.Render("matrix3-transform2d-interface.cs.tmpl")),
            ("IMat3x2Transform2D.g.cs", MathsTemplate.Render("matrix3x2-transform2d-interface.cs.tmpl")),
            ("IMat3x2SystemNumerics.g.cs", MathsTemplate.Render("matrix3x2-system-numerics-interface.cs.tmpl")),
            ("IMat4Transform.g.cs", MathsTemplate.Render("matrix4-transform-interface.cs.tmpl")),
            ("IMat4SystemNumerics.g.cs", MathsTemplate.Render("matrix4-system-numerics-interface.cs.tmpl")),
        };

        foreach (var (columns, rows) in MatrixCatalog.Shapes)
            files.Add(ShapeInterface(columns, rows));

        return files;
    }

    private static (string FileName, string Source) ShapeInterface(int columns, int rows)
    {
        var shape = columns == rows ? columns.ToString(CultureInfo.InvariantCulture) :
            $"{columns.ToString(CultureInfo.InvariantCulture)}x{rows.ToString(CultureInfo.InvariantCulture)}";
        var template = columns == rows ? "matrix-square-shape-interface.cs.tmpl" : "matrix-shape-interface.cs.tmpl";

        return ($"IMat{shape}.g.cs", MathsTemplate.Render(template,
            ("Shape", shape),
            ("ColumnParameters", Parameters("TColumn", "column", columns)),
            ("RowParameters", Parameters("TRow", "row", rows))));
    }

    private static string Parameters(string type, string name, int count)
    {
        var parameters = Enumerable.Range(0, count).Select(index =>
            $"{type} {name}{index.ToString(CultureInfo.InvariantCulture)}");
        return string.Join(", ", parameters);
    }
}
