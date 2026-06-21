namespace AlvorKit.Maths;

/// <summary>
/// Applies to all four-component numeric vector types with axis constants,
/// including <c>Vec4</c>, <c>Vec4i</c>, and <c>Vec4u</c>.
/// </summary>
/// <typeparam name="TSelf">The four-component numeric vector type, such as <c>Vec4</c> or <c>Vec4i</c>.</typeparam>
public interface IVec4Axes<TSelf> : IVec3Axes<TSelf>
    where TSelf : struct, IVec4Axes<TSelf>
{
    /// <summary>Gets the unit vector pointing along the positive W axis.</summary>
    static abstract TSelf UnitW { get; }
}
