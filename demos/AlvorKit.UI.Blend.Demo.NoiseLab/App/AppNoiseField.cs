namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>
/// Owns the FastNoise2 node graph and the preview texture. The graph is a fractal node over a swappable
/// source node, both created from runtime metadata; the editable parameters are discovered from the same
/// metadata, so the UI never hand-wires a variable list. The sample grid tracks the visible viewport size
/// (one sample per UI unit), so buffers and the texture are recreated on resize and reused per generate.
/// </summary>
[App]
public sealed class AppNoiseField : IDisposable
{
    private const int VariableTypeFloat = 0;
    private const int VariableTypeInt = 1;
    private const int VariableTypeEnum = 2;

    private readonly Fn fn;
    private readonly RootGl gl;
    private readonly float[] minMax = new float[2];
    private readonly AppRamps ramps;
    private readonly List<AppNoiseParameter> fractalParameters = [];
    private readonly List<AppNoiseParameter> sourceParameters = [];
    private float[] values = [];
    private Vec4u8[] pixels = [];
    private FnNode fractalNode;
    private FnNode sourceNode;
    private bool disposed;

    /// <summary>Creates the metadata catalogs and the initial FractalFBm(Simplex) graph; the grid sizes on first resize.</summary>
    public AppNoiseField(Fn fn, RootGl gl, AppRamps ramps)
    {
        this.fn = fn;
        this.gl = gl;
        this.ramps = ramps;

        Fractals = Catalog(["FractalFBm", "FractalRidged", "FractalPingPong"]);
        Sources = Catalog(["Simplex", "OpenSimplex2", "Perlin", "Value", "White", "CellularValue", "CellularDistance"]);
        if (Fractals.Count == 0 || Sources.Count == 0)
            throw new InvalidOperationException("FastNoise2 exposed no usable fractal or source metadata nodes.");

        Texture = CreateTexture((1, 1));
        BuildGraph();
    }

    /// <summary>Gets the creatable fractal node names, in dropdown order.</summary>
    public IReadOnlyList<BlendDropdownItem> Fractals { get; }

    /// <summary>Gets the creatable source node names, in dropdown order.</summary>
    public IReadOnlyList<BlendDropdownItem> Sources { get; }

    /// <summary>Gets the texture receiving generated pixels; replaced when the viewport resizes.</summary>
    public Texture2D Texture { get; private set; }

    /// <summary>Gets the sample grid width; zero until the viewport reports its size.</summary>
    public int Width { get; private set; }

    /// <summary>Gets the sample grid height; zero until the viewport reports its size.</summary>
    public int Height { get; private set; }

