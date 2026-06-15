using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen;

/// <summary>Selects OpenGL registry symbols from features and configured extensions.</summary>
/// <param name="config">Bindgen configuration containing OpenGL version, profile, API, and extensions.</param>
internal sealed class GlRegistryFeatureSelector(BindgenConfig config)
{
    /// <summary>Applies feature blocks and configured extensions into the selected symbol set.</summary>
    public GlFeatureSet Select(XElement registry)
    {
        var commands = new HashSet<string>();
        var tokens = new HashSet<string>();
        var since = new Dictionary<string, string>();
        var features = SelectFeatures(registry);

        foreach (var (feature, number) in features)
        {
            foreach (var require in feature.Elements("require").Where(MatchesProfile))
                ApplyBlock(require, add: true, number.ToString(2), commands, tokens, since);
            foreach (var remove in feature.Elements("remove").Where(MatchesProfile))
                ApplyBlock(remove, add: false, number.ToString(2), commands, tokens, since);
        }

        foreach (var extensionName in config.GlExtensions)
            ApplyExtension(registry, extensionName, commands, tokens, since);
        return new(commands, tokens, since);
    }

    /// <summary>Finds core feature elements at or below the configured OpenGL version.</summary>
    private List<(XElement Feature, Version Number)> SelectFeatures(XElement registry)
    {
        var ceiling = Version.Parse(config.GlVersion!);
        var features = registry.Elements("feature")
            .Where(feature => feature.Attribute("api")?.Value == config.GlApi)
            .Select(feature => (Element: feature, Number: Version.Parse(feature.Attribute("number")!.Value)))
            .Where(feature => feature.Number <= ceiling)
            .OrderBy(feature => feature.Number)
            .ToList();
        if (features.Count == 0)
            throw new InvalidOperationException($"No <feature api=\"{config.GlApi}\"> blocks at or below {config.GlVersion}.");
        return features;
    }

    /// <summary>Applies one configured extension to the selected symbol sets.</summary>
    private void ApplyExtension(
        XElement registry,
        string extensionName,
        ISet<string> commands,
        ISet<string> tokens,
        IDictionary<string, string> since)
    {
        var extension = registry.Element("extensions")?.Elements("extension")
            .FirstOrDefault(element => element.Attribute("name")?.Value == extensionName)
            ?? throw new InvalidOperationException($"Extension {extensionName} not found in the registry.");
        foreach (var require in extension.Elements("require").Where(MatchesProfile).Where(MatchesApi))
            ApplyBlock(require, add: true, extensionName, commands, tokens, since);
    }

    /// <summary>Applies one require/remove block to command and token sets.</summary>
    private static void ApplyBlock(
        XElement block,
        bool add,
        string version,
        ISet<string> commands,
        ISet<string> tokens,
        IDictionary<string, string> since)
    {
        foreach (var (set, elementName) in new[] { (commands, "command"), (tokens, "enum") })
            foreach (var name in block.Elements(elementName).Select(element => element.Attribute("name")!.Value))
            {
                if (add && set.Add(name))
                    since.TryAdd(name, version);
                else if (!add && set.Remove(name))
                    since.Remove(name);
            }
    }

    /// <summary>Finds the first feature version that introduced each name for a companion API.</summary>
    public static Dictionary<string, string> ScanApiAvailability(XElement registry, string api)
    {
        var since = new Dictionary<string, string>();
        var features = registry.Elements("feature")
            .Where(feature => feature.Attribute("api")?.Value == api)
            .Select(feature => (Element: feature, Number: Version.Parse(feature.Attribute("number")!.Value)))
            .OrderBy(feature => feature.Number);
        foreach (var (feature, number) in features)
            foreach (var elementName in new[] { "command", "enum" })
                foreach (var name in feature.Elements("require").Elements(elementName).Select(element => element.Attribute("name")!.Value))
                    since.TryAdd(name, number.ToString(2));
        return since;
    }

    /// <summary>Profile-less registry blocks apply to all profiles; profiled blocks must match config.</summary>
    private bool MatchesProfile(XElement element) =>
        element.Attribute("profile") is not { } profile || profile.Value == config.GlProfile;

    /// <summary>API-less extension requirements apply to all APIs; API-specific ones must match config.</summary>
    private bool MatchesApi(XElement element) =>
        element.Attribute("api") is not { } api || api.Value == config.GlApi;
}
