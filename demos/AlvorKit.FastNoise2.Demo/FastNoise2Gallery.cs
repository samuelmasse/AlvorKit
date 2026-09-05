namespace AlvorKit;

/// <summary>Coordinates catalog selection, typed showcase graphs, generation modes, and the reusable preview texture.</summary>
internal class FastNoise2Gallery
{
    /// <summary>Provides cached typed showcases owned by the injected graph scope.</summary>
    private readonly FastNoise2GalleryGraphs graphs;
    private readonly FastNoise2FeatureDatabase database;
    private readonly FastNoise2Preview preview;
    private int nodeIndex;
    private FastNoise2PreviewMode mode;
    private int seed = 12345;

    public Texture2D Texture => preview.Texture;
    public FastNoise2Feature Current => database.Nodes[nodeIndex];
    public string Title =>
        $"FastNoise2 {database.FastNoiseVersion} | {nodeIndex + 1}/{database.Nodes.Count} | " +
        $"{string.Join(" + ", Current.Groups)} | {Current.Name} | {mode} | seed {seed}";

    /// <summary>Creates the preview collaborators and generates the first catalog node.</summary>
    public FastNoise2Gallery(FastNoise2GalleryGraphs graphs, RootGl gl, Vec2u size, FastNoise2FeatureDatabase database)
    {
        this.graphs = graphs;
        this.database = database;
        preview = new(gl, size);
        Generate();
    }

    /// <summary>Selects the next catalog node and samples its cached typed graph.</summary>
    public void Next()
    {
        nodeIndex = (nodeIndex + 1) % database.Nodes.Count;
        Generate();
    }

    /// <summary>Selects the previous catalog node and samples its cached typed graph.</summary>
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

    /// <summary>Refreshes the selected showcase and prints its catalog description.</summary>
    private void Generate()
    {
        GeneratePreview();
        Console.WriteLine(
            $"{Title}{Environment.NewLine}  {Current.Purpose}{Environment.NewLine}" +
            $"  Variables: {List(Current.Variables)}{Environment.NewLine}" +
            $"  Required sources: {List(Current.Lookups)}{Environment.NewLine}" +
            $"  Hybrids: {List(Current.Hybrids)}");
    }

    /// <summary>Samples the cached graph using the selected shape and seed.</summary>
    private void GeneratePreview() =>
        preview.Generate(graphs.Get(Current.Type), mode, seed, Current.Type == FnNodeType.ConvertRgba8);

    private static string List(IReadOnlyCollection<string> values) => values.Count == 0 ? "none" : string.Join(", ", values);
}
