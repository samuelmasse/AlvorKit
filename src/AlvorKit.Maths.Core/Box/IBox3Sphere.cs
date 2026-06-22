namespace AlvorKit.Maths;

/// <summary>Applies to floating-point 3D axis-aligned boxes that can test matching spheres.</summary>
/// <typeparam name="TSelf">The concrete box type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TSphere">The matching 3D sphere type.</typeparam>
public interface IBox3Sphere<TSelf, TScalar, TVector3, TSphere> : IBox3<TSelf, TScalar, TVector3>
    where TSelf : struct, IBox3Sphere<TSelf, TScalar, TVector3, TSphere>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TSphere : struct, ISphere3<TSphere, TScalar, TVector3, TSelf>
{
    /// <summary>Returns whether the box fully contains <paramref name="sphere" />.</summary>
    bool Contains(TSphere sphere);

    /// <summary>Returns whether the box intersects <paramref name="sphere" />, counting touching boundaries as an intersection.</summary>
    bool Intersects(TSphere sphere);

    /// <summary>Returns whether a box intersects a sphere, counting touching boundaries as an intersection.</summary>
    static abstract bool Intersects(TSelf box, TSphere sphere);
}
