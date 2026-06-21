namespace AlvorKit.Maths;

/// <summary>Applies to all two-component vector types, including <c>Vec2</c>, <c>Vec2i</c>, and <c>Vec2b</c>.</summary>
/// <typeparam name="TSelf">The concrete two-component vector type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" />, <see cref="int" />, or <see cref="bool" />.</typeparam>
public interface IVec2<TSelf, TScalar> : IVec<TSelf, TScalar>
    where TSelf : struct, IVec2<TSelf, TScalar>
{
    /// <summary>Creates a vector from X and Y components.</summary>
    static abstract TSelf Create(TScalar x, TScalar y);

    /// <summary>Creates a vector from an X/Y tuple.</summary>
    static abstract implicit operator TSelf((TScalar X, TScalar Y) value);

    /// <summary>Returns this vector as an X/Y tuple.</summary>
    static abstract implicit operator (TScalar X, TScalar Y)(TSelf value);

    /// <summary>Deconstructs this vector into X and Y components.</summary>
    void Deconstruct(out TScalar x, out TScalar y);
}
