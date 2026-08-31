namespace AlvorKit;

/// <summary>Resolves typed graph members against exact FastNoise2 1.1.1 runtime metadata.</summary>
/// <param name="fn">The borrowed binding used for metadata queries.</param>
/// <remarks>
/// Every lookup linearly scans the relevant cold metadata table and matches both ordinal name and dimension. Runtime
/// variable kind codes are 0 for float, 1 for integer, and 2 for enum.
/// </remarks>
internal class FnMetadata(Fn fn)
{
    private const int FloatVariable = 0;
    private const int IntegerVariable = 1;
    private const int EnumVariable = 2;

    /// <summary>Finds a node metadata ID by exact, case-sensitive name.</summary>
    public int FindNode(string name)
    {
        var count = fn.GetMetadataCount();

        for (var id = 0; id < count; id++)
        {
            fn.GetMetadataName(id, out var actualName);

            if (string.Equals(actualName, name, StringComparison.Ordinal))
                return id;
        }

        throw new InvalidOperationException($"FastNoise2 did not expose node metadata named '{name}'.");
    }

    /// <summary>Finds an exact float variable index.</summary>
    public int FindFloat(FnNode node, FnMemberKey key) => FindVariable(node, key, FloatVariable);

    /// <summary>Finds an exact integer variable index.</summary>
    public int FindInteger(FnNode node, FnMemberKey key) => FindVariable(node, key, IntegerVariable);

    /// <summary>Finds an exact enum variable index.</summary>
    public int FindEnum(FnNode node, FnMemberKey key) => FindVariable(node, key, EnumVariable);

    /// <summary>Finds an enum option index by exact, case-sensitive label.</summary>
    public int FindEnumOption(FnNode node, int variableIndex, FnMemberKey key, string optionName)
    {
        var metadataId = fn.GetMetadataID(node);
        var optionCount = fn.GetMetadataEnumCount(metadataId, variableIndex);

        for (var optionIndex = 0; optionIndex < optionCount; optionIndex++)
        {
            fn.GetMetadataEnumName(metadataId, variableIndex, optionIndex, out var actualName);

            if (string.Equals(actualName, optionName, StringComparison.Ordinal))
                return optionIndex;
        }

        throw new InvalidOperationException(
            $"FastNoise2 node '{Name(metadataId)}' has no '{Display(key)}' option named '{optionName}'.");
    }

    /// <summary>Finds a hybrid index by exact name and dimension.</summary>
    public int FindHybrid(FnNode node, FnMemberKey key)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataHybridCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataHybridName(metadataId, index, out var actualName);
            var actualDimension = fn.GetMetadataHybridDimensionIdx(metadataId, index);

            if (Matches(actualName, actualDimension, key))
                return index;
        }

        throw Missing(metadataId, "hybrid input", key);
    }

    /// <summary>Finds a required-source index by exact name and dimension.</summary>
    public int FindSource(FnNode node, FnMemberKey key)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataNodeLookupCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataNodeLookupName(metadataId, index, out var actualName);
            var actualDimension = fn.GetMetadataNodeLookupDimensionIdx(metadataId, index);

            if (Matches(actualName, actualDimension, key))
                return index;
        }

        throw Missing(metadataId, "required source", key);
    }

    /// <summary>Returns a node's runtime metadata name.</summary>
    public string Name(FnNode node) => Name(fn.GetMetadataID(node));

    /// <summary>Creates a consistent diagnostic when a native setter rejects a resolved value.</summary>
    public InvalidOperationException Rejected(FnNode node, FnMemberKey key, object value) =>
        new($"FastNoise2 rejected '{Name(node)}.{Display(key)} = {value}'.");

    /// <summary>Finds a variable by exact name, dimension, and runtime kind.</summary>
    private int FindVariable(FnNode node, FnMemberKey key, int expectedType)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataVariableCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataVariableName(metadataId, index, out var actualName);
            var actualDimension = fn.GetMetadataVariableDimensionIdx(metadataId, index);

            if (!Matches(actualName, actualDimension, key))
                continue;

            var actualType = fn.GetMetadataVariableType(metadataId, index);

            if (actualType != expectedType)
            {
                throw new InvalidOperationException(
                    $"FastNoise2 member '{Name(metadataId)}.{Display(key)}' has runtime type {actualType}, " +
                    $"not expected type {expectedType}.");
            }

            return index;
        }

        throw Missing(metadataId, "variable", key);
    }

    /// <summary>Returns a metadata name or a stable numeric diagnostic when native text is absent.</summary>
    private string Name(int metadataId)
    {
        fn.GetMetadataName(metadataId, out var name);
        return name ?? $"metadata {metadataId}";
    }

    /// <summary>Creates a member-not-found diagnostic containing node, kind, and qualified member.</summary>
    private InvalidOperationException Missing(int metadataId, string kind, FnMemberKey key) =>
        new($"FastNoise2 node '{Name(metadataId)}' has no {kind} named '{Display(key)}'.");

    /// <summary>Tests exact ordinal name and dimension equality.</summary>
    private static bool Matches(string? name, int dimension, FnMemberKey key) =>
        dimension == key.Dimension && string.Equals(name, key.Name, StringComparison.Ordinal);

    /// <summary>Formats scalar or component-qualified metadata for diagnostics.</summary>
    internal static string Display(FnMemberKey key) =>
        key.Dimension < 0 ? key.Name : $"{key.Name}.{(char)('X' + key.Dimension)}";
}
