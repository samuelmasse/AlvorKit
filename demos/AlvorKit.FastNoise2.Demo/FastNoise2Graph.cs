namespace AlvorKit;

/// <summary>Builds one metadata-driven showcase graph and owns every native node handle created for it.</summary>
internal class FastNoise2Graph(Fn fn, FastNoise2Metadata metadata) : IDisposable
{
    private readonly List<FnNode> nodes = [];

    public FnNode Root { get; private set; }

    /// <summary>Replaces the current graph with the curated configuration for one catalog node.</summary>
    public void Build(FastNoise2Feature feature)
    {
        Clear();
        Root = CreateNode(feature.Name);
        WireRequiredSources(Root, feature.Name);
        metadata.ApplyShowcase(Root, feature);
        WireVisualHybridSources(Root, feature.Name);
    }

    /// <summary>Creates and retains a constant node for verifier-driven hybrid connections.</summary>
    public FnNode AddConstant(float value)
    {
        var node = CreateNode("Constant");
        metadata.SetVariable(node, "Value", value);
        return node;
    }

    /// <summary>Releases all caller-owned native references in reverse creation order.</summary>
    public void Clear()
    {
        for (var index = nodes.Count - 1; index >= 0; index--)
            fn.DeleteNodeRef(nodes[index]);

        nodes.Clear();
        Root = default;
    }

    /// <summary>Releases all native node references.</summary>
    public void Dispose() => Clear();

    private FnNode CreateNode(string name)
    {
        var node = fn.NewFromMetadata(metadata.FindId(name), uint.MaxValue);
        if (node == default)
            throw new InvalidOperationException($"FastNoise2 failed to create metadata node '{name}'.");

        nodes.Add(node);
        return node;
    }

    private FnNode CreateSource(string name)
    {
        var source = CreateNode(name);
        metadata.TrySetVariable(source, "Feature Scale", 28f);
        metadata.TrySetVariable(source, "Seed Offset", nodes.Count * 17);
        return source;
    }

    private FnNode CreateWarpSource()
    {
        var warp = CreateNode("DomainWarpSimplex");
        metadata.SetVariable(warp, "Feature Scale", 48f);
        metadata.SetHybridFloat(warp, "Warp Amplitude", 24f);
        metadata.SetLookup(warp, "Source", CreateSource("Simplex"));
        return warp;
    }

    private void WireRequiredSources(FnNode root, string rootName)
    {
        var metadataId = fn.GetMetadataID(root);
        var keys = metadata.LookupKeys(metadataId);

        foreach (var key in keys)
        {
            var source = rootName switch
            {
                "DomainWarpFractalProgressive" or "DomainWarpFractalIndependent" => CreateWarpSource(),
                "Fade" when key == "B" => CreateSource("CellularDistance"),
                _ => CreateSource("Simplex"),
            };

            metadata.SetLookup(root, key, source);
        }
    }

    private void WireVisualHybridSources(FnNode root, string rootName)
    {
        switch (rootName)
        {
            case "Subtract":
                metadata.SetHybridNode(root, "LHS", CreateSource("Simplex"));
                metadata.SetHybridNode(root, "RHS", CreateSource("Perlin"));
                break;
            case "Divide":
                metadata.SetHybridNode(root, "LHS", CreateSource("Simplex"));
                metadata.SetHybridNode(root, "RHS", AddConstant(0.75f));
                break;
            case "Modulus":
                metadata.SetHybridNode(root, "LHS", CreateSource("Simplex"));
                metadata.SetHybridNode(root, "RHS", AddConstant(0.35f));
                break;
            case "PowFloat":
                metadata.SetHybridNode(root, "Value", CreateSource("Simplex"));
                break;
            case "Fade":
                {
                    var fade = CreateSource("Simplex");
                    metadata.SetVariable(fade, "Feature Scale", 96f);
                    metadata.SetHybridNode(root, "Fade", fade);
                    break;
                }
        }
    }
}
