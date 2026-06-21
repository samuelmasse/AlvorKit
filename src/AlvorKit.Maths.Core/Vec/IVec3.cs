namespace AlvorKit.Maths;

/// <summary>Applies to all three-component vector types, including <c>Vec3</c>, <c>Vec3i</c>, and <c>Vec3b</c>.</summary>
/// <typeparam name="TSelf">The concrete three-component vector type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" />, <see cref="int" />, or <see cref="bool" />.</typeparam>
public interface IVec3<TSelf, TScalar> : IVec<TSelf, TScalar>
    where TSelf : struct, IVec3<TSelf, TScalar>
{
    /// <summary>Creates a vector from X, Y, and Z components.</summary>
    static abstract TSelf Create(TScalar x, TScalar y, TScalar z);

    /// <summary>Creates a vector from an X/Y/Z tuple.</summary>
    static abstract implicit operator TSelf((TScalar X, TScalar Y, TScalar Z) value);

    /// <summary>Returns this vector as an X/Y/Z tuple.</summary>
    static abstract implicit operator (TScalar X, TScalar Y, TScalar Z)(TSelf value);

    /// <summary>Deconstructs this vector into X, Y, and Z components.</summary>
    void Deconstruct(out TScalar x, out TScalar y, out TScalar z);
}
