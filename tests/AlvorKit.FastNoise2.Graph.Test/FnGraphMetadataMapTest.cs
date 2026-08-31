namespace AlvorKit;

[TestClass]
public class FnGraphMetadataMapTest
{
    private const int FloatVariable = 0;
    private const int IntegerVariable = 1;

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

    /// <summary>Proves every typed option value is accepted by its corresponding native FastNoise2 enum.</summary>
    [TestMethod]
    public void TypedOptionsMatchRuntimeMetadata()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
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
}
