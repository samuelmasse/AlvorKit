namespace AlvorKit;

/// <summary>Shows the repeated raw metadata pattern that existing consumers had to implement themselves.</summary>
internal class OldNoisePattern(Fn fn)
{
    private const uint MaximumFeatureSet = uint.MaxValue;

    /// <summary>Builds, samples, and manually releases a FractalFBm-over-CellularValue graph.</summary>
    public void Sample(Span<float> output)
    {
        FnNode source = default;
        FnNode root = default;

        try
        {
            source = CreateNode("CellularValue");
            SetFloat(source, "Feature Scale", 112f);
            SetInteger(source, "Seed Offset", 0);
            SetFloat(source, "Output Min", -1f);
            SetFloat(source, "Output Max", 1f);
            SetInteger(source, "Value Index", 0);
            SetEnum(source, "Distance Function", "Euclidean Squared");
            SetHybrid(source, "Grid Jitter", 1f);

            root = CreateNode("FractalFBm");
            SetInteger(root, "Octaves", 5);
            SetFloat(root, "Lacunarity", 2.05f);
            SetHybrid(root, "Gain", 0.5f);
            SetHybrid(root, "Weighted Strength", 0.12f);
            SetSource(root, "Source", source);

            fn.GenUniformGrid3D(root, output, -3f, 2f, 11f, 4, 3, 2, 0.5f, 0.75f, 1.25f, 4242);
        }
        finally
        {
            if (root != default)
                fn.DeleteNodeRef(root);

            if (source != default)
                fn.DeleteNodeRef(source);
        }
    }

    private FnNode CreateNode(string wantedName)
    {
        var count = fn.GetMetadataCount();

        for (var metadataId = 0; metadataId < count; metadataId++)
        {
            fn.GetMetadataName(metadataId, out var actualName);

            if (!string.Equals(actualName, wantedName, StringComparison.Ordinal))
                continue;

            var node = fn.NewFromMetadata(metadataId, MaximumFeatureSet);

            if (node != default)
                return node;
        }

        throw new InvalidOperationException($"FastNoise2 did not expose a creatable '{wantedName}' node.");
    }

    private void SetFloat(FnNode node, string wantedName, float value)
    {
        var index = FindVariable(node, wantedName);

        if (!fn.SetVariableFloat(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected float variable '{wantedName}'.");
    }

    private void SetInteger(FnNode node, string wantedName, int value)
    {
        var index = FindVariable(node, wantedName);

        if (!fn.SetVariableIntEnum(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected integer variable '{wantedName}'.");
    }

    private int FindVariable(FnNode node, string wantedName)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataVariableCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataVariableName(metadataId, index, out var actualName);

            if (string.Equals(actualName, wantedName, StringComparison.Ordinal))
                return index;
        }

        throw new InvalidOperationException($"FastNoise2 node has no variable named '{wantedName}'.");
    }

    private void SetEnum(FnNode node, string wantedName, string wantedValue)
    {
        var metadataId = fn.GetMetadataID(node);
        var variableIndex = FindVariable(node, wantedName);
        var count = fn.GetMetadataEnumCount(metadataId, variableIndex);

        for (var enumIndex = 0; enumIndex < count; enumIndex++)
        {
            fn.GetMetadataEnumName(metadataId, variableIndex, enumIndex, out var actualValue);

            if (!string.Equals(actualValue, wantedValue, StringComparison.Ordinal))
                continue;

            if (fn.SetVariableIntEnum(node, variableIndex, enumIndex))
                return;
        }

        throw new InvalidOperationException($"FastNoise2 rejected '{wantedName}' option '{wantedValue}'.");
    }

    private void SetHybrid(FnNode node, string wantedName, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataHybridCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataHybridName(metadataId, index, out var actualName);

            if (!string.Equals(actualName, wantedName, StringComparison.Ordinal))
                continue;

            if (fn.SetHybridFloat(node, index, value))
                return;
        }

        throw new InvalidOperationException($"FastNoise2 rejected hybrid input '{wantedName}'.");
    }

    private void SetSource(FnNode node, string wantedName, FnNode source)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataNodeLookupCount(metadataId);

        for (var index = 0; index < count; index++)
        {
            fn.GetMetadataNodeLookupName(metadataId, index, out var actualName);

            if (!string.Equals(actualName, wantedName, StringComparison.Ordinal))
                continue;

            if (fn.SetNodeLookup(node, index, source))
                return;
        }

        throw new InvalidOperationException($"FastNoise2 rejected required source '{wantedName}'.");
    }
}
