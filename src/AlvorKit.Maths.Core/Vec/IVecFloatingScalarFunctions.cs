namespace AlvorKit.Maths;

/// <summary>
/// Applies to floating-point vector types with scalar helper overloads, including <c>Vec2</c>, <c>Vec3h</c>, and
/// <c>Vec4d</c>.
/// </summary>
/// <typeparam name="TSelf">The floating-point vector type, such as <c>Vec3</c> or <c>Vec4d</c>.</typeparam>
/// <typeparam name="TScalar">The floating-point component type, such as <see cref="float" />, <see cref="Half" />, or <see cref="double" />.</typeparam>
public interface IVecFloatingScalarFunctions<TSelf, TScalar>
    where TSelf : struct, IVecFloatingScalarFunctions<TSelf, TScalar>
{
    /// <summary>Returns floor-based modulo for each component against a scalar divisor.</summary>
    static abstract TSelf Modulo(TSelf left, TScalar right);

    /// <summary>Returns floor-based modulo for each component against a scalar divisor.</summary>
    static abstract TSelf Mod(TSelf left, TScalar right);

    /// <summary>Returns zero where value is below scalar edge and one otherwise.</summary>
    static abstract TSelf Step(TScalar edge, TSelf value);

    /// <summary>Smoothly interpolates from zero to one between scalar edge values.</summary>
    static abstract TSelf SmoothStep(TScalar edge0, TScalar edge1, TSelf value);

    /// <summary>Returns component-wise powers raised to a scalar exponent.</summary>
    static abstract TSelf Pow(TSelf value, TScalar exponent);
}