    /// <summary>Resizes the sample grid to the visible viewport; returns whether the size changed.</summary>
    public bool Resize(int width, int height)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == Width && height == Height)
            return false;

        Width = width;
        Height = height;
        var count = width * height;
        if (values.Length != count)
        {
            values = new float[count];
            pixels = new Vec4u8[count];
        }

        Texture.Dispose();
        Texture = CreateTexture(((uint)width, (uint)height));
        return true;
    }

    private Texture2D CreateTexture(Vec2u size) => new(gl, size)
    {
        MinFilter = GlTextureMinFilter.Linear,
        MagFilter = GlTextureMagFilter.Linear,
        WrapS = GlTextureWrapMode.ClampToEdge,
        WrapT = GlTextureWrapMode.ClampToEdge,
    };

    /// <summary>Gets the selected fractal catalog index.</summary>
    public int FractalIndex { get; private set; }

    /// <summary>Gets the selected source catalog index.</summary>
    public int SourceIndex { get; private set; }

    /// <summary>Gets the fractal node's editable parameters, metadata variables first, then hybrids.</summary>
    public IReadOnlyList<AppNoiseParameter> FractalParameters => fractalParameters;

    /// <summary>Gets the source node's editable parameters.</summary>
    public IReadOnlyList<AppNoiseParameter> SourceParameters => sourceParameters;

    /// <summary>Gets the smallest sample of the last generation.</summary>
    public float SampleMin { get; private set; }

    /// <summary>Gets the largest sample of the last generation.</summary>
    public float SampleMax { get; private set; }

    /// <summary>Gets the duration of the last generation in milliseconds.</summary>
    public double GenerateMs { get; private set; }

    /// <summary>Returns the last generated sample at a texture pixel, or zero while the grid is unsized.</summary>
    public float Sample(int x, int y)
    {
        if (Width == 0 || Height == 0)
            return 0f;

        return values[(Math.Clamp(y, 0, Height - 1) * Width) + Math.Clamp(x, 0, Width - 1)];
    }

    /// <summary>Rebuilds the graph around a different fractal node, resetting its parameters to metadata defaults.</summary>
    public void SelectFractal(int index)
    {
        FractalIndex = Math.Clamp(index, 0, Fractals.Count - 1);
        BuildGraph();
    }

    /// <summary>Rebuilds the graph around a different source node, resetting its parameters to metadata defaults.</summary>
    public void SelectSource(int index)
    {
        SourceIndex = Math.Clamp(index, 0, Sources.Count - 1);
        BuildGraph();
    }

    /// <summary>Writes one parameter to its node, caching the value for display only when the node accepts it.</summary>
    public void Apply(AppNoiseParameter parameter, float value)
    {
        var node = fractalParameters.Contains(parameter) ? fractalNode : sourceNode;
        var accepted = parameter.Kind switch
        {
            AppNoiseParameterKind.Float => fn.SetVariableFloat(node, parameter.Index, value),
            AppNoiseParameterKind.Hybrid => fn.SetHybridFloat(node, parameter.Index, value),
            _ => fn.SetVariableIntEnum(node, parameter.Index, (int)MathF.Round(value)),
        };

        if (accepted)
            parameter.Value = value;
    }

    /// <summary>Generates a 2D slice at the supplied world offset/step/z, maps it through the ramp, and uploads it.</summary>
    public void Generate(int seed, Vec2 offset, float step, float z, bool normalize, bool invert, int ramp)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Width == 0 || Height == 0)
            return;

        var start = Stopwatch.GetTimestamp();
        fn.GenUniformGrid3D(
            fractalNode,
            values,
            offset.X * step,
            offset.Y * step,
            z,
            Width,
            Height,
            1,
            step,
            step,
            step,
            seed,
            minMax);
        GenerateMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        SampleMin = minMax[0];
        SampleMax = minMax[1];

        var low = normalize ? SampleMin : -1f;
        var high = normalize ? SampleMax : 1f;
        var scale = high > low ? 1f / (high - low) : 0f;

        for (var i = 0; i < values.Length; i++)
        {
            var t = Math.Clamp((values[i] - low) * scale, 0f, 1f);
            if (invert)
                t = 1f - t;
            pixels[i] = ramps.Color(ramp, t);
        }

        Texture.Pixels = pixels;
    }

    /// <summary>Releases the FastNoise2 nodes and the GPU texture.</summary>
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        DeleteNodes();
        Texture.Dispose();
    }

    private void BuildGraph()
    {
        DeleteNodes();
        fractalParameters.Clear();
        sourceParameters.Clear();

        sourceNode = CreateNode(Sources[SourceIndex].Text);
        fractalNode = CreateNode(Fractals[FractalIndex].Text);

        BuildParameters(fractalNode, fractalParameters);
        BuildParameters(sourceNode, sourceParameters);
        WireSource();
    }

    private void DeleteNodes()
    {
        if (fractalNode != default)
            fn.DeleteNodeRef(fractalNode);
        if (sourceNode != default)
            fn.DeleteNodeRef(sourceNode);
        fractalNode = default;
        sourceNode = default;
    }

    private FnNode CreateNode(string name)
    {
        var index = FindMetadata(name);
        var node = fn.NewFromMetadata(index, uint.MaxValue);
        if (node == default)
            throw new InvalidOperationException($"FastNoise2 failed to create the '{name}' node.");

        return node;
    }

    private void WireSource()
    {
        var metadataId = fn.GetMetadataID(fractalNode);
        var count = fn.GetMetadataNodeLookupCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataNodeLookupName(metadataId, i, out var name);
            if (!string.Equals(name, "Source", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!fn.SetNodeLookup(fractalNode, i, sourceNode))
                throw new InvalidOperationException("FastNoise2 rejected the fractal Source node lookup.");

            return;
        }

        throw new InvalidOperationException("The selected fractal node exposes no Source lookup.");
    }

    private void BuildParameters(FnNode node, List<AppNoiseParameter> parameters)
    {
        var metadataId = fn.GetMetadataID(node);

        var variableCount = fn.GetMetadataVariableCount(metadataId);
        for (var i = 0; i < variableCount; i++)
        {
            fn.GetMetadataVariableName(metadataId, i, out var name);
            fn.GetMetadataVariableDescription(metadataId, i, out var description);
            var type = fn.GetMetadataVariableType(metadataId, i);

            var parameter = type switch
            {
                VariableTypeFloat => new AppNoiseParameter
                {
                    Kind = AppNoiseParameterKind.Float,
                    Name = name ?? string.Empty,
                    Tooltip = Tooltip(name, description),
                    Index = i,
                    Min = fn.GetMetadataVariableMinFloat(metadataId, i),
                    Max = fn.GetMetadataVariableMaxFloat(metadataId, i),
                    Value = fn.GetMetadataVariableDefaultFloat(metadataId, i),
                },
                // Int min/max come back through the float getters as reinterpreted bits (the native side
                // returns the storage union), so they must be converted back to their integer values.
                VariableTypeInt => new AppNoiseParameter
                {
                    Kind = AppNoiseParameterKind.Int,
                    Name = name ?? string.Empty,
                    Tooltip = Tooltip(name, description),
                    Index = i,
                    Min = BitConverter.SingleToInt32Bits(fn.GetMetadataVariableMinFloat(metadataId, i)),
                    Max = BitConverter.SingleToInt32Bits(fn.GetMetadataVariableMaxFloat(metadataId, i)),
                    Value = fn.GetMetadataVariableDefaultIntEnum(metadataId, i),
                },
                VariableTypeEnum => new AppNoiseParameter
                {
                    Kind = AppNoiseParameterKind.Enum,
                    Name = name ?? string.Empty,
                    Tooltip = Tooltip(name, description),
                    Index = i,
                    EnumItems = EnumItems(metadataId, i),
                    Value = fn.GetMetadataVariableDefaultIntEnum(metadataId, i),
                },
                _ => null,
            };

            if (parameter == null)
                continue;

            parameters.Add(parameter);
            Apply(parameter, parameter.Value);
        }

        var hybridCount = fn.GetMetadataHybridCount(metadataId);
        for (var i = 0; i < hybridCount; i++)
        {
            fn.GetMetadataHybridName(metadataId, i, out var name);
            fn.GetMetadataHybridDescription(metadataId, i, out var description);

            var parameter = new AppNoiseParameter
            {
                Kind = AppNoiseParameterKind.Hybrid,
                Name = name ?? string.Empty,
                Tooltip = Tooltip(name, description),
                Index = i,
                Value = fn.GetMetadataHybridDefault(metadataId, i),
            };

            parameters.Add(parameter);
            Apply(parameter, parameter.Value);
        }
    }

    private IReadOnlyList<BlendDropdownItem> EnumItems(int metadataId, int variableIndex)
    {
        var count = fn.GetMetadataEnumCount(metadataId, variableIndex);
        var items = new BlendDropdownItem[count];
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataEnumName(metadataId, variableIndex, i, out var name);
            items[i] = new(name ?? string.Empty);
        }

        return items;
    }

    private IReadOnlyList<BlendDropdownItem> Catalog(ReadOnlySpan<string> candidates)
    {
        var items = new List<BlendDropdownItem>(candidates.Length);
        foreach (var candidate in candidates)
        {
            var index = TryFindMetadata(candidate);
            if (index < 0)
                continue;

            var probe = fn.NewFromMetadata(index, uint.MaxValue);
            if (probe == default)
                continue;

            fn.DeleteNodeRef(probe);
            items.Add(new(candidate));
        }

        return items;
    }

    private int FindMetadata(string name)
    {
        var index = TryFindMetadata(name);
        return index >= 0
            ? index
            : throw new InvalidOperationException($"FastNoise2 did not expose a '{name}' metadata node.");
    }

    private int TryFindMetadata(string name)
    {
        var count = fn.GetMetadataCount();
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataName(i, out var metadataName);
            if (string.Equals(metadataName, name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static string Tooltip(string? name, string? description) =>
        string.IsNullOrEmpty(description) ? name ?? string.Empty : $"{name}\n{description}";
}
