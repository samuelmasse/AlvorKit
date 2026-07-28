namespace AlvorKit.Mocking;

/// <summary>
/// Describes an immutable backend-specific rejection before instrumentation is installed.
/// </summary>
internal sealed class MockSignatureRejection : IEquatable<MockSignatureRejection>
{
    private readonly MockBackendIdentity backend;
    private readonly MockOperationKind operation;
    private readonly MockCanonicalSignature signature;
    private readonly MockUnsupportedSignatureReason reason;
    private readonly string message;

    /// <summary>
    /// Creates a deterministic rejection descriptor.
    /// </summary>
    internal MockSignatureRejection(
        MockBackendIdentity backend,
        MockOperationKind operation,
        MockCanonicalSignature signature,
        MockUnsupportedSignatureReason reason,
        string detail)
    {
        this.backend = backend;
        this.operation = operation;
        this.signature = signature;
        this.reason = reason;
        message = MockDiagnostics.SignatureRejection(
            backend,
            operation,
            signature,
            reason,
            detail);
    }

    internal MockBackendIdentity Backend => backend;
    internal MockOperationKind Operation => operation;
    internal MockCanonicalSignature Signature => signature;
    internal MockUnsupportedSignatureReason Reason => reason;
    internal string Message => message;

    /// <inheritdoc />
    public bool Equals(MockSignatureRejection? other)
    {
        return other is not null
            && backend == other.backend
            && operation == other.operation
            && signature.Equals(other.signature)
            && reason == other.reason
            && message == other.message;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockSignatureRejection other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(backend, operation, signature, reason, message);

    /// <inheritdoc />
    public override string ToString() => message;
}
