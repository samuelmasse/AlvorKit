namespace AlvorKit;

/// <summary>Shows the equivalent graph using typed members and graph-owned native lifetime.</summary>
internal class TypedNoisePattern(Fn fn)
{
    /// <summary>Builds and samples the same FractalFBm-over-CellularValue graph through the helper package.</summary>
    public void Sample(Span<float> output)
    {
        using var graph = new FnGraph(fn);

        var source = graph.Create(FnNodeType.CellularValue)
            .Float(FnFloatVariable.FeatureScale, 112f)
            .Integer(FnIntegerVariable.SeedOffset, 0)
            .Float(FnFloatVariable.OutputMinimum, -1f)
            .Float(FnFloatVariable.OutputMaximum, 1f)
            .Integer(FnIntegerVariable.ValueIndex, 0)
            .DistanceFunction(FnDistanceFunction.EuclideanSquared)
            .Hybrid(FnHybrid.GridJitter, 1f);

        var root = graph.Create(FnNodeType.FractalFbm)
            .Integer(FnIntegerVariable.Octaves, 5)
            .Float(FnFloatVariable.Lacunarity, 2.05f)
            .Hybrid(FnHybrid.Gain, 0.5f)
            .Hybrid(FnHybrid.WeightedStrength, 0.12f)
            .Source(FnSource.Source, source);

        Vec3 offset = (-3f, 2f, 11f);
        Vec3i count = (4, 3, 2);
        Vec3 step = (0.5f, 0.75f, 1.25f);
        root.GenUniformGrid3D(output, offset, count, step, 4242);
    }
}
