namespace AlvorKit;

/// <summary>Resolves typed graph members against exact FastNoise2 runtime metadata.</summary>
internal class FnMetadata(Fn fn)
{
    private const int FloatVariable = 0;
    private const int IntegerVariable = 1;
    private const int EnumVariable = 2;

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

    public int FindFloat(FnNode node, FnMemberKey key) => FindVariable(node, key, FloatVariable);

    public int FindInteger(FnNode node, FnMemberKey key) => FindVariable(node, key, IntegerVariable);

    public int FindEnum(FnNode node, FnMemberKey key) => FindVariable(node, key, EnumVariable);

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

    public string Name(FnNode node) => Name(fn.GetMetadataID(node));

    public int RequiredSourceCount(FnNode node) => fn.GetMetadataNodeLookupCount(fn.GetMetadataID(node));

    public InvalidOperationException Rejected(FnNode node, FnMemberKey key, object value) =>
        new($"FastNoise2 rejected '{Name(node)}.{Display(key)} = {value}'.");

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

    private string Name(int metadataId)
    {
        fn.GetMetadataName(metadataId, out var name);
        return name ?? $"metadata {metadataId}";
    }

    private InvalidOperationException Missing(int metadataId, string kind, FnMemberKey key) =>
        new($"FastNoise2 node '{Name(metadataId)}' has no {kind} named '{Display(key)}'.");

    private static bool Matches(string? name, int dimension, FnMemberKey key) =>
        dimension == key.Dimension && string.Equals(name, key.Name, StringComparison.Ordinal);

    internal static string Display(FnMemberKey key) =>
        key.Dimension < 0 ? key.Name : $"{key.Name}.{(char)('X' + key.Dimension)}";
}
