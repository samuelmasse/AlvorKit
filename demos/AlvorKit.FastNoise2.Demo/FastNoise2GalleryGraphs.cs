namespace AlvorKit;

/// <summary>Builds and reuses typed showcase graphs for the gallery's fixed node inventory.</summary>
internal class FastNoise2GalleryGraphs(FnGraph graph)
{
    /// <summary>Keeps each showcase available for repeated selections without allocating more native nodes.</summary>
    private readonly Dictionary<FnNodeType, FnGraphNode> roots = [];

    /// <summary>Tracks creation order within one showcase to reproduce its source seed offsets.</summary>
    private int nodeCount;

    /// <summary>Gets one immutable showcase, allocating native nodes only on its first selection.</summary>
    public FnGraphNode Get(FnNodeType type)
    {
        if (roots.TryGetValue(type, out var existing))
            return existing;

        nodeCount = 0;
        var root = Create(type);
        Configure(root, type);
        roots.Add(type, root);
        return root;
    }

    /// <summary>Creates an independently owned node and advances the current showcase's seed sequence.</summary>
    private FnGraphNode Create(FnNodeType type)
    {
        nodeCount++;
        return graph.Create(type);
    }

    /// <summary>Configures a source with the shared showcase scale and its creation-order seed.</summary>
    private FnGraphNode Source(FnNodeType type) => Create(type)
        .Float(FnFloatVariable.FeatureScale, 28f)
        .Integer(FnIntegerVariable.SeedOffset, nodeCount * 17);

    /// <summary>Builds the domain warp input used by the warp fractal showcases.</summary>
    private FnGraphNode Warp() => Create(FnNodeType.DomainWarpSimplex)
        .Float(FnFloatVariable.FeatureScale, 48f)
        .Hybrid(FnHybrid.WarpAmplitude, 24f)
        .Source(FnSource.Source, Source(FnNodeType.Simplex));

    /// <summary>Builds a constant node when a showcase demonstrates a connected hybrid input.</summary>
    private FnGraphNode Constant(float value) => Create(FnNodeType.Constant).Float(FnFloatVariable.Value, value);

    /// <summary>Wires the representative inputs and parameters for a catalog node.</summary>
    private void Configure(FnGraphNode root, FnNodeType type)
    {
        switch (type)
        {
            case FnNodeType.Constant:
                root.Float(FnFloatVariable.Value, 0.35f);
                return;
            case FnNodeType.White:
                return;
            case FnNodeType.Checkerboard:
                root.Float(FnFloatVariable.FeatureScale, 16f);
                return;
            case FnNodeType.SineWave:
                root.Float(FnFloatVariable.FeatureScale, 24f);
                return;
            case FnNodeType.Gradient:
                root.Float(FnFloatVariable.MultiplierX, 0.015f).Float(FnFloatVariable.MultiplierY, 0.009f)
                    .Hybrid(FnHybrid.OffsetX, 0f).Hybrid(FnHybrid.OffsetY, 0f);
                return;
            case FnNodeType.DistanceToPoint:
                root.DistanceFunction(FnDistanceFunction.Euclidean)
                    .Hybrid(FnHybrid.PointX, 0f).Hybrid(FnHybrid.PointY, 0f).Hybrid(FnHybrid.MinkowskiP, 1.5f);
                return;
            case FnNodeType.Simplex:
            case FnNodeType.SuperSimplex:
            case FnNodeType.Perlin:
            case FnNodeType.Value:
                root.Float(FnFloatVariable.FeatureScale, 32f);
                return;
            case FnNodeType.CellularValue:
            case FnNodeType.CellularDistance:
            case FnNodeType.CellularLookup:
                ConfigureCellular(root, type);
                return;
            case FnNodeType.DomainWarpFractalProgressive:
            case FnNodeType.DomainWarpFractalIndependent:
                root.Source(FnSource.DomainWarpSource, Warp()).Integer(FnIntegerVariable.Octaves, 4)
                    .Float(FnFloatVariable.Lacunarity, 2f)
                    .Hybrid(FnHybrid.Gain, 0.5f).Hybrid(FnHybrid.WeightedStrength, 0f);
                return;
            case FnNodeType.Subtract:
                root.Hybrid(FnHybrid.Lhs, Source(FnNodeType.Simplex)).Hybrid(FnHybrid.Rhs, Source(FnNodeType.Perlin));
                return;
            case FnNodeType.Divide:
            case FnNodeType.Modulus:
                root.Hybrid(FnHybrid.Lhs, Source(FnNodeType.Simplex))
                    .Hybrid(FnHybrid.Rhs, Constant(type == FnNodeType.Divide ? 0.75f : 0.35f));
                return;
            case FnNodeType.PowFloat:
                root.Hybrid(FnHybrid.Power, 2.5f).Hybrid(FnHybrid.Value, Source(FnNodeType.Simplex));
                return;
            case FnNodeType.Fade:
                root.Source(FnSource.A, Source(FnNodeType.Simplex))
                    .Source(FnSource.B, Source(FnNodeType.CellularDistance))
                    .Hybrid(FnHybrid.FadeMinimum, -1f).Hybrid(FnHybrid.FadeMaximum, 1f)
                    .Interpolation(FnInterpolation.Quintic)
                    .Hybrid(FnHybrid.Fade, Source(FnNodeType.Simplex).Float(FnFloatVariable.FeatureScale, 96f));
                return;
            case FnNodeType.Add:
            case FnNodeType.Multiply:
            case FnNodeType.Min:
            case FnNodeType.Max:
            case FnNodeType.MinSmooth:
            case FnNodeType.MaxSmooth:
                root.Source(FnSource.Lhs, Source(FnNodeType.Simplex)).Hybrid(FnHybrid.Rhs, type switch
                {
                    FnNodeType.Add => 0.25f,
                    FnNodeType.Multiply => 0.65f,
                    FnNodeType.Min or FnNodeType.MinSmooth => 0.2f,
                    _ => -0.2f,
                });

                if (type is FnNodeType.MinSmooth or FnNodeType.MaxSmooth)
                    root.Hybrid(FnHybrid.Smoothness, 0.25f);

                return;
            case FnNodeType.PowInt:
                root.Source(FnSource.Value, Source(FnNodeType.Simplex)).Integer(FnIntegerVariable.Power, 3);
                return;
        }

        ConfigureModifier(root, type);
    }

