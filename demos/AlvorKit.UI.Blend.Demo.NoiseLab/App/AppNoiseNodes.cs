namespace AlvorKit;

/// <summary>Owns the lab's fixed typed node choices and the controls for the currently connected pair.</summary>
[App]
public class AppNoiseNodes
{
    /// <summary>Number of source choices with explicitly authored controls.</summary>
    private const int SourceCount = 6;

    /// <summary>Owns the two reusable fractal roots for this app's lifetime.</summary>
    private readonly FnGraphNode[] fractals;
    /// <summary>Keeps every source choice alive, including choices currently disconnected from both roots.</summary>
    private readonly FnGraphNode[] sources;
    /// <summary>Labels matching the order of the reusable fractal roots.</summary>
    private readonly IReadOnlyList<BlendDropdownItem> fractalItems;
    /// <summary>Labels matching the order of the reusable source nodes.</summary>
    private readonly IReadOnlyList<BlendDropdownItem> sourceItems;
    /// <summary>Controls bound to the currently selected fractal.</summary>
    private AppNoiseParameter[] fractalParameters = [];
    /// <summary>Controls bound to the currently selected source.</summary>
    private AppNoiseParameter[] sourceParameters = [];
    /// <summary>Selected root in the fixed fractal inventory.</summary>
    private int fractalIndex;
    /// <summary>Selected input in the fixed source inventory.</summary>
    private int sourceIndex;

    /// <summary>Gets the connected fractal root used for preview generation.</summary>
    public FnGraphNode Root => fractals[fractalIndex];
    /// <summary>Gets the selected fractal dropdown index.</summary>
    public int FractalIndex => fractalIndex;
    /// <summary>Gets the selected source dropdown index.</summary>
    public int SourceIndex => sourceIndex;
    /// <summary>Gets the fixed fractal dropdown choices.</summary>
    public IReadOnlyList<BlendDropdownItem> Fractals => fractalItems;
    /// <summary>Gets the fixed source dropdown choices.</summary>
    public IReadOnlyList<BlendDropdownItem> Sources => sourceItems;
    /// <summary>Gets the controls that edit the selected fractal.</summary>
    public IReadOnlyList<AppNoiseParameter> FractalParameters => fractalParameters;
    /// <summary>Gets the controls that edit the selected source.</summary>
    public IReadOnlyList<AppNoiseParameter> SourceParameters => sourceParameters;

    /// <summary>Creates a bounded set of nodes once; switching choices reconnects and resets the existing nodes.</summary>
    public AppNoiseNodes(FnGraph graph)
    {
        fractals = [graph.Create(FnNodeType.FractalFbm), graph.Create(FnNodeType.FractalRidged)];
        fractalItems = [new("FractalFbm"), new("FractalRidged")];
        sources = new FnGraphNode[SourceCount];
        var items = new BlendDropdownItem[SourceCount];

        for (var index = 0; index < SourceCount; index++)
        {
            var type = SourceType(index);
            sources[index] = graph.Create(type);
            items[index] = new(type.ToString());
        }

        sourceItems = items;
        ResetParameters();
    }

