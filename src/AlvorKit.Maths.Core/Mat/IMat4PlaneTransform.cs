namespace AlvorKit.Maths;

/// <summary>Applies to 4x4 matrix types that create transforms from matching 3D planes.</summary>
/// <typeparam name="TSelf">The 4x4 matrix type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
public interface IMat4PlaneTransform<TSelf, TScalar, TVector3, TVector4, TPlane3> :
    IMat4<TSelf, TScalar, TVector4, TVector4, TSelf>
    where TSelf : struct, IMat4PlaneTransform<TSelf, TScalar, TVector3, TVector4, TPlane3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
{
    /// <summary>Creates a matrix that reflects points across <paramref name="plane" />.</summary>
    static abstract TSelf CreateReflection(TPlane3 plane);

    /// <summary>Creates a directional shadow projection matrix onto <paramref name="plane" />.</summary>
    static abstract TSelf CreateShadow(TVector3 lightDirection, TPlane3 plane);
}
