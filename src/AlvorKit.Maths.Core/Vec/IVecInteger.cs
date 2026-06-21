namespace AlvorKit.Maths;

/// <summary>Applies to all integer vector types, including <c>Vec2i</c>, <c>Vec3u</c>, and <c>Vec4u64</c>.</summary>
/// <typeparam name="TSelf">The integer vector type, such as <c>Vec3i</c> or <c>Vec4u</c>.</typeparam>
/// <typeparam name="TScalar">The integer component type, such as <see cref="int" />, <see cref="uint" />, or <see cref="ulong" />.</typeparam>
/// <typeparam name="TMask">The Boolean mask type returned by bit checks, such as <c>Vec3b</c>.</typeparam>
/// <typeparam name="TCount">The signed integer vector type returned by bit-count methods, such as <c>Vec3i</c>.</typeparam>
/// <typeparam name="TLength">The length and distance type, such as <see cref="float" /> for <c>Vec3i</c>.</typeparam>
/// <typeparam name="TArithmetic">The vector type returned by component-wise arithmetic.</typeparam>
public interface IVecInteger<TSelf, TScalar, TMask, TCount, TLength, TArithmetic> :
    IVecNumeric<TSelf, TScalar, TMask, TLength, TArithmetic>,
    IBitwiseOperators<TSelf, TSelf, TArithmetic>,
    IShiftOperators<TSelf, int, TArithmetic>,
    IVecIntegerCountShiftOperators<TSelf, TCount, TArithmetic>
    where TSelf : struct, IVecInteger<TSelf, TScalar, TMask, TCount, TLength, TArithmetic>
    where TMask : struct, IVecMask<TMask>
    where TCount : struct, IVec<TCount, int>
{
    /// <summary>Shifts each component left by a scalar bit count.</summary>
    static abstract TArithmetic operator <<(TSelf left, int right);

    /// <summary>Shifts each component right by a scalar bit count.</summary>
    static abstract TArithmetic operator >>(TSelf left, int right);

    /// <summary>Shifts each component right without sign extension by a scalar bit count.</summary>
    static abstract TArithmetic operator >>>(TSelf left, int right);

    /// <summary>Returns the number of set bits in each component.</summary>
    static abstract TCount BitCount(TSelf value);

    /// <summary>Returns the number of leading zero bits in each component.</summary>
    static abstract TCount LeadingZeroCount(TSelf value);

    /// <summary>Returns the number of trailing zero bits in each component.</summary>
    static abstract TCount TrailingZeroCount(TSelf value);

    /// <summary>Returns the least-significant set-bit index for each component, or -1 for zero components.</summary>
    static abstract TCount FindLeastSignificantBit(TSelf value);

    /// <summary>Returns the most-significant set-bit index for each component, or -1 for zero components.</summary>
    static abstract TCount FindMostSignificantBit(TSelf value);

    /// <summary>Returns whether each component is a positive power of two.</summary>
    static abstract TMask IsPowerOfTwo(TSelf value);
}
