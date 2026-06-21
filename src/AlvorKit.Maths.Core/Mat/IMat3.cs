namespace AlvorKit.Maths;

/// <summary>Applies to 3x3 square matrix types.</summary>
/// <typeparam name="TSelf">The concrete square matrix type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TColumn">The column and row vector type.</typeparam>
/// <typeparam name="TRow">The row vector type, equal to <typeparamref name="TColumn" /> for implementers.</typeparam>
/// <typeparam name="TTranspose">The transposed matrix type, equal to <typeparamref name="TSelf" /> for implementers.</typeparam>
public interface IMat3<TSelf, TScalar, TColumn, TRow, TTranspose> :
    IMatSquare<TSelf, TScalar, TColumn>
    where TSelf : struct, IMat3<TSelf, TScalar, TColumn, TRow, TTranspose>
    where TColumn : struct, IVec<TColumn, TScalar>
    where TRow : struct, IVec<TRow, TScalar>
    where TTranspose : struct
{
    /// <summary>Creates a matrix from column vectors.</summary>
    static abstract TSelf CreateColumns(TColumn column0, TColumn column1, TColumn column2);

    /// <summary>Creates a matrix from row vectors.</summary>
    static abstract TSelf CreateRows(TRow row0, TRow row1, TRow row2);
}
