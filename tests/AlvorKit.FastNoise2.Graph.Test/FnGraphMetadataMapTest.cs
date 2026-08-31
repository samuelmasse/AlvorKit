namespace AlvorKit;

/// <summary>Verifies every managed member and option mapping against pinned runtime metadata.</summary>
[TestClass]
public class FnGraphMetadataMapTest
{
    private const int FloatVariable = 0;
    private const int IntegerVariable = 1;
    private const int EnumVariable = 2;

    /// <summary>Proves every typed member key resolves to at least one member of the expected runtime kind.</summary>
    [TestMethod]
    public void TypedMemberEnumsMatchRuntimeMetadata()
    {
        var fn = new FnBackend();

        foreach (var variable in Enum.GetValues<FnFloatVariable>())
            Assert.IsTrue(ContainsVariable(fn, FnNames.Float(variable), FloatVariable), variable.ToString());

        foreach (var variable in Enum.GetValues<FnIntegerVariable>())
            Assert.IsTrue(ContainsVariable(fn, FnNames.Integer(variable), IntegerVariable), variable.ToString());

        foreach (var hybrid in Enum.GetValues<FnHybrid>())
            Assert.IsTrue(ContainsHybrid(fn, FnNames.Hybrid(hybrid)), hybrid.ToString());

        foreach (var source in Enum.GetValues<FnSource>())
            Assert.IsTrue(ContainsSource(fn, FnNames.Source(source)), source.ToString());
    }

    /// <summary>Proves every runtime metadata node and non-enum member has a typed managed representation.</summary>
    [TestMethod]
    public void RuntimeMetadataIsExactlyCoveredByTypedEnums()
    {
        var fn = new FnBackend();
        var nodeNames = Enum.GetValues<FnNodeType>().Select(FnNames.Node).ToArray();
        var floatKeys = Enum.GetValues<FnFloatVariable>().Select(FnNames.Float).ToHashSet();
        var integerKeys = Enum.GetValues<FnIntegerVariable>().Select(FnNames.Integer).ToHashSet();
        var hybridKeys = Enum.GetValues<FnHybrid>().Select(FnNames.Hybrid).ToHashSet();
        var sourceKeys = Enum.GetValues<FnSource>().Select(FnNames.Source).ToHashSet();
        var runtimeFloatKeys = new HashSet<FnMemberKey>();
        var runtimeIntegerKeys = new HashSet<FnMemberKey>();
        var runtimeHybridKeys = new HashSet<FnMemberKey>();
        var runtimeSourceKeys = new HashSet<FnMemberKey>();

        Assert.HasCount(fn.GetMetadataCount(), nodeNames);

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            fn.GetMetadataName(metadataId, out var nodeName);
            Assert.AreEqual(nodeNames[metadataId], nodeName, $"Metadata node id {metadataId} is not mapped exactly.");
            AddRuntimeMembers(fn, metadataId, runtimeFloatKeys, runtimeIntegerKeys, runtimeHybridKeys, runtimeSourceKeys);
        }

