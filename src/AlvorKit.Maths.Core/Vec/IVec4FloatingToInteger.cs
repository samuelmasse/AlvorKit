namespace AlvorKit.Maths;

/// <summary>Applies to four-component floating-point vector types with integer rounding helpers, including <c>Vec4</c> and <c>Vec4d</c>.</summary>
/// <typeparam name="TInteger">The matching four-component integer vector type, such as <c>Vec4i</c>.</typeparam>
public interface IVec4FloatingToInteger<TInteger>
    where TInteger : struct, IVec4<TInteger, int>
{
    /// <summary>Returns this vector truncated toward zero to integer components.</summary>
    TInteger TruncateToVec4i();

    /// <summary>Returns this vector rounded downward to integer components.</summary>
    TInteger FloorToVec4i();

    /// <summary>Returns this vector rounded upward to integer components.</summary>
    TInteger CeilingToVec4i();

    /// <summary>Returns this vector rounded to the nearest integer components.</summary>
    TInteger RoundToVec4i();
}
