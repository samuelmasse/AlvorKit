namespace AlvorKit.Maths;

/// <summary>
/// Applies to all two-component numeric vector types with axis constants,
/// including <c>Vec2</c>, <c>Vec2i</c>, and <c>Vec2u</c>.
/// </summary>
/// <typeparam name="TSelf">The two-component numeric vector type, such as <c>Vec2</c> or <c>Vec2i</c>.</typeparam>
public interface IVec2Axes<TSelf>
    where TSelf : struct, IVec2Axes<TSelf>
{
    /// <summary>Gets the unit vector pointing along the positive X axis.</summary>
    static abstract TSelf UnitX { get; }

    /// <summary>Gets the unit vector pointing along the positive Y axis.</summary>
    static abstract TSelf UnitY { get; }
}
