namespace AlvorKit.Maths;

/// <summary>
/// Applies to integer vector types with vector-scalar integer operators, including <c>Vec2i</c>, <c>Vec3u</c>, and
/// <c>Vec4u64</c>.
/// </summary>
/// <typeparam name="TSelf">The integer vector type, such as <c>Vec3i</c> or <c>Vec4u</c>.</typeparam>
/// <typeparam name="TScalar">The integer component type, such as <see cref="int" />, <see cref="uint" />, or <see cref="ulong" />.</typeparam>
/// <typeparam name="TResult">The vector type returned by vector-scalar integer operators.</typeparam>
public interface IVecScalarIntegerOperators<TSelf, TScalar, TResult> :
    IBitwiseOperators<TSelf, TScalar, TResult>
    where TSelf : struct, IVecScalarIntegerOperators<TSelf, TScalar, TResult>
{
    /// <summary>Returns scalar bitwise AND against every vector component.</summary>
    static abstract TResult operator &(TScalar left, TSelf right);

    /// <summary>Returns scalar bitwise OR against every vector component.</summary>
    static abstract TResult operator |(TScalar left, TSelf right);

    /// <summary>Returns scalar exclusive-OR against every vector component.</summary>
    static abstract TResult operator ^(TScalar left, TSelf right);
}
