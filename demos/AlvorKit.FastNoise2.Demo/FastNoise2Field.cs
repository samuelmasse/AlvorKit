/// <summary>Owns a Craftdig-style FastNoise2 FBm node, reusable sample buffers, and the texture used by the demo.</summary>
internal sealed class FastNoise2Field : IDisposable
{
    private const string SourceNodeName = "Simplex";
    private const string RootNodeName = "FractalFBm";
    private const float FastNoiseLiteDefaultFrequency = 0.01f;
    private const float FastNoiseLiteDefaultFeatureScale = 1f / FastNoiseLiteDefaultFrequency;
    private const float CraftdigBiasCenterZ = 60f;

    private readonly Fn fn = new FnBackend();
    private readonly FnNode sourceNode;
    private readonly FnNode node;
    private readonly int width;
    private readonly int height;
    private readonly float[] values;
    private readonly float[] minMax = new float[2];
    private readonly (byte Red, byte Green, byte Blue, byte Alpha)[] pixels;
    private bool disposed;

    /// <summary>Creates the FastNoise2 node graph and GPU texture.</summary>
    public FastNoise2Field(RootGl gl, Vec2u size)
    {
        (sourceNode, _) = CreateNode(fn, SourceNodeName);
        SetVariableFloat(fn, sourceNode, "Feature Scale", FastNoiseLiteDefaultFeatureScale);
        SetVariableInt(fn, sourceNode, "Seed Offset", 0);
        SetVariableFloat(fn, sourceNode, "Output Min", -1f);
        SetVariableFloat(fn, sourceNode, "Output Max", 1f);

        (node, _) = CreateNode(fn, RootNodeName);
        SetVariableInt(fn, node, "Octaves", 3);
        SetVariableFloat(fn, node, "Lacunarity", 2f);
        SetHybridFloat(fn, node, "Gain", 0.5f);
        SetHybridFloat(fn, node, "Weighted Strength", 0f);
        SetNodeLookup(fn, node, "Source", sourceNode);

        NodeName = $"{RootNodeName}({SourceNodeName})";
        width = checked((int)size.X);
        height = checked((int)size.Y);
        var pixelCount = checked(width * height);
        values = new float[pixelCount];
        pixels = new (byte, byte, byte, byte)[pixelCount];

        Texture = new Texture2D(gl, "fastnoise2-demo-craftdig-fbm", size)
        {
            MinFilter = GlTextureMinFilter.Linear,
            MagFilter = GlTextureMagFilter.Linear,
            WrapS = GlTextureWrapMode.ClampToEdge,
            WrapT = GlTextureWrapMode.ClampToEdge,
        };
    }

    /// <summary>Gets the texture receiving generated pixels.</summary>
    public Texture2D Texture { get; }

    /// <summary>Gets the generated texture width in pixels.</summary>
    public int Width => width;

    /// <summary>Gets the generated texture height in pixels.</summary>
    public int Height => height;

    /// <summary>Gets the FastNoise2 node metadata graph name selected for the demo.</summary>
    public string NodeName { get; }

    /// <summary>Gets or sets the sample-space pixel offset used for panning.</summary>
    public Vec2 Offset { get; set; }

    /// <summary>Gets or sets the FastNoise2 world-space distance between adjacent samples.</summary>
    public float Step { get; set; } = 1f;

    /// <summary>Gets or sets the native FastNoise2 seed.</summary>
    public int Seed { get; set; } = 12345;

    /// <summary>Changes zoom while preserving the world coordinate under the supplied texture pixel.</summary>
    public bool ZoomAround(Vec2 texturePixel, float nextStep)
    {
        var clampedStep = Math.Clamp(nextStep, 0.15f, 8f);
        if (MathF.Abs(clampedStep - Step) < 0.00001f)
            return false;

        var scale = Step / clampedStep;
        Offset = (
            ((Offset.X + texturePixel.X) * scale) - texturePixel.X,
            ((Offset.Y + texturePixel.Y) * scale) - texturePixel.Y);
        Step = clampedStep;
        return true;
    }

    /// <summary>Regenerates 3D FBm samples, maps them to grayscale RGBA, and uploads them to the texture.</summary>
    public void Regenerate()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        fn.GenUniformGrid3D(
            node,
            values,
            Offset.X * Step,
            Offset.Y * Step,
            CraftdigBiasCenterZ,
            width,
            height,
            1,
            Step,
            Step,
            Step,
            Seed,
            minMax);

        WritePixels();
        Texture.Pixels = pixels;
    }

    /// <summary>Releases the FastNoise2 node and GPU texture.</summary>
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        fn.DeleteNodeRef(node);
        fn.DeleteNodeRef(sourceNode);
        Texture.Dispose();
    }

    private static (FnNode Node, string Name) CreateNode(Fn fn, string nodeName)
    {
        var exact = TryCreateNode(fn, name => string.Equals(name, nodeName, StringComparison.OrdinalIgnoreCase));
        if (exact.Node != default)
            return exact;

        var partial = TryCreateNode(fn, name => name.Contains(nodeName, StringComparison.OrdinalIgnoreCase));
        if (partial.Node != default)
            return partial;

        throw new InvalidOperationException($"FastNoise2 did not expose a creatable {nodeName} metadata node.");
    }

    private static (FnNode Node, string Name) TryCreateNode(Fn fn, Func<string, bool> match)
    {
        var count = fn.GetMetadataCount();
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataName(i, out var metadataName);
            var name = metadataName ?? string.Empty;
            if (!match(name))
                continue;

            var node = fn.NewFromMetadata(i, uint.MaxValue);
            if (node != default)
                return (node, name);
        }

        return (default, string.Empty);
    }

    private static void SetVariableFloat(Fn fn, FnNode node, string variableName, float value)
    {
        var index = FindVariableIndex(fn, node, variableName);
        if (!fn.SetVariableFloat(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected float variable '{variableName}'.");
    }

    private static void SetVariableInt(Fn fn, FnNode node, string variableName, int value)
    {
        var index = FindVariableIndex(fn, node, variableName);
        if (!fn.SetVariableIntEnum(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected int variable '{variableName}'.");
    }

    private static void SetHybridFloat(Fn fn, FnNode node, string hybridName, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataHybridCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataHybridName(metadataId, i, out var name);
            if (!string.Equals(name, hybridName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!fn.SetHybridFloat(node, i, value))
                throw new InvalidOperationException($"FastNoise2 rejected hybrid '{hybridName}'.");

            return;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose hybrid '{hybridName}'.");
    }

    private static void SetNodeLookup(Fn fn, FnNode node, string lookupName, FnNode source)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataNodeLookupCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataNodeLookupName(metadataId, i, out var name);
            if (!string.Equals(name, lookupName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!fn.SetNodeLookup(node, i, source))
                throw new InvalidOperationException($"FastNoise2 rejected node lookup '{lookupName}'.");

            return;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose node lookup '{lookupName}'.");
    }

    private static int FindVariableIndex(Fn fn, FnNode node, string variableName)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataVariableCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataVariableName(metadataId, i, out var name);
            if (string.Equals(name, variableName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose variable '{variableName}'.");
    }

    private void WritePixels()
    {
        for (var i = 0; i < values.Length; i++)
        {
            var value = float.IsFinite(values[i]) ? Math.Clamp(values[i], -1f, 1f) : 0f;
            var gray = Byte((value + 1f) * 127.5f);
            pixels[i] = (gray, gray, gray, 255);
        }
    }

    private static byte Byte(float value) => (byte)Math.Clamp(MathF.Round(value), 0f, 255f);
}
