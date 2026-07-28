namespace AlvorKit.Mocking;

/// <summary>
/// Identifies a backend and the ABI version of its generated artifacts.
/// </summary>
internal readonly record struct MockBackendIdentity
{
    private readonly MockBackendKind kind;
    private readonly int abiVersion;

    /// <summary>
    /// Creates an identity for one backend ABI.
    /// </summary>
    internal MockBackendIdentity(MockBackendKind kind, int abiVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(abiVersion);
        this.kind = kind;
        this.abiVersion = abiVersion;
    }

    /// <summary>
    /// Gets the backend kind.
    /// </summary>
    internal MockBackendKind Kind => kind;

    /// <summary>
    /// Gets the backend ABI version.
    /// </summary>
    internal int AbiVersion => abiVersion;

    /// <inheritdoc />
    public override string ToString() => $"{kind} ABI {abiVersion}";
}
