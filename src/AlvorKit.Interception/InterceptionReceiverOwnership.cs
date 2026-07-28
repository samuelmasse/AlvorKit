namespace AlvorKit.Interception;

/// <summary>Describes how one exact call shape carries its hidden receiver.</summary>
public enum InterceptionReceiverOwnership
{
    /// <summary>The operation has no hidden receiver.</summary>
    None,

    /// <summary>The receiver is an ordinary managed object reference.</summary>
    Reference,

    /// <summary>The receiver is a managed reference to caller-owned value storage.</summary>
    ManagedReference,

    /// <summary>The receiver is a readonly managed reference to caller-owned value storage.</summary>
    ReadOnlyManagedReference
}
