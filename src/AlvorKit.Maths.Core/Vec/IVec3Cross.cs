namespace AlvorKit.Maths;

/// <summary>
/// Applies to all three-component numeric vector types with cross products,
/// including <c>Vec3</c>, <c>Vec3i</c>, and <c>Vec3u64</c>.
/// </summary>
/// <typeparam name="TSelf">The three-component numeric vector type, such as <c>Vec3</c> or <c>Vec3i</c>.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" />, <see cref="int" />, or <see cref="ulong" />.</typeparam>
public interface IVec3Cross<TSelf, TScalar> : IVec3<TSelf, TScalar>
    where TSelf : struct, IVec3Cross<TSelf, TScalar>
{
    /// <summary>Returns the cross product of two vectors.</summary>
    static abstract TSelf Cross(TSelf left, TSelf right);
}
