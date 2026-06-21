namespace AlvorKit.Maths;

/// <summary>Applies to the single-precision 3D plane type with <see cref="System.Numerics.Plane" /> conversions.</summary>
/// <typeparam name="TSelf">The single-precision plane type, such as <c>Plane3</c>.</typeparam>
public interface IPlane3SystemNumerics<TSelf>
    where TSelf : struct, IPlane3SystemNumerics<TSelf>
{
    /// <summary>Converts a single-precision plane to <see cref="System.Numerics.Plane" />.</summary>
    static abstract explicit operator System.Numerics.Plane(TSelf value);

    /// <summary>Converts a <see cref="System.Numerics.Plane" /> to a single-precision plane.</summary>
    static abstract explicit operator TSelf(System.Numerics.Plane value);
}
