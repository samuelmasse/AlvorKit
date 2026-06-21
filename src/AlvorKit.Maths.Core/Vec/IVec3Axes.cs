namespace AlvorKit.Maths;

/// <summary>
/// Applies to all three-component numeric vector types with axis constants,
/// including <c>Vec3</c>, <c>Vec3i</c>, and <c>Vec3u</c>.
/// </summary>
/// <typeparam name="TSelf">The three-component numeric vector type, such as <c>Vec3</c> or <c>Vec3i</c>.</typeparam>
public interface IVec3Axes<TSelf> : IVec2Axes<TSelf>
    where TSelf : struct, IVec3Axes<TSelf>
{
    /// <summary>Gets the unit vector pointing along the positive Z axis.</summary>
    static abstract TSelf UnitZ { get; }
}