    /// <summary>Selects a reusable fractal root and resets the connected pair to the lab defaults.</summary>
    public void SelectFractal(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, fractals.Length);
        fractalIndex = index;
        ResetParameters();
    }

    /// <summary>Reconnects the selected source and resets the pair to the lab defaults.</summary>
    public void SelectSource(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, sources.Length);
        sourceIndex = index;
        ResetParameters();
    }

    /// <summary>Rebinds the controls, applies their defaults, and connects the selected nodes.</summary>
    private void ResetParameters()
    {
        var root = Root;
        var source = sources[sourceIndex];
        fractalParameters =
        [
            AppNoiseParameter.Integer(root, FnIntegerVariable.Octaves, "Octaves",
                "Number of noise layers.", 3, 1, 16),
            AppNoiseParameter.Float(root, FnFloatVariable.Lacunarity, "Lacunarity",
                "Frequency multiplier between successive octaves.", 2f, 0.1f, 4f),
            AppNoiseParameter.Hybrid(root, FnHybrid.Gain, "Gain",
                "Amplitude multiplier between successive octaves.", 0.5f),
            AppNoiseParameter.Hybrid(root, FnHybrid.WeightedStrength, "Weighted Strength",
                "Strength of spatial weighting between octaves.", 0f),
        ];
        sourceParameters = SourceParametersFor(source, SourceType(sourceIndex));
        root.Source(FnSource.Source, source);
    }

    /// <summary>Creates only the controls supported by the selected source, including its typed enum choices.</summary>
    private static AppNoiseParameter[] SourceParametersFor(FnGraphNode node, FnNodeType type)
    {
        var parameters = new List<AppNoiseParameter>();

        if (type != FnNodeType.White)
        {
            parameters.Add(AppNoiseParameter.Float(node, FnFloatVariable.FeatureScale, "Feature Scale",
                "Feature size in world units. Larger values make broader features.", 100f, 0.01f, 1024f));
        }

        parameters.Add(AppNoiseParameter.Integer(node, FnIntegerVariable.SeedOffset, "Seed Offset",
            "Changes this source without changing the generation seed.", 0, float.NegativeInfinity, float.PositiveInfinity));
        parameters.Add(AppNoiseParameter.Float(node, FnFloatVariable.OutputMinimum, "Output Min",
            "Lower source output bound before fractal accumulation.", -1f, float.NegativeInfinity, float.PositiveInfinity));
        parameters.Add(AppNoiseParameter.Float(node, FnFloatVariable.OutputMaximum, "Output Max",
            "Upper source output bound before fractal accumulation.", 1f, float.NegativeInfinity, float.PositiveInfinity));

        if (type is not (FnNodeType.CellularValue or FnNodeType.CellularDistance))
            return [.. parameters];

        parameters.Add(AppNoiseParameter.Choice("Distance Function", "Metric used to choose the nearest cellular features.",
            FnDistanceFunction.EuclideanSquared, value => node.DistanceFunction(value)));

        if (type == FnNodeType.CellularValue)
        {
            parameters.Add(AppNoiseParameter.Integer(node, FnIntegerVariable.ValueIndex, "Value Index",
                "Nearest-cell rank whose value is returned.", 0, 0, 3));
        }
        else
        {
            parameters.Add(AppNoiseParameter.Integer(node, FnIntegerVariable.DistanceIndex0, "Distance Index 0",
                "First nearest-cell distance rank.", 0, 0, 3));
            parameters.Add(AppNoiseParameter.Integer(node, FnIntegerVariable.DistanceIndex1, "Distance Index 1",
                "Second nearest-cell distance rank.", 1, 0, 3));
            parameters.Add(AppNoiseParameter.Choice("Return Type", "How the two distance ranks are combined.",
                FnCellularReturnType.Index0, value => node.CellularReturnType(value)));
        }

        parameters.Add(AppNoiseParameter.Hybrid(node, FnHybrid.MinkowskiP, "Minkowski P",
            "Exponent used by the Minkowski distance function.", 1.5f));
        parameters.Add(AppNoiseParameter.Hybrid(node, FnHybrid.GridJitter, "Grid Jitter",
            "Displacement from the regular cellular grid; 1 uses full jitter.", 1f));
        parameters.Add(AppNoiseParameter.Hybrid(node, FnHybrid.SizeJitter, "Size Jitter",
            "Variation in cellular feature sizes.", 0f));
        return [.. parameters];
    }

    /// <summary>Maps the stable dropdown order to supported typed sources.</summary>
    private static FnNodeType SourceType(int index) => index switch
    {
        0 => FnNodeType.Simplex,
        1 => FnNodeType.Perlin,
        2 => FnNodeType.Value,
        3 => FnNodeType.White,
        4 => FnNodeType.CellularValue,
        5 => FnNodeType.CellularDistance,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
