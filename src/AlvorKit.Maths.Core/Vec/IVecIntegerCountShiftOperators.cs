namespace AlvorKit.Maths;

/// <summary>
/// Applies to integer vector types with signed integer-vector shift counts, including <c>Vec2u</c>, <c>Vec3i8</c>,
/// and <c>Vec4u128</c>.
/// </summary>
/// <typeparam name="TSelf">The integer vector type being shifted, such as <c>Vec3u</c>.</typeparam>
/// <typeparam name="TCount">The signed integer-vector shift count type, such as <c>Vec3i</c>.</typeparam>
/// <typeparam name="TResult">The vector type returned by shift operators.</typeparam>
public interface IVecIntegerCountShiftOperators<TSelf, TCount, TResult>
    where TSelf : struct, IVecIntegerCountShiftOperators<TSelf, TCount, TResult>
    where TCount : struct, IVec<TCount, int>
{
    /// <summary>Shifts each component left by matching integer-vector bit counts.</summary>
    static abstract TResult operator <<(TSelf left, TCount right);

    /// <summary>Shifts each component right by matching integer-vector bit counts.</summary>
    static abstract TResult operator >>(TSelf left, TCount right);

    /// <summary>Shifts each component right without sign extension by matching integer-vector bit counts.</summary>
    static abstract TResult operator >>>(TSelf left, TCount right);
}
