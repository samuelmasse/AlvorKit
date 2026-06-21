namespace AlvorKit.Maths;

/// <summary>
/// Applies to numeric vector types with vector-scalar arithmetic operators, including <c>Vec2</c>, <c>Vec3i</c>, and
/// <c>Vec4u64</c>.
/// </summary>
/// <typeparam name="TSelf">The numeric vector type, such as <c>Vec3</c> or <c>Vec4i</c>.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" />, <see cref="int" />, or <see cref="ulong" />.</typeparam>
/// <typeparam name="TResult">The vector type returned by vector-scalar arithmetic.</typeparam>
public interface IVecScalarArithmeticOperators<TSelf, TScalar, TResult> :
    IAdditionOperators<TSelf, TScalar, TResult>,
    ISubtractionOperators<TSelf, TScalar, TResult>,
    IMultiplyOperators<TSelf, TScalar, TResult>,
    IDivisionOperators<TSelf, TScalar, TResult>,
    IModulusOperators<TSelf, TScalar, TResult>
    where TSelf : struct, IVecScalarArithmeticOperators<TSelf, TScalar, TResult>
{
    /// <summary>Adds a vector to a scalar.</summary>
    static abstract TResult operator +(TScalar left, TSelf right);

    /// <summary>Subtracts a vector from a scalar.</summary>
    static abstract TResult operator -(TScalar left, TSelf right);

    /// <summary>Multiplies a scalar by a vector.</summary>
    static abstract TResult operator *(TScalar left, TSelf right);

    /// <summary>Divides a scalar by a vector.</summary>
    static abstract TResult operator /(TScalar left, TSelf right);

    /// <summary>Computes the remainder from dividing a scalar by a vector.</summary>
    static abstract TResult operator %(TScalar left, TSelf right);

    /// <summary>Constrains each component between scalar minimum and maximum values.</summary>
    static abstract TSelf Clamp(TSelf value, TScalar min, TScalar max);
}
