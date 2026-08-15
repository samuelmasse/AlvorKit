namespace AlvorKit;

/// <summary>
/// Canonicalizes and validates a signature before a backend installs instrumentation.
/// </summary>
internal static class MockSignatureValidator
{
    /// <summary>
    /// Produces the canonical signature and a stable backend-specific rejection when unsupported.
    /// </summary>
    internal static MockSignatureValidation Validate(
        MethodBase method,
        MockBackendIdentity backend,
        MockOperationKind operation)
    {
        MockCanonicalSignature signature = MockCanonicalSignature.Create(method);

        if (method.DeclaringType is null)
        {
            return Reject(
                backend,
                operation,
                signature,
                MockUnsupportedSignatureReason.MissingDeclaringType,
                "the operation has no runtime declaring type");
        }

        if (method.ContainsGenericParameters || method.DeclaringType.ContainsGenericParameters)
        {
            return Reject(
                backend,
                operation,
                signature,
                MockUnsupportedSignatureReason.OpenGenericSignature,
                "the executable signature still contains open generic parameters");
        }

        if ((method.CallingConvention & CallingConventions.VarArgs) != 0)
        {
            return Reject(
                backend,
                operation,
                signature,
                MockUnsupportedSignatureReason.VariableArguments,
                "variable argument lists cannot be represented by an exact typed dispatch frame");
        }

        if (backend.Kind == MockBackendKind.Proxy && operation != MockOperationKind.InstanceMethod)
        {
            return Reject(
                backend,
                operation,
                signature,
                MockUnsupportedSignatureReason.UnsupportedOperation,
                "the proxy backend requires an instance-method receiver");
        }

        return new MockSignatureValidation(signature, null);
    }

    private static MockSignatureValidation Reject(
        MockBackendIdentity backend,
        MockOperationKind operation,
        MockCanonicalSignature signature,
        MockUnsupportedSignatureReason reason,
        string detail)
    {
        return new MockSignatureValidation(
            signature,
            new MockSignatureRejection(backend, operation, signature, reason, detail));
    }
}
