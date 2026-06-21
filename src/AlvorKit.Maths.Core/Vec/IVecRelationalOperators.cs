namespace AlvorKit.Maths;

/// <summary>Applies to all numeric vector types with comparison operators, including <c>Vec3</c>, <c>Vec2i</c>, and <c>Vec4u64</c>.</summary>
/// <typeparam name="TSelf">The numeric vector type being compared.</typeparam>
/// <typeparam name="TMask">The Boolean mask type returned by comparisons, such as <c>Vec3b</c>.</typeparam>
public interface IVecRelationalOperators<TSelf, TMask>
    where TSelf : struct, IVecRelationalOperators<TSelf, TMask>
    where TMask : struct
{
    /// <summary>Returns a mask containing component-wise less-than results.</summary>
    static abstract TMask operator <(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise less-than-or-equal results.</summary>
    static abstract TMask operator <=(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise greater-than results.</summary>
    static abstract TMask operator >(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise greater-than-or-equal results.</summary>
    static abstract TMask operator >=(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise equality results.</summary>
    static abstract TMask Equal(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise inequality results.</summary>
    static abstract TMask NotEqual(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise less-than results.</summary>
    static abstract TMask LessThan(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise less-than-or-equal results.</summary>
    static abstract TMask LessThanOrEqual(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise greater-than results.</summary>
    static abstract TMask GreaterThan(TSelf left, TSelf right);

    /// <summary>Returns a mask containing component-wise greater-than-or-equal results.</summary>
    static abstract TMask GreaterThanOrEqual(TSelf left, TSelf right);
}
