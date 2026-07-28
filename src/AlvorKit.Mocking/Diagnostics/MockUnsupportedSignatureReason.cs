namespace AlvorKit.Mocking;

/// <summary>
/// Identifies a stable category of pre-installation signature rejection.
/// </summary>
internal enum MockUnsupportedSignatureReason
{
    MissingDeclaringType,
    OpenGenericSignature,
    VariableArguments,
    UnsupportedOperation,
}
