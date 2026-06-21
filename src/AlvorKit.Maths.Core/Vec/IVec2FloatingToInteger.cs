namespace AlvorKit.Maths;

/// <summary>Applies to two-component floating-point vector types with integer rounding helpers, including <c>Vec2</c> and <c>Vec2d</c>.</summary>
/// <typeparam name="TInteger">The matching two-component integer vector type, such as <c>Vec2i</c>.</typeparam>
public interface IVec2FloatingToInteger<TInteger>
    where TInteger : struct, IVec2<TInteger, int>
{
    /// <summary>Returns this vector truncated toward zero to integer components.</summary>
    TInteger TruncateToVec2i();

    /// <summary>Returns this vector rounded downward to integer components.</summary>
    TInteger FloorToVec2i();

    /// <summary>Returns this vector rounded upward to integer components.</summary>
    TInteger CeilingToVec2i();

    /// <summary>Returns this vector rounded to the nearest integer components.</summary>
    TInteger RoundToVec2i();
}
