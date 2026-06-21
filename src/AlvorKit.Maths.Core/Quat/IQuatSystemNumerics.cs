namespace AlvorKit.Maths;

/// <summary>Applies to quaternion types that convert to and from <see cref="System.Numerics.Quaternion" />.</summary>
/// <typeparam name="TSelf">The concrete quaternion type.</typeparam>
public interface IQuatSystemNumerics<TSelf>
    where TSelf : struct, IQuatSystemNumerics<TSelf>
{
    /// <summary>Converts to a System.Numerics quaternion.</summary>
    static abstract explicit operator System.Numerics.Quaternion(TSelf value);

    /// <summary>Converts from a System.Numerics quaternion.</summary>
    static abstract explicit operator TSelf(System.Numerics.Quaternion value);
}
