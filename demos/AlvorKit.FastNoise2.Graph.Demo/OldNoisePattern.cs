namespace AlvorKit;

/// <summary>Shows the low-level metadata pattern that existing consumers had to implement themselves.</summary>
/// <param name="fn">The borrowed FastNoise2 binding used for construction, sampling, and release.</param>
/// <remarks>This class exists only for comparison. New production code should use <see cref="FnGraph"/>.</remarks>
internal class OldNoisePattern(Fn fn)
{
    private const uint MaximumFeatureSet = uint.MaxValue;

    /// <summary>Builds, samples, and manually releases a FractalFBm-over-CellularValue graph.</summary>
    /// <param name="output">Destination with at least 24 elements for the 4 x 3 x 2 grid.</param>
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

    /// <summary>Finds an exact metadata name and creates one raw native node reference.</summary>
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

    /// <summary>Finds and sets a raw float variable by metadata name.</summary>
    private void SetFloat(FnNode node, string wantedName, float value)
    {
        var index = FindVariable(node, wantedName);

        if (!fn.SetVariableFloat(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected float variable '{wantedName}'.");
    }

    /// <summary>Finds and sets a raw integer variable by metadata name.</summary>
    private void SetInteger(FnNode node, string wantedName, int value)
    {
        var index = FindVariable(node, wantedName);

        if (!fn.SetVariableIntEnum(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected integer variable '{wantedName}'.");
    }

    /// <summary>Linearly resolves a raw variable index by exact metadata name.</summary>
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

    /// <summary>Resolves and sets a raw enum variable and option by exact metadata names.</summary>
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

    /// <summary>Finds a hybrid input by name and assigns its stored float constant.</summary>
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

    /// <summary>Finds a required input by name and connects another raw node.</summary>
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
