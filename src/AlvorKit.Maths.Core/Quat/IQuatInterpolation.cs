namespace AlvorKit.Maths;

/// <summary>Applies to quaternion types with orientation interpolation and exponential helpers.</summary>
/// <typeparam name="TSelf">The concrete quaternion type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
public interface IQuatInterpolation<TSelf, TScalar>
    where TSelf : struct, IQuatInterpolation<TSelf, TScalar>
{
    /// <summary>Linearly interpolates between two quaternions without normalizing.</summary>
    static abstract TSelf Lerp(TSelf from, TSelf to, TScalar amount);

    /// <summary>Linearly interpolates between two quaternions and normalizes the result.</summary>
    static abstract TSelf Nlerp(TSelf from, TSelf to, TScalar amount);

    /// <summary>Spherically interpolates along the shortest arc.</summary>
    static abstract TSelf Slerp(TSelf from, TSelf to, TScalar amount);

    /// <summary>Spherically interpolates with extra spins along the arc.</summary>
    static abstract TSelf Slerp(TSelf from, TSelf to, TScalar amount, int extraSpins);

    /// <summary>Interpolates between quaternions using squad control points.</summary>
    static abstract TSelf Squad(TSelf from, TSelf to, TSelf fromControl, TSelf toControl, TScalar amount);

    /// <summary>Creates a squad control point for the current quaternion.</summary>
    static abstract TSelf CreateSquadControlPoint(TSelf previous, TSelf current, TSelf next);

    /// <summary>Returns the quaternion exponential.</summary>
    static abstract TSelf Exp(TSelf value);

    /// <summary>Returns the quaternion logarithm.</summary>
    static abstract TSelf Log(TSelf value);

    /// <summary>Raises a quaternion to a scalar power.</summary>
    static abstract TSelf Pow(TSelf value, TScalar exponent);

    /// <summary>Returns the quaternion square root.</summary>
    static abstract TSelf Sqrt(TSelf value);
}
