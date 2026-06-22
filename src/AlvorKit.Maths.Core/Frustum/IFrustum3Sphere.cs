namespace AlvorKit.Maths;

/// <summary>Applies to 3D frustum types that can test matching 3D spheres.</summary>
/// <typeparam name="TSelf">The concrete frustum type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
public interface IFrustum3Sphere<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3> :
    IFrustum3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TSelf : struct, IFrustum3Sphere<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
{
    /// <summary>Returns whether the frustum fully contains <paramref name="sphere" />.</summary>
    bool Contains(TSphere3 sphere);

    /// <summary>Returns whether the frustum intersects <paramref name="sphere" />, counting touching surfaces as an intersection.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Classifies how the frustum relates to <paramref name="sphere" />.</summary>
    ContainmentKind Classify(TSphere3 sphere);
}
