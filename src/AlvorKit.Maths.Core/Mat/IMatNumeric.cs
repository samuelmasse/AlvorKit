namespace AlvorKit.Maths;

/// <summary>Applies to all numeric matrix types, including <c>Mat2</c>, <c>Mat3x4</c>, and <c>Mat4d</c>.</summary>
/// <typeparam name="TSelf">The matrix type, such as <c>Mat3</c> or <c>Mat4d</c>.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TColumn">The column vector type.</typeparam>
/// <typeparam name="TRow">The row vector type.</typeparam>
/// <typeparam name="TTranspose">The transposed matrix type.</typeparam>
public interface IMatNumeric<TSelf, TScalar, TColumn, TRow, TTranspose> :
    IMat<TSelf, TScalar, TColumn, TRow, TTranspose>,
    IAdditiveIdentity<TSelf, TSelf>,
    IUnaryPlusOperators<TSelf, TSelf>,
    IUnaryNegationOperators<TSelf, TSelf>,
    IAdditionOperators<TSelf, TSelf, TSelf>,
    ISubtractionOperators<TSelf, TSelf, TSelf>,
    IDivisionOperators<TSelf, TSelf, TSelf>,
    IModulusOperators<TSelf, TSelf, TSelf>
    where TSelf : struct, IMatNumeric<TSelf, TScalar, TColumn, TRow, TTranspose>
    where TColumn : struct, IVec<TColumn, TScalar>
    where TRow : struct, IVec<TRow, TScalar>
    where TTranspose : struct
{
}
