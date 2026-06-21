namespace AlvorKit.Maths;

/// <summary>Applies to all 2D axis-aligned box types.</summary>
public interface IBox2<TSelf, TScalar, TVector> : IBox<TSelf, TScalar, TVector>
    where TSelf : struct, IBox2<TSelf, TScalar, TVector>
{
    /// <summary>Gets the size along the X axis.</summary>
    TScalar Width { get; }

    /// <summary>Gets the size along the Y axis.</summary>
    TScalar Height { get; }

    /// <summary>Gets the 2D area.</summary>
    TScalar Area { get; }
}
