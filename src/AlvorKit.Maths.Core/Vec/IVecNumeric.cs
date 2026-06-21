namespace AlvorKit.Maths;

/// <summary>
/// Applies to all non-Boolean numeric vector types, including <c>Vec2</c>, <c>Vec3i</c>, and <c>Vec4u64</c>.
/// </summary>
/// <typeparam name="TSelf">The numeric vector type, such as <c>Vec3</c> or <c>Vec4i</c>.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" />, <see cref="int" />, or <see cref="ulong" />.</typeparam>
/// <typeparam name="TMask">The Boolean mask type returned by comparisons, such as <c>Vec3b</c>.</typeparam>
/// <typeparam name="TLength">The length and distance type, such as <see cref="float" /> for <c>Vec3i</c>.</typeparam>
/// <typeparam name="TArithmetic">The vector type returned by component-wise arithmetic.</typeparam>
public interface IVecNumeric<TSelf, TScalar, TMask, TLength, TArithmetic> :
    IVec<TSelf, TScalar>,
    IAdditiveIdentity<TSelf, TSelf>,
    IMultiplicativeIdentity<TSelf, TSelf>,
    IUnaryPlusOperators<TSelf, TArithmetic>,
    IIncrementOperators<TSelf>,
    IDecrementOperators<TSelf>,
    IAdditionOperators<TSelf, TSelf, TArithmetic>,
    ISubtractionOperators<TSelf, TSelf, TArithmetic>,
    IMultiplyOperators<TSelf, TSelf, TArithmetic>,
    IDivisionOperators<TSelf, TSelf, TArithmetic>,
    IModulusOperators<TSelf, TSelf, TArithmetic>,
    IVecRelationalOperators<TSelf, TMask>,
    IVecMetric<TSelf, TScalar, TLength>
    where TSelf : struct, IVecNumeric<TSelf, TScalar, TMask, TLength, TArithmetic>
    where TMask : struct, IVecMask<TMask>
{
    /// <summary>Gets the zero vector.</summary>
    static abstract TSelf Zero { get; }

    /// <summary>Gets a vector with every component set to one.</summary>
    static abstract TSelf One { get; }

    /// <summary>Returns the component-wise minimum of two vectors.</summary>
    static abstract TSelf Min(TSelf left, TSelf right);

    /// <summary>Returns the component-wise maximum of two vectors.</summary>
    static abstract TSelf Max(TSelf left, TSelf right);

    /// <summary>Constrains each component between matching vector minimum and maximum components.</summary>
    static abstract TSelf Clamp(TSelf value, TSelf min, TSelf max);
}
