namespace AlvorKit.Maths;

/// <summary>Applies to all 3D axis-aligned box types.</summary>
public interface IBox3<TSelf, TScalar, TVector> : IBox<TSelf, TScalar, TVector>
    where TSelf : struct, IBox3<TSelf, TScalar, TVector>
{
    /// <summary>Gets the size along the X axis.</summary>
    TScalar Width { get; }

    /// <summary>Gets the size along the Y axis.</summary>
    TScalar Height { get; }

    /// <summary>Gets the size along the Z axis.</summary>
    TScalar Depth { get; }

    /// <summary>Gets the 3D volume.</summary>
    TScalar Volume { get; }
}
