namespace AlvorKit;

/// <summary>Provides exact-name access to FastNoise2's runtime node and member metadata.</summary>
internal class FastNoise2Metadata(Fn fn)
{
    public const int VariableFloat = 0;
    public const int VariableInt = 1;
    public const int VariableEnum = 2;

    /// <summary>Finds a metadata id by its exact runtime node name.</summary>
    public int FindId(string nodeName)
    {
        var count = fn.GetMetadataCount();

        for (var id = 0; id < count; id++)
        {
            if (string.Equals(Name(id), nodeName, StringComparison.Ordinal))
                return id;
        }

        throw new InvalidOperationException($"FastNoise2 did not expose metadata node '{nodeName}'.");
    }

    /// <summary>Gets one exact runtime node name.</summary>
    public string Name(int metadataId)
    {
        fn.GetMetadataName(metadataId, out var name);
        return name ?? string.Empty;
    }

    /// <summary>Gets the metadata name for a live node.</summary>
    public string Name(FnNode node) => Name(fn.GetMetadataID(node));

    /// <summary>Gets exact group names in runtime metadata order.</summary>
    public IReadOnlyList<string> Groups(int metadataId)
    {
        var groups = new string[fn.GetMetadataGroupCount(metadataId)];

        for (var index = 0; index < groups.Length; index++)
        {
            fn.GetMetadataGroupName(metadataId, index, out var name);
            groups[index] = name ?? string.Empty;
        }

        return groups;
    }

    /// <summary>Gets exact qualified variable keys in runtime metadata order.</summary>
    public IReadOnlyList<string> VariableKeys(int metadataId)
    {
        var keys = new string[fn.GetMetadataVariableCount(metadataId)];

        for (var index = 0; index < keys.Length; index++)
        {
            fn.GetMetadataVariableName(metadataId, index, out var name);
            keys[index] = Key(name, fn.GetMetadataVariableDimensionIdx(metadataId, index));
        }

        return keys;
    }

    /// <summary>Gets exact required-source keys in runtime metadata order.</summary>
    public IReadOnlyList<string> LookupKeys(int metadataId)
    {
        var keys = new string[fn.GetMetadataNodeLookupCount(metadataId)];

        for (var index = 0; index < keys.Length; index++)
        {
            fn.GetMetadataNodeLookupName(metadataId, index, out var name);
            keys[index] = Key(name, fn.GetMetadataNodeLookupDimensionIdx(metadataId, index));
        }

        return keys;
    }

    /// <summary>Gets exact hybrid-input keys in runtime metadata order.</summary>
    public IReadOnlyList<string> HybridKeys(int metadataId)
    {
        var keys = new string[fn.GetMetadataHybridCount(metadataId)];

        for (var index = 0; index < keys.Length; index++)
        {
            fn.GetMetadataHybridName(metadataId, index, out var name);
            keys[index] = Key(name, fn.GetMetadataHybridDimensionIdx(metadataId, index));
        }

        return keys;
    }

    /// <summary>Gets all display values for one enum variable.</summary>
    public IReadOnlyList<string> EnumValues(int metadataId, int variableIndex)
    {
        var values = new string[fn.GetMetadataEnumCount(metadataId, variableIndex)];

        for (var enumIndex = 0; enumIndex < values.Length; enumIndex++)
        {
            fn.GetMetadataEnumName(metadataId, variableIndex, enumIndex, out var value);
            values[enumIndex] = value ?? string.Empty;
        }

        return values;
    }

    /// <summary>Gets the runtime type code for one variable key.</summary>
    public int VariableType(int metadataId, string key) => fn.GetMetadataVariableType(metadataId, FindVariable(metadataId, key));

    /// <summary>Gets the default numeric representation for one float or int variable.</summary>
    public float VariableDefault(int metadataId, int variableIndex) =>
        fn.GetMetadataVariableType(metadataId, variableIndex) == VariableFloat
            ? fn.GetMetadataVariableDefaultFloat(metadataId, variableIndex)
            : fn.GetMetadataVariableDefaultIntEnum(metadataId, variableIndex);

    /// <summary>Gets the default constant value for one hybrid input.</summary>
    public float HybridDefault(int metadataId, int hybridIndex) => fn.GetMetadataHybridDefault(metadataId, hybridIndex);

    /// <summary>Applies all curated values for a feature to its live node.</summary>
    public void ApplyShowcase(FnNode node, FastNoise2Feature feature)
    {
        foreach (var variable in feature.Showcase.Variables)
            SetVariable(node, variable.Key, variable.Value);

        foreach (var enumValue in feature.Showcase.Enums)
            SetEnum(node, enumValue.Key, enumValue.Value);

        foreach (var hybrid in feature.Showcase.Hybrids)
            SetHybridFloat(node, hybrid.Key, hybrid.Value);
    }

