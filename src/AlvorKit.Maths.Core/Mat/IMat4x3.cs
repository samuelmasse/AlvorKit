namespace AlvorKit.Maths;

/// <summary>Applies to 4x3 matrix types.</summary>
/// <typeparam name="TSelf">The concrete matrix type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TColumn">The column vector type.</typeparam>
/// <typeparam name="TRow">The row vector type.</typeparam>
/// <typeparam name="TTranspose">The transposed matrix type.</typeparam>
public interface IMat4x3<TSelf, TScalar, TColumn, TRow, TTranspose> :
    IMatNumeric<TSelf, TScalar, TColumn, TRow, TTranspose>
    where TSelf : struct, IMat4x3<TSelf, TScalar, TColumn, TRow, TTranspose>
    where TColumn : struct, IVec<TColumn, TScalar>
    where TRow : struct, IVec<TRow, TScalar>
    where TTranspose : struct
{
    /// <summary>Creates a matrix from column vectors.</summary>
    static abstract TSelf CreateColumns(TColumn column0, TColumn column1, TColumn column2, TColumn column3);

    /// <summary>Creates a matrix from row vectors.</summary>
    static abstract TSelf CreateRows(TRow row0, TRow row1, TRow row2);
}
