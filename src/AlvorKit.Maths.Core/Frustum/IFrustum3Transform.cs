namespace AlvorKit.Maths;

/// <summary>Applies to 3D frustum types that can be created from and transformed by matching matrices.</summary>
/// <typeparam name="TSelf">The concrete frustum type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TMatrix4">The matching 4x4 matrix type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
public interface IFrustum3Transform<TSelf, TScalar, TVector3, TVector4, TMatrix4, TPlane3, TBox3> :
    IFrustum3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TSelf : struct, IFrustum3Transform<TSelf, TScalar, TVector3, TVector4, TMatrix4, TPlane3, TBox3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TMatrix4 : struct
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
{
    /// <summary>Creates a frustum from a clip-space transform using default OpenGL depth from negative one to one.</summary>
    static abstract TSelf CreateFromClipTransform(TMatrix4 clipFromSource);

    /// <summary>Creates a frustum from a clip-space transform using an explicit depth range.</summary>
    static abstract TSelf CreateFromClipTransform(TMatrix4 clipFromSource, ProjectionDepthRange depthRange);

    /// <summary>Attempts to create a frustum from a clip-space transform using an explicit depth range.</summary>
    static abstract bool TryCreateFromClipTransform(TMatrix4 clipFromSource, ProjectionDepthRange depthRange, out TSelf result);

    /// <summary>Transforms a frustum by a matrix using inverse-transpose plane semantics.</summary>
    static abstract TSelf Transform(TSelf frustum, TMatrix4 matrix);

    /// <summary>Attempts to transform a frustum by an invertible matrix.</summary>
    static abstract bool TryTransform(TSelf frustum, TMatrix4 matrix, out TSelf result);
}
