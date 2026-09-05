namespace AlvorKit;

/// <summary>Loads the checked-in curated overlay for FastNoise2 runtime metadata.</summary>
internal static class FastNoise2FeatureCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Loads the versioned feature database through the repository resource root.</summary>
    public static FastNoise2FeatureDatabase Load()
    {
        var path = Path.Combine(ProjectRoot.ResDirectory(typeof(FastNoise2FeatureCatalog)), "fastnoise2", "features.json");
        var database = JsonSerializer.Deserialize<FastNoise2FeatureDatabase>(File.ReadAllText(path), JsonOptions);

        return database ?? throw new InvalidOperationException($"FastNoise2 feature database '{path}' was empty.");
    }
}

/// <summary>Versioned structured knowledge for the FastNoise2 binding, node catalog, and recipes.</summary>
internal class FastNoise2FeatureDatabase
{
    public int SchemaVersion { get; init; }
    public string FastNoiseVersion { get; init; } = string.Empty;
    public string BindingVersion { get; init; } = string.Empty;
    public JsonElement SourceRevision { get; init; }
    public string Purpose { get; init; } = string.Empty;
    public List<string> IntegerVariableNames { get; init; } = [];
    public List<string> CApiSymbols { get; init; } = [];
    public List<FastNoise2SamplingCapability> SamplingCapabilities { get; init; } = [];
    public List<FastNoise2BindingCapability> BindingCapabilities { get; init; } = [];
    public JsonElement WrapperContract { get; init; }
    public JsonElement KnownUpstreamBehavior { get; init; }
    public List<FastNoise2ManagedMethod> ManagedMethods { get; init; } = [];
    public List<FastNoise2ManagedEnum> ManagedEnums { get; init; } = [];
    public List<FastNoise2Feature> Nodes { get; init; } = [];
    public List<FastNoise2Recipe> Recipes { get; init; } = [];
}

/// <summary>One documented wrapper method or overload family.</summary>
internal class FastNoise2ManagedMethod
{
    public string Owner { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

/// <summary>One managed enum and its structured value inventory.</summary>
internal class FastNoise2ManagedEnum
{
    public string Name { get; init; } = string.Empty;
    public List<JsonElement> Values { get; init; } = [];
}

/// <summary>One FastNoise2 generation entry point or batch-output capability.</summary>
internal class FastNoise2SamplingCapability
{
    public string Name { get; init; } = string.Empty;
    public int Dimensions { get; init; }
    public string Layout { get; init; } = string.Empty;
    public string Use { get; init; } = string.Empty;
}

/// <summary>Whether and how the AlvorKit C binding exposes one upstream capability.</summary>
internal class FastNoise2BindingCapability
{
    public string Name { get; init; } = string.Empty;
    public bool Available { get; init; }
    public string Api { get; init; } = string.Empty;
}

/// <summary>Curated meaning, exact runtime member inventory, and representative values for one node.</summary>
internal class FastNoise2Feature
{
    /// <summary>Maps the catalog spelling to its typed node identity; no native metadata ID is used.</summary>
    public FnNodeType Type => Enum.Parse<FnNodeType>(Name, true);

    public string Name { get; init; } = string.Empty;
    public List<string> Groups { get; init; } = [];
    public string Purpose { get; init; } = string.Empty;
    public List<string> Variables { get; init; } = [];
    public List<string> Lookups { get; init; } = [];
    public List<string> Hybrids { get; init; } = [];
    public Dictionary<string, List<string>> Enums { get; init; } = new(StringComparer.Ordinal);
    public FastNoise2Showcase Showcase { get; init; } = new();
}

/// <summary>Safe non-default values used to make one node visually recognizable and verifier-friendly.</summary>
internal class FastNoise2Showcase
{
    public Dictionary<string, float> Variables { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, float> Hybrids { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> Enums { get; init; } = new(StringComparer.Ordinal);
}

/// <summary>Compact graph recommendation stored in the agent knowledge database.</summary>
internal class FastNoise2Recipe
{
    public string Name { get; init; } = string.Empty;
    public string Graph { get; init; } = string.Empty;
    public string Use { get; init; } = string.Empty;
}
