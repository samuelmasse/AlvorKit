namespace AlvorKit;

/// <summary>Coordinates catalog selection, native showcase graphs, generation modes, and the reusable preview texture.</summary>
internal class FastNoise2Gallery
{
    private readonly Fn fn;
    private readonly FastNoise2FeatureDatabase database;
    private readonly FastNoise2Metadata metadata;
    private readonly FastNoise2Preview preview;
    private readonly FastNoise2Graph graph;
    private int nodeIndex;
    private FastNoise2PreviewMode mode;
    private int seed = 12345;

    public Texture2D Texture => preview.Texture;
    public FastNoise2Feature Current => database.Nodes[nodeIndex];
    public string Title =>
        $"FastNoise2 {database.FastNoiseVersion} | {nodeIndex + 1}/{database.Nodes.Count} | " +
        $"{string.Join(" + ", Current.Groups)} | {Current.Name} | {mode} | seed {seed}";

    /// <summary>Creates the preview collaborators and generates the first catalog node.</summary>
    public FastNoise2Gallery(Fn fn, RootGl gl, Vec2u size, FastNoise2FeatureDatabase database)
    {
        this.fn = fn;
        this.database = database;
        metadata = new(fn);
        preview = new(gl, size);
        graph = new(fn, metadata);
        Generate();
    }

    /// <summary>Selects the next catalog node and rebuilds its representative graph.</summary>
    public void Next()
    {
        nodeIndex = (nodeIndex + 1) % database.Nodes.Count;
        Generate();
    }

    /// <summary>Selects the previous catalog node and rebuilds its representative graph.</summary>
    public void Previous()
    {
        nodeIndex = nodeIndex == 0 ? database.Nodes.Count - 1 : nodeIndex - 1;
        Generate();
    }

    /// <summary>Cycles 2D, 3D-slice, 4D-slice, and tileable generation for the current graph.</summary>
    public void NextMode()
    {
        mode = (FastNoise2PreviewMode)(((int)mode + 1) % 4);
        GeneratePreview();
    }

    /// <summary>Changes the global seed and regenerates the current graph.</summary>
    public void Reseed()
    {
        seed = seed > int.MaxValue - 1337 ? 12345 : seed + 1337;
        GeneratePreview();
    }

    /// <summary>Releases all native node handles; the root GL layer owns the texture lifetime.</summary>
    public void Clear() => graph.Clear();

    private void Generate()
    {
        graph.Build(Current);
        GeneratePreview();
        Console.WriteLine(
            $"{Title}{Environment.NewLine}  {Current.Purpose}{Environment.NewLine}" +
            $"  Variables: {List(Current.Variables)}{Environment.NewLine}" +
            $"  Required sources: {List(Current.Lookups)}{Environment.NewLine}" +
            $"  Hybrids: {List(Current.Hybrids)}");
    }

    private void GeneratePreview() => preview.Generate(fn, graph.Root, mode, seed, Current.Name == "ConvertRGBA8");

    private static string List(IReadOnlyCollection<string> values) => values.Count == 0 ? "none" : string.Join(", ", values);
}
