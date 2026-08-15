namespace AlvorKit;

/// <summary>Identifies an IL prefix accepted for exact original-operation replay.</summary>
public enum LoadedOperationPrefixKind
{
    /// <summary>The field operation retains volatile memory semantics.</summary>
    Volatile,

    /// <summary>The call retains a concrete constrained receiver type.</summary>
    Constrained
}
