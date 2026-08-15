namespace AlvorKit;

/// <summary>
/// Keys one reusable exact-signature dispatch artifact without retaining per-mock or per-call state.
/// </summary>
internal sealed class MockDispatchCacheKey : IEquatable<MockDispatchCacheKey>
{
    private readonly MockTypeIdentity runtimeType;
    private readonly MockBackendIdentity backend;
    private readonly MockOperationKind operation;
    private readonly MockMethodIdentity method;
    private readonly MockCanonicalSignature signature;

    private MockDispatchCacheKey(
        MockTypeIdentity runtimeType,
        MockBackendIdentity backend,
        MockOperationKind operation,
        MockMethodIdentity method,
        MockCanonicalSignature signature)
    {
        this.runtimeType = runtimeType;
        this.backend = backend;
        this.operation = operation;
        this.method = method;
        this.signature = signature;
    }

    internal MockTypeIdentity RuntimeType => runtimeType;
    internal MockBackendIdentity Backend => backend;
    internal MockOperationKind Operation => operation;
    internal MockMethodIdentity Method => method;
    internal MockCanonicalSignature Signature => signature;

    /// <summary>
    /// Builds a key and its canonical signature from the exact runtime construction.
    /// </summary>
    internal static MockDispatchCacheKey Create(
        Type runtimeType,
        MethodBase method,
        MockBackendIdentity backend,
        MockOperationKind operation)
    {
        return Create(runtimeType, method, backend, operation, MockCanonicalSignature.Create(method));
    }

    /// <summary>
    /// Builds a key using a canonical signature already produced by pre-installation validation.
    /// </summary>
    internal static MockDispatchCacheKey Create(
        Type runtimeType,
        MethodBase method,
        MockBackendIdentity backend,
        MockOperationKind operation,
        MockCanonicalSignature signature)
    {
        return new MockDispatchCacheKey(
            new MockTypeIdentity(runtimeType),
            backend,
            operation,
            MockMethodIdentity.Create(method),
            signature);
    }

    /// <inheritdoc />
    public bool Equals(MockDispatchCacheKey? other)
    {
        return other is not null
            && runtimeType == other.runtimeType
            && backend == other.backend
            && operation == other.operation
            && method.Equals(other.method)
            && signature.Equals(other.signature);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MockDispatchCacheKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(runtimeType, backend, operation, method, signature);
}
