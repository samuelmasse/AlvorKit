namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated matrix type.</summary>
internal sealed record MatrixSpec(int Columns, int Rows, ScalarSpec Scalar)
{
    /// <summary>Gets the generated matrix type name.</summary>
    public string TypeName => Scalar.MatrixName(Columns, Rows);

    /// <summary>Gets the vector type used for each column.</summary>
    public string ColumnTypeName => Scalar.VectorName(Rows);

    /// <summary>Gets the vector type used for each row.</summary>
    public string RowTypeName => Scalar.VectorName(Columns);

    /// <summary>Gets the matching transpose matrix type name.</summary>
    public string TransposeTypeName => Scalar.MatrixName(Rows, Columns);

    /// <summary>Gets whether this matrix is square.</summary>
    public bool IsSquare => Columns == Rows;

    /// <summary>Gets the number of scalar components in this matrix.</summary>
    public int ComponentCount => Columns * Rows;

    /// <summary>Gets the generated column field names.</summary>
    public IReadOnlyList<string> ColumnNames => Enumerable.Range(0, Columns).Select(index => $"Column{index}").ToArray();

    /// <summary>Gets the generated primary constructor parameter names.</summary>
    public IReadOnlyList<string> ColumnParameters => Enumerable.Range(0, Columns).Select(index => $"column{index}").ToArray();

    /// <summary>Gets the component name used by a column vector for the given row.</summary>
    public static string RowComponent(int row) => VectorCatalog.Components[row];

    /// <summary>Gets the component name used by a row vector for the given column.</summary>
    public static string ColumnComponent(int column) => VectorCatalog.Components[column];
}
