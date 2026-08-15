namespace AlvorKit;

/// <summary>
/// Carries a canonical signature and an optional immutable rejection from pre-installation validation.
/// </summary>
internal readonly record struct MockSignatureValidation
{
    private readonly MockCanonicalSignature signature;
    private readonly MockSignatureRejection? rejection;

    /// <summary>
    /// Creates a completed validation result.
    /// </summary>
    internal MockSignatureValidation(MockCanonicalSignature signature, MockSignatureRejection? rejection)
    {
        this.signature = signature;
        this.rejection = rejection;
    }

    internal MockCanonicalSignature Signature => signature;
    internal MockSignatureRejection? Rejection => rejection;
    internal bool IsSupported => rejection is null;
}
