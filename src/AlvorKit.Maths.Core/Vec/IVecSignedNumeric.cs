namespace AlvorKit.Maths;

/// <summary>Applies to all signed numeric vector types, including <c>Vec2</c>, <c>Vec3i</c>, and <c>Vec4d</c>.</summary>
/// <typeparam name="TSelf">The signed numeric vector type, such as <c>Vec3</c> or <c>Vec4i</c>.</typeparam>
/// <typeparam name="TScalar">The signed component type, such as <see cref="float" />, <see cref="int" />, or <see cref="Half" />.</typeparam>
/// <typeparam name="TMask">The Boolean mask type returned by comparisons, such as <c>Vec3b</c>.</typeparam>
/// <typeparam name="TLength">The length and distance type, such as <see cref="float" /> for <c>Vec3i</c>.</typeparam>
/// <typeparam name="TArithmetic">The vector type returned by component-wise arithmetic.</typeparam>
public interface IVecSignedNumeric<TSelf, TScalar, TMask, TLength, TArithmetic> :
    IVecNumeric<TSelf, TScalar, TMask, TLength, TArithmetic>,
    IUnaryNegationOperators<TSelf, TArithmetic>
    where TSelf : struct, IVecSignedNumeric<TSelf, TScalar, TMask, TLength, TArithmetic>
    where TMask : struct, IVecMask<TMask>
{
    /// <summary>Returns the absolute value of each component.</summary>
    static abstract TSelf Abs(TSelf value);
}
