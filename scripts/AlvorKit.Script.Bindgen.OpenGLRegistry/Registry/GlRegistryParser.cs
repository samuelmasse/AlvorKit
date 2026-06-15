namespace AlvorKit.Script.Bindgen;

/// <summary>Builds the OpenGL binding model from gl.xml plus configuration hints.</summary>
/// <param name="config">Bindgen configuration that selects registry features and naming rules.</param>
public sealed class GlRegistryParser(BindgenConfig config)
{
    /// <summary>Generated catch-all enum name for untyped GLenum positions.</summary>
    private string CatchAllName => config.ApiClass + "Enum";

    /// <summary>Generated catch-all enum name for tokens too wide for the normal catch-all enum.</summary>
    private string WideCatchAllName => config.ApiClass + "WideEnum";

    /// <summary>Parses the registry and returns the complete model consumed by emitters.</summary>
    public GlBindingModel Parse(string registryPath, IReadOnlyDictionary<string, XmlDocComment> docs)
    {
        var registry = XDocument.Load(registryPath).Root
            ?? throw new InvalidOperationException($"{registryPath} has no root element.");

        var featureSet = new GlRegistryFeatureSelector(config).Select(registry);
        var esSince = config.GlEsApi.Length > 0 ? GlRegistryFeatureSelector.ScanApiAvailability(registry, config.GlEsApi) : [];
        GlAvailability Availability(string name) => new(featureSet.Since[name], esSince.GetValueOrDefault(name));

        var tokens = new GlRegistryTokenCollector(config).Collect(registry, featureSet.Tokens, Availability);
        var groupSet = new GlRegistryGroupBuilder(config, CatchAllName).Build(tokens);
        var callbackDefinitions = new GlCallbackTypedefParser(config).Parse(registry);
        var commandSet = new GlRegistryCommandBuilder(config, CatchAllName).Build(
            registry,
            featureSet.Commands,
            Availability,
            docs,
            groupSet.ManagedNameByGroup,
            callbackDefinitions.ManagedNames);

        var narrow = tokens.Where(token => token.Value <= uint.MaxValue);
        var allTokens = new GlEnumGroup(
            "GLenum",
            CatchAllName,
            IsFlags: false,
            GlRegistryMemberSorter.Sort(narrow, groupSet.ManagedNameByGroup));
        var wideTokens = tokens.Where(token => token.Value > uint.MaxValue).ToList();
        var wideGroup = wideTokens.Count == 0
            ? null
            : new GlEnumGroup(
                "GLwide",
                WideCatchAllName,
                IsFlags: false,
                GlRegistryMemberSorter.Sort(wideTokens, groupSet.ManagedNameByGroup))
            { UnderlyingType = "ulong" };
        var handleTypes = commandSet.HandleTypes.Count == 0
            ? commandSet.HandleTypes
            : commandSet.HandleTypes.Append("GlHandle").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var delegates = new GlCallbackDelegateBuilder(config, CatchAllName).Build(
            callbackDefinitions.Signatures,
            commandSet.UsedCallbacks,
            groupSet.ManagedNameByGroup);

        return new(
            groupSet.Groups,
            allTokens,
            wideGroup,
            commandSet.Commands,
            commandSet.UngroupedEnumUses,
            commandSet.SkippedCommands,
            handleTypes,
            delegates);
    }
}