        AssertSetEquals("float variables", floatKeys, runtimeFloatKeys);
        AssertSetEquals("integer variables", integerKeys, runtimeIntegerKeys);
        AssertSetEquals("hybrids", hybridKeys, runtimeHybridKeys);
        AssertSetEquals("required sources", sourceKeys, runtimeSourceKeys);
    }

    /// <summary>Proves every runtime enum member and option has one exact typed option representation.</summary>
    [TestMethod]
    public void RuntimeOptionsAreExactlyCoveredByTypedEnums()
    {
        var fn = new FnBackend();
        var expected = ExpectedOptions();
        var actual = RuntimeOptions(fn);

        Assert.HasCount(expected.Count, actual);

        foreach (var entry in expected)
        {
            Assert.IsTrue(actual.TryGetValue(entry.Key, out var values), $"Runtime enum '{entry.Key}' is missing.");
            CollectionAssert.AreEqual(entry.Value, values, $"Runtime enum '{entry.Key}' has unexpected options.");
        }
    }

    /// <summary>Proves XYZW component diagnostics use metadata axis labels rather than ASCII adjacency.</summary>
    [TestMethod]
    public void MemberDisplayFormatsEveryDimension()
    {
        Assert.AreEqual("Offset.X", FnMetadata.Display(new("Offset", 0)));
        Assert.AreEqual("Offset.Y", FnMetadata.Display(new("Offset", 1)));
        Assert.AreEqual("Offset.Z", FnMetadata.Display(new("Offset", 2)));
        Assert.AreEqual("Offset.W", FnMetadata.Display(new("Offset", 3)));
    }

    /// <summary>Proves every member occurrence on every runtime node can be configured through the typed wrapper.</summary>
    [TestMethod]
    public void TypedWrapperConfiguresEveryRuntimeMember()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);

        foreach (var nodeType in Enum.GetValues<FnNodeType>())
        {
            var node = graph.Create(nodeType);
            var metadataId = fn.GetMetadataID(node.Native);
            ConfigureVariables(fn, node, metadataId);
            ConfigureHybrids(fn, graph, node, metadataId);
            ConfigureSources(fn, graph, node, metadataId);
        }
    }

    /// <summary>Proves every typed option value is accepted by its corresponding native FastNoise2 enum.</summary>
    [TestMethod]
    public void TypedOptionsMatchRuntimeMetadata()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var distance = graph.Create(FnNodeType.DistanceToPoint);
        var cellular = graph.Create(FnNodeType.CellularDistance);
        var warp = graph.Create(FnNodeType.DomainWarpSimplex);
        var fade = graph.Create(FnNodeType.Fade);
        var remap = graph.Create(FnNodeType.Remap);
        var remove = graph.Create(FnNodeType.RemoveDimension);
        var rotate = graph.Create(FnNodeType.DomainRotatePlane);

        foreach (var value in Enum.GetValues<FnDistanceFunction>())
            distance.DistanceFunction(value);

        foreach (var value in Enum.GetValues<FnCellularReturnType>())
            cellular.CellularReturnType(value);

        foreach (var value in Enum.GetValues<FnVectorizationScheme>())
            warp.VectorizationScheme(value);

        foreach (var value in Enum.GetValues<FnInterpolation>())
            fade.Interpolation(value);

        remap.ClampOutput(false).ClampOutput(true);

        foreach (var value in Enum.GetValues<FnRemovedDimension>())
            remove.RemovedDimension(value);

        foreach (var value in Enum.GetValues<FnRotationType>())
            rotate.RotationType(value);
    }

    private static bool ContainsVariable(Fn fn, FnMemberKey key, int type)
    {
        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var index = 0; index < fn.GetMetadataVariableCount(metadataId); index++)
            {
                fn.GetMetadataVariableName(metadataId, index, out var name);
                var dimension = fn.GetMetadataVariableDimensionIdx(metadataId, index);

                if (Matches(name, dimension, key) && fn.GetMetadataVariableType(metadataId, index) == type)
                    return true;
            }
        }

        return false;
    }

    private static bool ContainsHybrid(Fn fn, FnMemberKey key)
    {
        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var index = 0; index < fn.GetMetadataHybridCount(metadataId); index++)
            {
                fn.GetMetadataHybridName(metadataId, index, out var name);
                var dimension = fn.GetMetadataHybridDimensionIdx(metadataId, index);

                if (Matches(name, dimension, key))
                    return true;
            }
        }

        return false;
    }

    private static bool ContainsSource(Fn fn, FnMemberKey key)
    {
        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var index = 0; index < fn.GetMetadataNodeLookupCount(metadataId); index++)
            {
                fn.GetMetadataNodeLookupName(metadataId, index, out var name);
                var dimension = fn.GetMetadataNodeLookupDimensionIdx(metadataId, index);

                if (Matches(name, dimension, key))
                    return true;
            }
        }

        return false;
    }

    private static bool Matches(string? name, int dimension, FnMemberKey key) =>
        dimension == key.Dimension && string.Equals(name, key.Name, StringComparison.Ordinal);

    private static void ConfigureVariables(Fn fn, FnGraphNode node, int metadataId)
    {
        for (var index = 0; index < fn.GetMetadataVariableCount(metadataId); index++)
        {
            var key = VariableKey(fn, metadataId, index);
            var type = fn.GetMetadataVariableType(metadataId, index);

            if (type == FloatVariable)
            {
                var variable = Enum.GetValues<FnFloatVariable>().Single(value => FnNames.Float(value) == key);
                node.Float(variable, fn.GetMetadataVariableDefaultFloat(metadataId, index));
            }
            else if (type == IntegerVariable)
            {
                var variable = Enum.GetValues<FnIntegerVariable>().Single(value => FnNames.Integer(value) == key);
                node.Integer(variable, fn.GetMetadataVariableDefaultIntEnum(metadataId, index));
            }
            else ConfigureOption(node, key.Name, fn.GetMetadataVariableDefaultIntEnum(metadataId, index));
        }
    }

    private static FnGraphNode ConfigureOption(FnGraphNode node, string name, int option) => name switch
    {
        "Distance Function" => node.DistanceFunction(Enum.GetValues<FnDistanceFunction>()[option]),
        "Return Type" => node.CellularReturnType(Enum.GetValues<FnCellularReturnType>()[option]),
        "Interpolation" => node.Interpolation(Enum.GetValues<FnInterpolation>()[option]),
        "Clamp Output" => node.ClampOutput(option != 0),
        "Remove Dimension" => node.RemovedDimension(Enum.GetValues<FnRemovedDimension>()[option]),
        "Rotation Type" => node.RotationType(Enum.GetValues<FnRotationType>()[option]),
        "Vectorization Scheme" => node.VectorizationScheme(Enum.GetValues<FnVectorizationScheme>()[option]),
        _ => throw new InvalidOperationException($"Runtime enum '{name}' has no typed setter."),
    };

    private static void ConfigureHybrids(Fn fn, FnGraph graph, FnGraphNode node, int metadataId)
    {
        for (var index = 0; index < fn.GetMetadataHybridCount(metadataId); index++)
        {
            fn.GetMetadataHybridName(metadataId, index, out var name);
            var key = new FnMemberKey(name ?? string.Empty, fn.GetMetadataHybridDimensionIdx(metadataId, index));
            var hybrid = Enum.GetValues<FnHybrid>().Single(value => FnNames.Hybrid(value) == key);
            var value = fn.GetMetadataHybridDefault(metadataId, index);
            node.Hybrid(hybrid, value).Hybrid(hybrid, graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, value));
        }
    }

    private static void ConfigureSources(Fn fn, FnGraph graph, FnGraphNode node, int metadataId)
    {
        for (var index = 0; index < fn.GetMetadataNodeLookupCount(metadataId); index++)
        {
            fn.GetMetadataNodeLookupName(metadataId, index, out var name);
            var key = new FnMemberKey(name ?? string.Empty, fn.GetMetadataNodeLookupDimensionIdx(metadataId, index));
            var source = Enum.GetValues<FnSource>().Single(value => FnNames.Source(value) == key);
            var sourceNode = source == FnSource.DomainWarpSource
                ? graph.Create(FnNodeType.DomainWarpGradient)
                : graph.Create(FnNodeType.Constant);
            node.Source(source, sourceNode);
        }
    }

    private static void AddRuntimeMembers(
        Fn fn,
        int metadataId,
        HashSet<FnMemberKey> floatKeys,
        HashSet<FnMemberKey> integerKeys,
        HashSet<FnMemberKey> hybridKeys,
        HashSet<FnMemberKey> sourceKeys)
    {
        for (var index = 0; index < fn.GetMetadataVariableCount(metadataId); index++)
        {
            var key = VariableKey(fn, metadataId, index);
            var type = fn.GetMetadataVariableType(metadataId, index);

            if (type == FloatVariable)
                floatKeys.Add(key);
            else if (type == IntegerVariable)
                integerKeys.Add(key);
        }

        for (var index = 0; index < fn.GetMetadataHybridCount(metadataId); index++)
        {
            fn.GetMetadataHybridName(metadataId, index, out var name);
            hybridKeys.Add(new(name ?? string.Empty, fn.GetMetadataHybridDimensionIdx(metadataId, index)));
        }

        for (var index = 0; index < fn.GetMetadataNodeLookupCount(metadataId); index++)
        {
            fn.GetMetadataNodeLookupName(metadataId, index, out var name);
            sourceKeys.Add(new(name ?? string.Empty, fn.GetMetadataNodeLookupDimensionIdx(metadataId, index)));
        }
    }

    private static Dictionary<string, string[]> ExpectedOptions() => new(StringComparer.Ordinal)
    {
        ["Distance Function"] = [.. Enum.GetValues<FnDistanceFunction>().Select(FnNames.DistanceFunction)],
        ["Return Type"] = [.. Enum.GetValues<FnCellularReturnType>().Select(FnNames.CellularReturnType)],
        ["Interpolation"] = [.. Enum.GetValues<FnInterpolation>().Select(FnNames.Interpolation)],
        ["Clamp Output"] = ["False", "True"],
        ["Remove Dimension"] = [.. Enum.GetValues<FnRemovedDimension>().Select(FnNames.RemovedDimension)],
        ["Rotation Type"] = [.. Enum.GetValues<FnRotationType>().Select(FnNames.RotationType)],
        ["Vectorization Scheme"] =
            [.. Enum.GetValues<FnVectorizationScheme>().Select(FnNames.VectorizationScheme)],
    };

    private static Dictionary<string, string[]> RuntimeOptions(Fn fn)
    {
        var result = new Dictionary<string, string[]>(StringComparer.Ordinal);

        for (var metadataId = 0; metadataId < fn.GetMetadataCount(); metadataId++)
        {
            for (var variableIndex = 0; variableIndex < fn.GetMetadataVariableCount(metadataId); variableIndex++)
            {
                if (fn.GetMetadataVariableType(metadataId, variableIndex) != EnumVariable)
                    continue;

                var key = VariableKey(fn, metadataId, variableIndex);
                Assert.AreEqual(-1, key.Dimension, $"Runtime enum '{key.Name}' unexpectedly has a dimension.");
                var values = new string[fn.GetMetadataEnumCount(metadataId, variableIndex)];

                for (var enumIndex = 0; enumIndex < values.Length; enumIndex++)
                {
                    fn.GetMetadataEnumName(metadataId, variableIndex, enumIndex, out var value);
                    values[enumIndex] = value ?? string.Empty;
                }

                if (result.TryGetValue(key.Name, out var existing))
                    CollectionAssert.AreEqual(existing, values, $"Runtime enum '{key.Name}' differs between nodes.");
                else result.Add(key.Name, values);
            }
        }

        return result;
    }

    private static FnMemberKey VariableKey(Fn fn, int metadataId, int variableIndex)
    {
        fn.GetMetadataVariableName(metadataId, variableIndex, out var name);
        return new(name ?? string.Empty, fn.GetMetadataVariableDimensionIdx(metadataId, variableIndex));
    }

    private static void AssertSetEquals(string label, HashSet<FnMemberKey> expected, HashSet<FnMemberKey> actual)
    {
        var missing = string.Join(", ", expected.Except(actual));
        var unexpected = string.Join(", ", actual.Except(expected));
        Assert.IsTrue(expected.SetEquals(actual), $"{label}: missing [{missing}], unexpected [{unexpected}].");
    }
}