    /// <summary>Sets one float or int variable by its exact qualified key.</summary>
    public void SetVariable(FnNode node, string key, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var index = FindVariable(metadataId, key);
        var type = fn.GetMetadataVariableType(metadataId, index);
        var accepted = type switch
        {
            VariableFloat => fn.SetVariableFloat(node, index, value),
            VariableInt => fn.SetVariableIntEnum(node, index, (int)MathF.Round(value)),
            _ => throw new InvalidOperationException($"FastNoise2 variable '{Name(metadataId)}.{key}' requires an enum name."),
        };

        if (!accepted)
            throw new InvalidOperationException($"FastNoise2 rejected variable '{Name(metadataId)}.{key}'.");
    }

    /// <summary>Sets a variable when the node exposes the requested unqualified key.</summary>
    public bool TrySetVariable(FnNode node, string key, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var index = TryFindVariable(metadataId, key);
        if (index < 0)
            return false;

        var type = fn.GetMetadataVariableType(metadataId, index);
        return type == VariableFloat
            ? fn.SetVariableFloat(node, index, value)
            : fn.SetVariableIntEnum(node, index, (int)MathF.Round(value));
    }

    /// <summary>Sets one enum variable by exact display value.</summary>
    public void SetEnum(FnNode node, string key, string value)
    {
        var metadataId = fn.GetMetadataID(node);
        var variableIndex = FindVariable(metadataId, key);
        var values = EnumValues(metadataId, variableIndex);

        for (var enumIndex = 0; enumIndex < values.Count; enumIndex++)
        {
            if (!string.Equals(values[enumIndex], value, StringComparison.Ordinal))
                continue;

            if (!fn.SetVariableIntEnum(node, variableIndex, enumIndex))
                throw new InvalidOperationException($"FastNoise2 rejected enum '{Name(metadataId)}.{key}={value}'.");

            return;
        }

        throw new InvalidOperationException($"FastNoise2 enum '{Name(metadataId)}.{key}' has no value '{value}'.");
    }

    /// <summary>Connects one required source by exact qualified key.</summary>
    public void SetLookup(FnNode node, string key, FnNode source)
    {
        var metadataId = fn.GetMetadataID(node);
        var index = FindLookup(metadataId, key);

        if (!fn.SetNodeLookup(node, index, source))
            throw new InvalidOperationException($"FastNoise2 rejected required source '{Name(metadataId)}.{key}'.");
    }

    /// <summary>Sets one hybrid constant by exact qualified key.</summary>
    public void SetHybridFloat(FnNode node, string key, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var index = FindHybrid(metadataId, key);

        if (!fn.SetHybridFloat(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected hybrid constant '{Name(metadataId)}.{key}'.");
    }

    /// <summary>Connects one hybrid node by exact qualified key.</summary>
    public void SetHybridNode(FnNode node, string key, FnNode source)
    {
        var metadataId = fn.GetMetadataID(node);
        var index = FindHybrid(metadataId, key);

        if (!fn.SetHybridNodeLookup(node, index, source))
            throw new InvalidOperationException($"FastNoise2 rejected hybrid source '{Name(metadataId)}.{key}'.");
    }

    private int FindVariable(int metadataId, string key)
    {
        var index = TryFindVariable(metadataId, key);
        return index >= 0
            ? index
            : throw new InvalidOperationException($"FastNoise2 node '{Name(metadataId)}' has no variable '{key}'.");
    }

    private int TryFindVariable(int metadataId, string key)
    {
        var keys = VariableKeys(metadataId);

        for (var index = 0; index < keys.Count; index++)
        {
            if (string.Equals(keys[index], key, StringComparison.Ordinal))
                return index;
        }

        return -1;
    }

    private int FindLookup(int metadataId, string key)
    {
        var keys = LookupKeys(metadataId);

        for (var index = 0; index < keys.Count; index++)
        {
            if (string.Equals(keys[index], key, StringComparison.Ordinal))
                return index;
        }

        throw new InvalidOperationException($"FastNoise2 node '{Name(metadataId)}' has no required source '{key}'.");
    }

    private int FindHybrid(int metadataId, string key)
    {
        var keys = HybridKeys(metadataId);

        for (var index = 0; index < keys.Count; index++)
        {
            if (string.Equals(keys[index], key, StringComparison.Ordinal))
                return index;
        }

        throw new InvalidOperationException($"FastNoise2 node '{Name(metadataId)}' has no hybrid input '{key}'.");
    }

    private static string Key(string? name, int dimension) =>
        dimension < 0 ? name ?? string.Empty : $"{name}.{"XYZW"[dimension]}";
}
