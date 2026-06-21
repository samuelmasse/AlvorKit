namespace AlvorKit.Maths;

/// <summary>Applies to 3D plane types that can be transformed by matching matrices and quaternions.</summary>
/// <typeparam name="TSelf">The concrete plane type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component coefficient vector type.</typeparam>
/// <typeparam name="TMatrix4">The matching 4x4 matrix type.</typeparam>
/// <typeparam name="TQuaternion">The matching quaternion type.</typeparam>
public interface IPlane3Transform<TSelf, TScalar, TVector3, TVector4, TMatrix4, TQuaternion> :
    IPlane3<TSelf, TScalar, TVector3, TVector4>
    where TSelf : struct, IPlane3Transform<TSelf, TScalar, TVector3, TVector4, TMatrix4, TQuaternion>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TMatrix4 : struct
    where TQuaternion : struct
{
    /// <summary>Transforms a plane by a matrix using inverse-transpose plane semantics.</summary>
    static abstract TSelf Transform(TSelf plane, TMatrix4 matrix);

    /// <summary>Attempts to transform a plane by an invertible matrix.</summary>
    static abstract bool TryTransform(TSelf plane, TMatrix4 matrix, out TSelf result);

    /// <summary>Transforms a plane normal by a quaternion while preserving its offset.</summary>
    static abstract TSelf Transform(TSelf plane, TQuaternion rotation);
}
