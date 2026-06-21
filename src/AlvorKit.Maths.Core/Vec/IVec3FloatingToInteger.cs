namespace AlvorKit.Maths;

/// <summary>Applies to three-component floating-point vector types with integer rounding helpers, including <c>Vec3</c> and <c>Vec3d</c>.</summary>
/// <typeparam name="TInteger">The matching three-component integer vector type, such as <c>Vec3i</c>.</typeparam>
public interface IVec3FloatingToInteger<TInteger>
    where TInteger : struct, IVec3<TInteger, int>
{
    /// <summary>Returns this vector truncated toward zero to integer components.</summary>
    TInteger TruncateToVec3i();

    /// <summary>Returns this vector rounded downward to integer components.</summary>
    TInteger FloorToVec3i();

    /// <summary>Returns this vector rounded upward to integer components.</summary>
    TInteger CeilingToVec3i();

    /// <summary>Returns this vector rounded to the nearest integer components.</summary>
    TInteger RoundToVec3i();
}
