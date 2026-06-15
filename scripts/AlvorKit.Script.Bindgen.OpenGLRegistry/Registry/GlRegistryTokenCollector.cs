using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen;

/// <summary>Collects selected OpenGL tokens from registry enum blocks.</summary>
/// <param name="config">Bindgen configuration used for naming and filtering tokens.</param>
internal sealed class GlRegistryTokenCollector(BindgenConfig config)
{
    /// <summary>Collects selected tokens from registry enum blocks.</summary>
    public List<GlRegistryToken> Collect(
        XElement registry,
        HashSet<string> names,
        Func<string, GlAvailability> availability)
    {
        var byName = new Dictionary<string, GlRegistryToken>();
        foreach (var block in registry.Elements("enums"))
            CollectBlock(block, names, availability, byName);

        var missing = names.Where(name => !byName.ContainsKey(name) && !config.SkipConstants.ContainsKey(name)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException($"Required tokens not defined in any <enums> block: {string.Join(", ", missing.Take(10))}");

        GlManagedNameGuard.AssertUnique(byName.Values.Select(token => (token.ManagedName, token.NativeName)), "token");
        return [.. byName.Values];
    }

    /// <summary>Collects selected tokens from a single registry enum block.</summary>
    private void CollectBlock(
        XElement block,
        HashSet<string> names,
        Func<string, GlAvailability> availability,
        IDictionary<string, GlRegistryToken> byName)
    {
        var isBitmask = block.Attribute("type")?.Value == "bitmask";
        foreach (var element in block.Elements("enum"))
        {
            var name = element.Attribute("name")!.Value;
            if (!ShouldCollect(element, name, names))
                continue;

            var token = new GlRegistryToken(
                name,
                CSharpName.FromNativeIdentifier(name, config.Prefix, config.DigitNamePrefix, dimensionSegments: true),
                GlTokenValueParser.Parse(element.Attribute("value")!.Value),
                TokenGroups(element),
                isBitmask,
                availability(name));
            if (!byName.TryAdd(name, token) && byName[name].Value != token.Value)
                throw new InvalidOperationException($"{name} is defined twice with different values.");
        }
    }

    /// <summary>Returns whether one registry enum element should be collected.</summary>
    private bool ShouldCollect(XElement element, string name, IReadOnlySet<string> names)
    {
        if (!names.Contains(name) || config.SkipConstants.ContainsKey(name))
            return false;
        return element.Attribute("api") is not { } api || api.Value == config.GlApi;
    }

    /// <summary>Splits a registry group attribute into native group names.</summary>
    private static string[] TokenGroups(XElement element) =>
        element.Attribute("group")?.Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
}
