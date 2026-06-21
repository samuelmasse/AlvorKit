namespace AlvorKit.Maths;

/// <summary>
/// Applies to matrix types with matrix-scalar arithmetic operators, including <c>Mat2</c> and <c>Mat4d</c>.
/// </summary>
/// <typeparam name="TSelf">The matrix type, such as <c>Mat3</c> or <c>Mat4d</c>.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
public interface IMatScalarArithmeticOperators<TSelf, TScalar> :
    IAdditionOperators<TSelf, TScalar, TSelf>,
    ISubtractionOperators<TSelf, TScalar, TSelf>,
    IMultiplyOperators<TSelf, TScalar, TSelf>,
    IDivisionOperators<TSelf, TScalar, TSelf>,
    IModulusOperators<TSelf, TScalar, TSelf>
    where TSelf : struct, IMatScalarArithmeticOperators<TSelf, TScalar>
{
    /// <summary>Adds a matrix to a scalar.</summary>
    static abstract TSelf operator +(TScalar left, TSelf right);

    /// <summary>Subtracts a matrix from a scalar.</summary>
    static abstract TSelf operator -(TScalar left, TSelf right);

    /// <summary>Multiplies a scalar by a matrix.</summary>
    static abstract TSelf operator *(TScalar left, TSelf right);

    /// <summary>Divides a scalar by a matrix.</summary>
    static abstract TSelf operator /(TScalar left, TSelf right);

    /// <summary>Computes the remainder from dividing a scalar by a matrix.</summary>
    static abstract TSelf operator %(TScalar left, TSelf right);
}
