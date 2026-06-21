namespace AlvorKit.Maths;

/// <summary>Applies to matrix types that expose tolerance-based matrix queries.</summary>
/// <typeparam name="TSelf">The matrix type.</typeparam>
/// <typeparam name="TScalar">The component type.</typeparam>
public interface IMatQuery<TSelf, TScalar>
    where TSelf : struct, IMatQuery<TSelf, TScalar>
{
    /// <summary>Returns whether every component is within epsilon of zero.</summary>
    static abstract bool IsNull(TSelf value, TScalar epsilon);

    /// <summary>Returns whether this matrix has identity components within epsilon.</summary>
    static abstract bool IsIdentity(TSelf value, TScalar epsilon);

    /// <summary>Returns whether every component is within epsilon of zero.</summary>
    bool IsNull(TScalar epsilon);

    /// <summary>Returns whether this matrix has identity components within epsilon.</summary>
    bool IsIdentity(TScalar epsilon);
}
