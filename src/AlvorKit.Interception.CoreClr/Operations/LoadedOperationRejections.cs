namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Creates uniform coordinate-rich operation-recognition diagnostics.</summary>
internal static class LoadedOperationRejections
{
    /// <summary>Creates a rejection related to the candidate operation itself.</summary>
    internal static LoadedOperationRejection AtOperation(
        LoadedIlInstruction operation,
        LoadedOperationRejectionReason reason,
        int metadataToken,
        string detail) =>
        Create(
            operation,
            operation,
            reason,
            metadataToken,
            detail);

    /// <summary>Creates a rejection related to one owned prefix.</summary>
    internal static LoadedOperationRejection AtPrefix(
        LoadedIlInstruction operation,
        LoadedIlInstruction prefix,
        LoadedOperationRejectionReason reason,
        int metadataToken,
        string detail) =>
        Create(operation, prefix, reason, metadataToken, detail);

    /// <summary>Formats one stable rejection without reflection display names.</summary>
    private static LoadedOperationRejection Create(
        LoadedIlInstruction operation,
        LoadedIlInstruction related,
        LoadedOperationRejectionReason reason,
        int metadataToken,
        string detail) =>
        new(
            reason,
            operation.BaselineOffset,
            related.BaselineOffset,
            related.OpCodeValue,
            metadataToken,
            $"Cannot recognize loaded operation '{operation.OpCode.Name}' " +
            $"at IL_{operation.BaselineOffset:X4}: {detail}.");
}