    /// <summary>Configures cellular distance, value, or lookup output for the gallery.</summary>
    private void ConfigureCellular(FnGraphNode root, FnNodeType type)
    {
        if (type == FnNodeType.CellularLookup)
            root.Source(FnSource.Lookup, Source(FnNodeType.Simplex));

        root.Float(FnFloatVariable.FeatureScale, 32f)
            .Hybrid(FnHybrid.MinkowskiP, 1.5f).Hybrid(FnHybrid.GridJitter, 1f).Hybrid(FnHybrid.SizeJitter, 0f)
            .DistanceFunction(type == FnNodeType.CellularDistance
                ? FnDistanceFunction.Euclidean : FnDistanceFunction.EuclideanSquared);

        if (type == FnNodeType.CellularValue)
            root.Integer(FnIntegerVariable.ValueIndex, 0);

        if (type == FnNodeType.CellularDistance)
        {
            root.Integer(FnIntegerVariable.DistanceIndex0, 0).Integer(FnIntegerVariable.DistanceIndex1, 1)
                .CellularReturnType(FnCellularReturnType.Index0AbsoluteDifference1);
        }
    }

    /// <summary>Connects a common Simplex input and configures the selected modifier.</summary>
    private void ConfigureModifier(FnGraphNode root, FnNodeType type)
    {
        root.Source(FnSource.Source, Source(FnNodeType.Simplex));

        switch (type)
        {
            case FnNodeType.FractalFbm:
            case FnNodeType.FractalRidged:
                root.Integer(FnIntegerVariable.Octaves, 5).Float(FnFloatVariable.Lacunarity, 2f)
                    .Hybrid(FnHybrid.Gain, 0.5f).Hybrid(FnHybrid.WeightedStrength, 0f);
                break;
            case FnNodeType.PingPong:
                root.Hybrid(FnHybrid.PingPongStrength, 2.5f);
                break;
            case FnNodeType.DomainWarpSimplex:
            case FnNodeType.DomainWarpSuperSimplex:
            case FnNodeType.DomainWarpGradient:
                root.Float(FnFloatVariable.FeatureScale, 48f).Hybrid(FnHybrid.WarpAmplitude, 24f);

                if (type != FnNodeType.DomainWarpGradient)
                    root.VectorizationScheme(FnVectorizationScheme.OrthogonalGradientMatrix);

                break;
            case FnNodeType.DomainScale:
                root.Float(FnFloatVariable.Scaling, 1.7f);
                break;
            case FnNodeType.DomainOffset:
                root.Hybrid(FnHybrid.OffsetX, 13f).Hybrid(FnHybrid.OffsetY, -7f);
                break;
            case FnNodeType.DomainRotate:
                root.Float(FnFloatVariable.Yaw, 0.7f).Float(FnFloatVariable.Pitch, 0f).Float(FnFloatVariable.Roll, 0f);
                break;
            case FnNodeType.DomainAxisScale:
                root.Float(FnFloatVariable.ScalingX, 1.8f).Float(FnFloatVariable.ScalingY, 0.65f);
                break;
            case FnNodeType.SeedOffset:
                root.Integer(FnIntegerVariable.SeedOffset, 19);
                break;
            case FnNodeType.ConvertRgba8:
                root.Float(FnFloatVariable.Minimum, -1f).Float(FnFloatVariable.Maximum, 1f);
                break;
            case FnNodeType.Remap:
                root.Hybrid(FnHybrid.FromMinimum, -1f).Hybrid(FnHybrid.FromMaximum, 1f)
                    .Hybrid(FnHybrid.ToMinimum, 0f).Hybrid(FnHybrid.ToMaximum, 1f).ClampOutput(true);
                break;
            case FnNodeType.Terrace:
                root.Float(FnFloatVariable.StepCount, 6f).Hybrid(FnHybrid.Smoothness, 0.25f);
                break;
            case FnNodeType.AddDimension:
                root.Hybrid(FnHybrid.NewDimensionPosition, 17f);
                break;
            case FnNodeType.RemoveDimension:
                root.RemovedDimension(FnRemovedDimension.Y);
                break;
            case FnNodeType.DomainRotatePlane:
                root.RotationType(FnRotationType.ImproveXyPlanes);
                break;
            case FnNodeType.Abs:
            case FnNodeType.SignedSquareRoot:
            case FnNodeType.GeneratorCache:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "No gallery showcase is configured.");
        }
    }
}
