namespace AlvorKit;

/// <summary>Exhaustively checks the curated database against runtime metadata and exercises every exposed feature family.</summary>
internal class FastNoise2FeatureVerifier(Fn fn, FastNoise2FeatureDatabase database)
{
    private const int Seed = 1337;

    private readonly FastNoise2Metadata metadata = new(fn);

    /// <summary>Runs catalog, node, generation, serialization, SIMD, and concurrency verification.</summary>
    public void Verify()
    {
        VerifyCatalog();
        VerifyEveryNode();
        var activeFeatureSet = VerifyGenerationSurface();

        Console.WriteLine(
            $"FastNoise2 {database.FastNoiseVersion} feature verification PASS: " +
            $"{database.CApiSymbols.Count} C symbols, {database.ManagedMethods.Count} managed signatures, " +
            $"{database.Nodes.Count} nodes, {VariableCount()} variables, {LookupCount()} required sources, " +
            $"{HybridCount()} hybrids, {EnumCount()} enums/{EnumValueCount()} values, " +
            $"{database.SamplingCapabilities.Count} sampling capabilities, active SIMD 0x{activeFeatureSet:X}.");
    }

    private void VerifyCatalog()
    {
        Require(database.SchemaVersion == 3, $"Unsupported FastNoise2 feature database schema {database.SchemaVersion}.");
        Require(database.FastNoiseVersion == "1.1.1", $"Unexpected FastNoise2 catalog version '{database.FastNoiseVersion}'.");
        Require(database.BindingVersion == "1.1.1.3", $"Unexpected binding catalog version '{database.BindingVersion}'.");
        Require(database.SourceRevision.ValueKind == JsonValueKind.Object, "The exact upstream source revision is missing.");
        Require(
            database.SourceRevision.GetProperty("commit").GetString() == "903c1f2d2f9d53ddce94cd223f32727d9ab3aeaa",
            "The audited FastNoise2 source commit changed.");
        Require(
            database.SourceRevision.GetProperty("fastSimdCommit").GetString() ==
                "16450dae9528727e500e7254f635a671f9c7ee2d",
            "The audited FastSIMD source commit changed.");
        Require(database.CApiSymbols.Count == 45, "The FastNoise2 C API symbol inventory is incomplete.");
        Require(
            database.CApiSymbols.Distinct(StringComparer.Ordinal).Count() == database.CApiSymbols.Count,
            "The FastNoise2 C API symbol inventory contains duplicates.");
        Require(database.SamplingCapabilities.Count == 11, "The FastNoise2 sampling capability inventory is incomplete.");
        Require(database.BindingCapabilities.Count == 17, "The FastNoise2 binding capability inventory is incomplete.");
        Require(
            database.SamplingCapabilities.All(value => HasText(value.Name) && HasText(value.Layout) && HasText(value.Use)),
            "A sampling capability is undocumented.");
        Require(
            database.BindingCapabilities.All(value => HasText(value.Name) && HasText(value.Api)),
            "A binding capability is undocumented.");
        Require(database.WrapperContract.ValueKind == JsonValueKind.Object, "The wrapper contract is missing.");
        Require(database.KnownUpstreamBehavior.ValueKind == JsonValueKind.Array, "Upstream behavior notes are missing.");
        Require(database.KnownUpstreamBehavior.GetArrayLength() == 9, "The upstream behavior inventory is incomplete.");
        Require(database.ManagedMethods.Count == 33, "The managed method inventory is incomplete.");
        Require(
            database.ManagedMethods.All(value => HasText(value.Owner) && HasText(value.Signature) && HasText(value.Purpose)),
            "A managed method is undocumented.");
        Require(database.ManagedEnums.Count == 12, "The managed enum inventory is incomplete.");
        Require(
            database.ManagedEnums.All(value => value.Name.Length > 0 && value.Values.Count > 0),
            "A managed enum inventory entry is empty.");
        VerifyManagedEnums();
        Require(database.Recipes.Count == 6, "The FastNoise2 recipe inventory is incomplete.");
        Require(
            database.Recipes.All(value => HasText(value.Name) && HasText(value.Graph) && HasText(value.Use)),
            "A FastNoise2 recipe is undocumented.");
        Require(database.Nodes.Count == fn.GetMetadataCount(), "The FastNoise2 node catalog count does not match runtime metadata.");
        VerifyBehaviorDocumentation();
        VerifyRawBindingSurface();
        VerifyManagedSurface();
        VerifyIntegerVariableCatalog();

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            var feature = database.Nodes[metadataId];
            var runtimeName = metadata.Name(metadataId);
            Require(HasText(feature.Purpose), $"Catalog node '{feature.Name}' has no curated purpose.");
            Require(
                feature.Name == runtimeName,
                $"Catalog node {metadataId} is '{feature.Name}', but runtime metadata is '{runtimeName}'.");
            RequireSequence($"{runtimeName} groups", feature.Groups, metadata.Groups(metadataId));
            RequireSequence($"{runtimeName} variables", feature.Variables, metadata.VariableKeys(metadataId));
            RequireSequence($"{runtimeName} required sources", feature.Lookups, metadata.LookupKeys(metadataId));
            RequireSequence($"{runtimeName} hybrids", feature.Hybrids, metadata.HybridKeys(metadataId));
            VerifyEnumCatalog(metadataId, feature);
            VerifyMetadataDetails(metadataId, runtimeName);
        }
    }

    private void VerifyBehaviorDocumentation()
    {
        foreach (var behavior in database.KnownUpstreamBehavior.EnumerateArray())
        {
            Require(HasText(behavior.GetProperty("name").GetString()), "An upstream behavior note has no name.");
            Require(HasText(behavior.GetProperty("behavior").GetString()), "An upstream behavior note has no behavior.");
            Require(
                HasText(behavior.GetProperty("managedResponse").GetString()),
                "An upstream behavior note has no managed response.");
        }
    }

    private void VerifyMetadataDetails(int metadataId, string nodeName)
    {
        fn.GetMetadataDescription(metadataId, out var description);
        Require(description is not null, $"Runtime node description '{nodeName}' is null.");

        for (var index = 0; index < fn.GetMetadataVariableCount(metadataId); index++)
        {
            fn.GetMetadataVariableDescription(metadataId, index, out var variableDescription);
            Require(variableDescription is not null, $"Runtime variable description '{nodeName}[{index}]' is null.");

            if (fn.GetMetadataVariableType(metadataId, index) != FastNoise2Metadata.VariableFloat)
                continue;

            Require(float.IsFinite(fn.GetMetadataVariableDefaultFloat(metadataId, index)), "A float default is non-finite.");
            Require(float.IsFinite(fn.GetMetadataVariableMinFloat(metadataId, index)), "A float minimum is non-finite.");
            Require(float.IsFinite(fn.GetMetadataVariableMaxFloat(metadataId, index)), "A float maximum is non-finite.");
        }

        for (var index = 0; index < fn.GetMetadataNodeLookupCount(metadataId); index++)
        {
            fn.GetMetadataNodeLookupDescription(metadataId, index, out var lookupDescription);
            Require(lookupDescription is not null, $"Runtime source description '{nodeName}[{index}]' is null.");
        }

        for (var index = 0; index < fn.GetMetadataHybridCount(metadataId); index++)
        {
            fn.GetMetadataHybridDescription(metadataId, index, out var hybridDescription);
            Require(hybridDescription is not null, $"Runtime hybrid description '{nodeName}[{index}]' is null.");
            Require(float.IsFinite(fn.GetMetadataHybridDefault(metadataId, index)), "A hybrid default is non-finite.");
        }
    }

    private void VerifyRawBindingSurface()
    {
        var methods = typeof(Fn).GetMethods().Select(method => method.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var symbol in database.CApiSymbols)
        {
            var managedName = symbol[2..];
            Require(methods.Contains(managedName), $"Raw binding method '{managedName}' is missing for C symbol '{symbol}'.");
        }
    }

    private void VerifyManagedSurface()
    {
        var actual = new Dictionary<string, int>(StringComparer.Ordinal);
        var publicDeclared = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly;

        var publicInstance = publicDeclared | System.Reflection.BindingFlags.Instance;
        Increment(actual, "FnGraph.FnGraph", typeof(FnGraph).GetConstructors(publicInstance).Length);
        AddMethods(actual, typeof(FnGraph), publicInstance);
        AddMethods(actual, typeof(FnGraphNode), publicInstance);
        AddMethods(actual, typeof(FnGraphNodeSampling), publicDeclared | System.Reflection.BindingFlags.Static);
        AddMethods(actual, typeof(FnGraphNodePositionRanges), publicDeclared | System.Reflection.BindingFlags.Static);

        var documented = database.ManagedMethods
            .GroupBy(method => $"{method.Owner}.{SignatureName(method.Signature)}", StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        Require(actual.Count == documented.Count, "The managed API documentation has an unexpected method family.");

        foreach (var entry in actual)
        {
            Require(
                documented.TryGetValue(entry.Key, out var count) && count == entry.Value,
                $"Managed API '{entry.Key}' has {entry.Value} public overloads but the catalog documents {count}.");
        }
    }

    private void VerifyManagedEnums()
    {
        Type[] types =
        [
            typeof(FnNodeType),
            typeof(FnFloatVariable),
            typeof(FnIntegerVariable),
            typeof(FnHybrid),
            typeof(FnSource),
            typeof(FnDistanceFunction),
            typeof(FnCellularReturnType),
            typeof(FnInterpolation),
            typeof(FnRemovedDimension),
            typeof(FnRotationType),
            typeof(FnVectorizationScheme),
            typeof(FnFeatureSet),
        ];

        foreach (var type in types)
        {
            var catalog = database.ManagedEnums.Single(value => value.Name == type.Name);
            var managedNames = catalog.Values.Select(ManagedEnumName).ToArray();
            RequireSequence($"{type.Name} managed values", managedNames, Enum.GetNames(type));

            if (type == typeof(FnFeatureSet))
                VerifyFeatureSetMasks(catalog);
        }
    }

    private static string ManagedEnumName(JsonElement value) => value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? string.Empty
        : value.GetProperty("managed").GetString() ?? string.Empty;

    private static void VerifyFeatureSetMasks(FastNoise2ManagedEnum catalog)
    {
        foreach (var value in catalog.Values)
        {
            var name = ManagedEnumName(value);
            var documented = value.GetProperty("nativeMask").GetUInt32();
            var managed = Convert.ToUInt32(Enum.Parse<FnFeatureSet>(name));
            Require(documented == managed, $"FnFeatureSet.{name} documents mask {documented} but uses {managed}.");
        }
    }

    private static void AddMethods(
        Dictionary<string, int> methods,
        Type owner,
        System.Reflection.BindingFlags flags)
    {
        foreach (var method in owner.GetMethods(flags))
            Increment(methods, $"{owner.Name}.{method.Name}", 1);
    }

    private static void Increment(Dictionary<string, int> counts, string key, int amount)
    {
        counts.TryGetValue(key, out var current);
        counts[key] = current + amount;
    }

    private static string SignatureName(string signature)
    {
        var beforeParameters = signature[..signature.IndexOf('(', StringComparison.Ordinal)];
        var separator = beforeParameters.LastIndexOf(' ');
        return beforeParameters[(separator + 1)..];
    }

    private void VerifyIntegerVariableCatalog()
    {
        var runtimeNames = new List<string>();

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            var keys = metadata.VariableKeys(metadataId);

            for (var variableIndex = 0; variableIndex < keys.Count; variableIndex++)
            {
                if (fn.GetMetadataVariableType(metadataId, variableIndex) != FastNoise2Metadata.VariableInt)
                    continue;

                var key = keys[variableIndex];
                if (!runtimeNames.Contains(key, StringComparer.Ordinal))
                    runtimeNames.Add(key);
            }
        }

        RequireSequence("integer variable names", database.IntegerVariableNames, runtimeNames);
    }

    private void VerifyEnumCatalog(int metadataId, FastNoise2Feature feature)
    {
        var runtimeEnumCount = 0;
        var variableKeys = metadata.VariableKeys(metadataId);

        for (var variableIndex = 0; variableIndex < variableKeys.Count; variableIndex++)
        {
            if (fn.GetMetadataVariableType(metadataId, variableIndex) != FastNoise2Metadata.VariableEnum)
                continue;

            runtimeEnumCount++;
            var key = variableKeys[variableIndex];
            if (!feature.Enums.TryGetValue(key, out var values))
            {
                throw new InvalidOperationException($"Catalog node '{feature.Name}' is missing enum '{key}'.");
            }

            RequireSequence($"{feature.Name}.{key} enum values", values, metadata.EnumValues(metadataId, variableIndex));
        }

        Require(
            feature.Enums.Count == runtimeEnumCount,
            $"Catalog node '{feature.Name}' declares {feature.Enums.Count} enums but runtime metadata has {runtimeEnumCount}.");
    }

    private void VerifyEveryNode()
    {
        foreach (var feature in database.Nodes)
        {
            using var graph = new FastNoise2Graph(fn, metadata);
            graph.Build(feature);
            Require(metadata.Name(graph.Root) == feature.Name, $"Showcase root for '{feature.Name}' has the wrong metadata type.");
            Require(fn.GetActiveFeatureSet(graph.Root) != 0, $"FastNoise2 node '{feature.Name}' reported an invalid SIMD feature set.");

            VerifyVariableSetters(graph.Root, feature);
            VerifyHybridSetters(graph, feature);
            VerifyNodeOutput(graph.Root, feature.Name);
        }
    }

    private void VerifyVariableSetters(FnNode root, FastNoise2Feature feature)
    {
        var metadataId = fn.GetMetadataID(root);
        var keys = metadata.VariableKeys(metadataId);

        for (var variableIndex = 0; variableIndex < keys.Count; variableIndex++)
        {
            var key = keys[variableIndex];
            var type = fn.GetMetadataVariableType(metadataId, variableIndex);
            if (type == FastNoise2Metadata.VariableEnum)
            {
                var enumCount = fn.GetMetadataEnumCount(metadataId, variableIndex);

                for (var enumIndex = 0; enumIndex < enumCount; enumIndex++)
                {
                    Require(
                        fn.SetVariableIntEnum(root, variableIndex, enumIndex),
                        $"FastNoise2 rejected enum index {enumIndex} for '{feature.Name}.{key}'.");
                    VerifyNodeOutput(root, feature.Name);
                }

                continue;
            }

            var value = feature.Showcase.Variables.GetValueOrDefault(key, metadata.VariableDefault(metadataId, variableIndex));
            var accepted = type == FastNoise2Metadata.VariableFloat
                ? fn.SetVariableFloat(root, variableIndex, value)
                : fn.SetVariableIntEnum(root, variableIndex, (int)MathF.Round(value));
            Require(accepted, $"FastNoise2 rejected representative value for '{feature.Name}.{key}'.");
        }
    }

    private void VerifyHybridSetters(FastNoise2Graph graph, FastNoise2Feature feature)
    {
        var metadataId = fn.GetMetadataID(graph.Root);
        var keys = metadata.HybridKeys(metadataId);

        for (var hybridIndex = 0; hybridIndex < keys.Count; hybridIndex++)
        {
            var key = keys[hybridIndex];
            var value = feature.Showcase.Hybrids.GetValueOrDefault(key, metadata.HybridDefault(metadataId, hybridIndex));
            Require(fn.SetHybridFloat(graph.Root, hybridIndex, value), $"FastNoise2 rejected hybrid constant '{feature.Name}.{key}'.");
            VerifyNodeOutput(graph.Root, feature.Name);

            var source = graph.AddConstant(value);
            Require(
                fn.SetHybridNodeLookup(graph.Root, hybridIndex, source),
                $"FastNoise2 rejected hybrid node source '{feature.Name}.{key}'.");
            VerifyNodeOutput(graph.Root, feature.Name);
        }
    }

    private uint VerifyGenerationSurface()
    {
        var feature = database.Nodes.Single(node => node.Name == "FractalFBm");
        using var graph = new FastNoise2Graph(fn, metadata);
        graph.Build(feature);

        VerifyUniformGrids(graph.Root);
        VerifyPositionArrays(graph.Root);
        VerifyTileableAndSingle(graph.Root);
        VerifyEncodedTree();
        VerifyConcurrentGeneration(graph.Root);
        return fn.GetActiveFeatureSet(graph.Root);
    }

    private void VerifyUniformGrids(FnNode root)
    {
        var minMax = new float[2];
        var grid2D = new float[12 * 7];
        fn.GenUniformGrid2D(root, grid2D, -3.25f, 7.5f, 12, 7, 0.75f, 1.25f, Seed, minMax);
        VerifyNumericOutput("GenUniformGrid2D", grid2D, minMax);

        var grid3D = new float[8 * 5 * 3];
        fn.GenUniformGrid3D(root, grid3D, -3f, 2f, 11f, 8, 5, 3, 0.5f, 0.75f, 1.25f, Seed, minMax);
        VerifyNumericOutput("GenUniformGrid3D", grid3D, minMax);

        var grid4D = new float[6 * 4 * 3 * 2];
        fn.GenUniformGrid4D(root, grid4D, -2f, 3f, 5f, 7f, 6, 4, 3, 2, 0.5f, 0.75f, 1f, 1.25f, Seed, minMax);
        VerifyNumericOutput("GenUniformGrid4D", grid4D, minMax);

        fn.GenUniformGrid2D(root, grid2D, 0f, 0f, 12, 7, 1f, 1f, Seed);
        VerifyFinite("GenUniformGrid2D without min/max", grid2D);
    }

    private void VerifyPositionArrays(FnNode root)
    {
        const int count = 31;
        var x = new float[count];
        var y = new float[count];
        var z = new float[count];
        var w = new float[count];
        var output = new float[count];
        var minMax = new float[2];

        for (var index = 0; index < count; index++)
        {
            x[index] = (index * 0.37f) - 4f;
            y[index] = ((index * index) * 0.031f) - 2f;
            z[index] = (index % 7) * 0.63f;
            w[index] = (index % 5) * -0.41f;
        }

        fn.GenPositionArray2D(root, output, count, x, y, 1.5f, -0.25f, Seed, minMax);
        VerifyNumericOutput("GenPositionArray2D", output, minMax);
        fn.GenPositionArray3D(root, output, count, x, y, z, 1.5f, -0.25f, 9f, Seed, minMax);
        VerifyNumericOutput("GenPositionArray3D", output, minMax);
        fn.GenPositionArray4D(root, output, count, x, y, z, w, 1.5f, -0.25f, 9f, 3f, Seed, minMax);
        VerifyNumericOutput("GenPositionArray4D", output, minMax);
    }

    private void VerifyTileableAndSingle(FnNode root)
    {
        var minMax = new float[2];
        var tile = new float[13 * 11];
        fn.GenTileable2D(root, tile, 13, 11, 1f, 1f, Seed, minMax);
        VerifyNumericOutput("GenTileable2D", tile, minMax);

        Require(float.IsFinite(fn.GenSingle2D(root, 1.25f, -7f, Seed)), "GenSingle2D returned a non-finite value.");
        Require(float.IsFinite(fn.GenSingle3D(root, 1.25f, -7f, 3.5f, Seed)), "GenSingle3D returned a non-finite value.");
        Require(float.IsFinite(fn.GenSingle4D(root, 1.25f, -7f, 3.5f, 9f, Seed)), "GenSingle4D returned a non-finite value.");
    }

    private void VerifyEncodedTree()
    {
        const string encodedWikiTree = "DQkGDA==";
        var root = fn.NewFromEncodedNodeTree(encodedWikiTree, uint.MaxValue);
        Require(root != default, "FastNoise2 failed to load the upstream encoded-tree example.");

        try
        {
            var output = new float[9 * 5];
            var minMax = new float[2];
            fn.GenUniformGrid2D(root, output, -2f, 3f, 9, 5, 0.75f, 1.25f, Seed, minMax);
            VerifyNumericOutput("NewFromEncodedNodeTree", output, minMax);
        }
        finally
        {
            fn.DeleteNodeRef(root);
        }
    }

    private void VerifyConcurrentGeneration(FnNode root)
    {
        const int count = 64 * 32;
        var expected = new float[count];
        fn.GenUniformGrid2D(root, expected, -17f, 23f, 64, 32, 0.5f, 0.75f, Seed);

        Parallel.For(0, 4, worker =>
        {
            var output = new float[count];
            fn.GenUniformGrid2D(root, output, -17f, 23f, 64, 32, 0.5f, 0.75f, Seed);
            if (!expected.AsSpan().SequenceEqual(output))
                throw new InvalidOperationException($"Shared-tree concurrent generation differed in worker {worker}.");
        });
    }

    private void VerifyNodeOutput(FnNode root, string nodeName)
    {
        var output = new float[17 * 13];
        var minMax = new float[2];
        fn.GenUniformGrid2D(root, output, -8f, -6f, 17, 13, 1f, 1f, Seed, minMax);

        if (nodeName == "ConvertRGBA8")
        {
            VerifyPackedRgba8(output);
            return;
        }

        VerifyNumericOutput(nodeName, output, minMax);
    }

    private static void VerifyPackedRgba8(ReadOnlySpan<float> output)
    {
        foreach (var value in output)
        {
            var packed = BitConverter.SingleToUInt32Bits(value);
            var red = (byte)packed;
            var green = (byte)(packed >> 8);
            var blue = (byte)(packed >> 16);
            var alpha = (byte)(packed >> 24);
            Require(red == green && green == blue && alpha == 255, "ConvertRGBA8 emitted an invalid packed grayscale pixel.");
        }
    }

    private static void VerifyNumericOutput(string operation, ReadOnlySpan<float> output, ReadOnlySpan<float> minMax)
    {
        VerifyFinite(operation, output);
        var minimum = float.PositiveInfinity;
        var maximum = float.NegativeInfinity;

        foreach (var value in output)
        {
            minimum = MathF.Min(minimum, value);
            maximum = MathF.Max(maximum, value);
        }

        Require(
            MathF.Abs(minimum - minMax[0]) <= 0.00001f && MathF.Abs(maximum - minMax[1]) <= 0.00001f,
            $"{operation} reported min/max [{minMax[0]}, {minMax[1]}] for actual range [{minimum}, {maximum}].");
    }

    private static void VerifyFinite(string operation, ReadOnlySpan<float> output)
    {
        foreach (var value in output)
        {
            if (!float.IsFinite(value))
                throw new InvalidOperationException($"{operation} emitted non-finite numeric output.");
        }
    }

    private int VariableCount()
    {
        var count = 0;

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
            count += fn.GetMetadataVariableCount(metadataId);

        return count;
    }

    private int LookupCount()
    {
        var count = 0;

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
            count += fn.GetMetadataNodeLookupCount(metadataId);

        return count;
    }

    private int HybridCount()
    {
        var count = 0;

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
            count += fn.GetMetadataHybridCount(metadataId);

        return count;
    }

    private int EnumCount()
    {
        var count = 0;

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var variableIndex = 0; variableIndex < fn.GetMetadataVariableCount(metadataId); variableIndex++)
            {
                if (fn.GetMetadataVariableType(metadataId, variableIndex) == FastNoise2Metadata.VariableEnum)
                    count++;
            }
        }

        return count;
    }

    private int EnumValueCount()
    {
        var count = 0;

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var variableIndex = 0; variableIndex < fn.GetMetadataVariableCount(metadataId); variableIndex++)
                count += fn.GetMetadataEnumCount(metadataId, variableIndex);
        }

        return count;
    }

    private static void RequireSequence(string label, IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        Require(expected.Count == actual.Count, $"{label}: catalog has {expected.Count} entries but runtime metadata has {actual.Count}.");

        for (var index = 0; index < expected.Count; index++)
            Require(
                expected[index] == actual[index],
                $"{label}: entry {index} is '{expected[index]}' but runtime metadata is '{actual[index]}'.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);
}
