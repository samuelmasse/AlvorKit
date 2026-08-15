namespace AlvorKit;

/// <summary>Rejects emitted deltas that add definitions outside the v1 method-body boundary.</summary>
internal static class SourceUpdateDeltaValidator
{
    internal static SourceUpdateDeltaTokens Validate(
        EmitDifferenceResult result,
        byte[] metadataDelta)
    {
        var updatedMethods = result.UpdatedMethods.ToArray();
        var changedTypes = result.ChangedTypes.ToArray();
        if (updatedMethods.Length != 1)
            throw new InvalidOperationException($"Source Update emitted {updatedMethods.Length} updated MethodDefs.");
        if (changedTypes.Length != 1)
            throw new InvalidOperationException($"Source Update emitted {changedTypes.Length} changed TypeDefs.");

        using var provider = MetadataReaderProvider.FromMetadataImage(
            ImmutableArray.Create(metadataDelta));
        var reader = provider.GetMetadataReader();
        var methodToken = MetadataTokens.GetToken(updatedMethods[0]);
        var changedTypeTokens = changedTypes
            .Select(static handle => MetadataTokens.GetToken(handle))
            .ToArray();
        var log = reader.GetEditAndContinueLogEntries();
        foreach (var entry in log)
        {
            var operation = entry.Operation.ToString();
            if (operation.StartsWith("Add", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Source Update delta adds a definition through EnC operation {operation}.");
            }
            if (entry.Handle.Kind == HandleKind.MethodDefinition &&
                MetadataTokens.GetToken(entry.Handle) != methodToken)
            {
                throw new InvalidOperationException("Source Update delta changes an unexpected MethodDef.");
            }
        }

        return new(methodToken, changedTypeTokens);
    }
}
