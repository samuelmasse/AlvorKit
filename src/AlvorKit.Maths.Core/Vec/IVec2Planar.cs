namespace AlvorKit.Maths;

/// <summary>Applies to signed two-component numeric vector types with planar helpers, including <c>Vec2</c> and <c>Vec2i</c>.</summary>
/// <typeparam name="TSelf">The signed two-component numeric vector type, such as <c>Vec2</c> or <c>Vec2i</c>.</typeparam>
/// <typeparam name="TScalar">The signed component type, such as <see cref="float" /> or <see cref="int" />.</typeparam>
public interface IVec2Planar<TSelf, TScalar>
    where TSelf : struct, IVec2Planar<TSelf, TScalar>
{
    /// <summary>Gets this vector rotated 90 degrees counter-clockwise.</summary>
    TSelf PerpendicularLeft { get; }

    /// <summary>Gets this vector rotated 90 degrees clockwise.</summary>
    TSelf PerpendicularRight { get; }

    /// <summary>Returns the 2D scalar cross product.</summary>
    static abstract TScalar Cross(TSelf left, TSelf right);

    /// <summary>Returns the 2D perpendicular dot product.</summary>
    static abstract TScalar PerpDot(TSelf left, TSelf right);
}
