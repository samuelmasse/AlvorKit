using System.Globalization;
using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen;

/// <summary>Parses the Khronos OpenGL registry (gl.xml) and builds a managed binding model.</summary>
public sealed class GlRegistryParser(BindgenConfig config)
{
    /// <summary>Registry value types by C name. Pointers to any of these become nint.</summary>
    private static readonly Dictionary<string, string> ValueTypes = new()
    {
        ["void"] = "void",
        ["GLvoid"] = "void",
        ["GLboolean"] = "bool",
        ["GLbyte"] = "sbyte",
        ["GLubyte"] = "byte",
        ["GLchar"] = "byte",
        ["GLshort"] = "short",
        ["GLushort"] = "ushort",
        ["GLint"] = "int",
        ["GLuint"] = "uint",
        ["GLsizei"] = "int",
        ["GLenum"] = "uint",
        ["GLbitfield"] = "uint",
        ["GLfloat"] = "float",
        ["GLclampf"] = "float",
        ["GLdouble"] = "double",
        ["GLclampd"] = "double",
        ["GLint64"] = "long",
        ["GLuint64"] = "ulong",
        ["GLintptr"] = "nint",
        ["GLsizeiptr"] = "nint",
        ["GLsync"] = "nint",
        ["GLDEBUGPROC"] = "nint"
    };

    private readonly Dictionary<string, string> managedNameByGroup = [];
    private readonly List<string> ungroupedEnumUses = [];
    private string CatchAllName => config.ApiClass + "Enum";

    public GlBindingModel Parse(string registryPath, IReadOnlyDictionary<string, XmlDocComment> docs)
    {
        var registry = XDocument.Load(registryPath).Root
            ?? throw new InvalidOperationException($"{registryPath} has no root element.");

        var (commandNames, tokenNames, since) = SelectFeatureSet(registry);
        var esSince = config.GlEsApi.Length > 0 ? ScanApiAvailability(registry, config.GlEsApi) : [];
        GlAvailability Availability(string name) => new(since[name], esSince.GetValueOrDefault(name));
        var tokens = CollectTokens(registry, tokenNames, Availability);
        var groups = BuildGroups(tokens);
        var skipped = new List<string>();
        var commands = BuildCommands(registry, commandNames, Availability, docs, skipped);

        var narrow = tokens.Where(token => token.Value <= uint.MaxValue);
        var allTokens = new GlEnumGroup("GLenum", CatchAllName, IsFlags: false, SortMembers(narrow));
        var wideConstants = tokens.Where(token => token.Value > uint.MaxValue)
            .OrderBy(token => token.ManagedName, StringComparer.Ordinal)
            .Select(token => new GlConstant(token.ManagedName, token.NativeName, token.Value, token.Availability))
            .ToList();

        return new(groups, allTokens, commands, wideConstants, ungroupedEnumUses, skipped);
    }

    private sealed record RegistryToken(
        string NativeName, string ManagedName, ulong Value, string[] Groups, bool IsBitmask, GlAvailability Availability);

    /// <summary>
    /// Walks the &lt;feature&gt; blocks up to the configured version in order, applying requires
    /// and profile removes, then the opted-in extensions. Yields the selected command and token
    /// names with the version (or extension) that introduced each.
    /// </summary>
    private (HashSet<string> Commands, HashSet<string> Tokens, Dictionary<string, string> Since) SelectFeatureSet(XElement registry)
    {
        var commands = new HashSet<string>();
        var tokens = new HashSet<string>();
        var since = new Dictionary<string, string>();
        var ceiling = Version.Parse(config.GlVersion!);

        var features = registry.Elements("feature")
            .Where(feature => feature.Attribute("api")?.Value == config.GlApi)
            .Select(feature => (Element: feature, Number: Version.Parse(feature.Attribute("number")!.Value)))
            .Where(feature => feature.Number <= ceiling)
            .OrderBy(feature => feature.Number)
            .ToList();
        if (features.Count == 0)
            throw new InvalidOperationException($"No <feature api=\"{config.GlApi}\"> blocks at or below {config.GlVersion}.");

        foreach (var (feature, number) in features)
        {
            foreach (var require in feature.Elements("require").Where(MatchesProfile))
                Apply(require, add: true, number.ToString(2));
            foreach (var remove in feature.Elements("remove").Where(MatchesProfile))
                Apply(remove, add: false, number.ToString(2));
        }

        foreach (var extensionName in config.GlExtensions)
        {
            var extension = registry.Element("extensions")?.Elements("extension")
                .FirstOrDefault(element => element.Attribute("name")?.Value == extensionName)
                ?? throw new InvalidOperationException($"Extension {extensionName} not found in the registry.");
            foreach (var require in extension.Elements("require").Where(MatchesProfile).Where(MatchesApi))
                Apply(require, add: true, extensionName);
        }

        return (commands, tokens, since);

        void Apply(XElement block, bool add, string version)
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
    }

    /// <summary>A require/remove with no profile applies to all; otherwise it must match.</summary>
    private bool MatchesProfile(XElement element) =>
        element.Attribute("profile") is not { } profile || profile.Value == config.GlProfile;

    /// <summary>An extension require with no api applies to all; otherwise it must match.</summary>
    private bool MatchesApi(XElement element) =>
        element.Attribute("api") is not { } api || api.Value == config.GlApi;

    /// <summary>
    /// The earliest version in which each command or token is required by an <paramref name="api"/>
    /// feature block, ignoring profiles and removes - i.e. when it first appeared in that API.
    /// </summary>
    private static Dictionary<string, string> ScanApiAvailability(XElement registry, string api)
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

    private List<RegistryToken> CollectTokens(XElement registry, HashSet<string> names, Func<string, GlAvailability> availability)
    {
        var byName = new Dictionary<string, RegistryToken>();
        foreach (var block in registry.Elements("enums"))
        {
            var isBitmask = block.Attribute("type")?.Value == "bitmask";
            foreach (var element in block.Elements("enum"))
            {
                var name = element.Attribute("name")!.Value;
                if (!names.Contains(name) || config.SkipConstants.ContainsKey(name))
                    continue;
                if (element.Attribute("api") is { } api && api.Value != config.GlApi)
                    continue;

                var token = new RegistryToken(
                    name,
                    CSharpName.FromNativeIdentifier(name, config.Prefix, config.DigitNamePrefix, dimensionSegments: true),
                    ParseTokenValue(element.Attribute("value")!.Value),
                    element.Attribute("group")?.Value.Split(',') ?? [],
                    isBitmask,
                    availability(name));
                if (!byName.TryAdd(name, token) && byName[name].Value != token.Value)
                    throw new InvalidOperationException($"{name} is defined twice with different values.");
            }
        }

        var missing = names.Where(name => !byName.ContainsKey(name) && !config.SkipConstants.ContainsKey(name)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException($"Required tokens not defined in any <enums> block: {string.Join(", ", missing.Take(10))}");

        AssertUniqueManagedNames(byName.Values.Select(token => (token.ManagedName, token.NativeName)), "token");
        return [.. byName.Values];
    }

    private static ulong ParseTokenValue(string text) =>
        text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? ulong.Parse(text[2..], NumberStyles.HexNumber)
            : unchecked((ulong)long.Parse(text));

    /// <summary>
    /// Builds the typed enum groups from the group attributes of the selected tokens and resolves
    /// their managed names: typeRenames first, then a vestigial ARB suffix is dropped when that
    /// does not collide with another group, and finally the api-class prefix is applied so every
    /// enum type is namespaced (BufferTarget becomes GlBufferTarget) and cannot collide with
    /// unrelated framework types.
    /// </summary>
    private List<GlEnumGroup> BuildGroups(List<RegistryToken> tokens)
    {
        var membersByGroup = new Dictionary<string, List<RegistryToken>>();
        foreach (var token in tokens.Where(token => token.Value <= uint.MaxValue))
            foreach (var group in token.Groups)
            {
                if (!membersByGroup.TryGetValue(group, out var members))
                    membersByGroup[group] = members = [];
                members.Add(token);
            }

        foreach (var nativeName in membersByGroup.Keys)
        {
            if (!config.TypeRenames.TryGetValue(nativeName, out var managedName))
                managedName = nativeName.EndsWith("ARB") && nativeName.Length > 3 && !membersByGroup.ContainsKey(nativeName[..^3])
                    ? nativeName[..^3]
                    : nativeName;
            managedName = config.ApiClass + managedName;
            if (managedName == CatchAllName)
                throw new InvalidOperationException($"Group {nativeName} collides with the {CatchAllName} catch-all.");
            managedNameByGroup.Add(nativeName, managedName);
        }
        AssertUniqueManagedNames(managedNameByGroup.Select(pair => (pair.Value, pair.Key)), "enum group");

        return membersByGroup
            .Select(pair => new GlEnumGroup(
                pair.Key,
                managedNameByGroup[pair.Key],
                pair.Value.Any(token => token.IsBitmask),
                SortMembers(pair.Value)))
            .OrderBy(group => group.ManagedName, StringComparer.Ordinal)
            .ToList();
    }

    private List<GlEnumMember> SortMembers(IEnumerable<RegistryToken> tokens) => tokens
        .OrderBy(token => token.Value)
        .ThenBy(token => token.ManagedName, StringComparer.Ordinal)
        .Select(token => new GlEnumMember(token.ManagedName, token.NativeName, token.Value, token.Availability, GroupNames(token)))
        .ToList();

    /// <summary>The managed names of the typed enum groups a token belongs to, sorted for stable output.</summary>
    private List<string> GroupNames(RegistryToken token) => token.Groups
        .Where(managedNameByGroup.ContainsKey)
        .Select(group => managedNameByGroup[group])
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToList();

    private List<GlCommand> BuildCommands(XElement registry, HashSet<string> names, Func<string, GlAvailability> availability, IReadOnlyDictionary<string, XmlDocComment> docs, List<string> skipped)
    {
        var elementByName = registry.Elements("commands").Elements("command")
            .ToDictionary(element => element.Element("proto")!.Element("name")!.Value);

        var commands = new List<GlCommand>();
        foreach (var name in names.Order(StringComparer.Ordinal))
        {
            if (config.Skip.TryGetValue(name, out var reason))
            {
                skipped.Add($"{name}: {reason}");
                continue;
            }
            if (!elementByName.TryGetValue(name, out var element))
                throw new InvalidOperationException($"{name} is required by a feature but not defined in <commands>.");

            var proto = MapDeclaration(element.Element("proto")!, name);
            // A const char/byte pointer return is a NUL-terminated C string; the raw nint return is
            // kept and string/span convenience overloads are derived from this flag.
            var returnsCString = proto is { PointerDepth: 1, PointeeType: "byte", PointeeIsConst: true };
            var parameters = element.Elements("param").Select(param => MapParameter(param, name)).ToList();
            commands.Add(new(
                name,
                CSharpName.FromNativeIdentifier(name, "gl", config.DigitNamePrefix),
                proto.Type.Managed,
                proto.Type.Interop,
                parameters,
                availability(name),
                docs.GetValueOrDefault(name),
                returnsCString));
        }

        AssertUniqueManagedNames(commands.Select(command => (command.ManagedName, command.NativeName)), "command");
        return commands.OrderBy(command => command.ManagedName, StringComparer.Ordinal).ToList();
    }

    private GlParameter MapParameter(XElement param, string commandName)
    {
        var declaration = MapDeclaration(param, commandName);
        return new(
            declaration.Name,
            CSharpName.Parameter(declaration.Name),
            declaration.Type.Managed,
            declaration.Type.Interop,
            param.Attribute("len")?.Value,
            declaration.PointerDepth,
            declaration.PointeeType,
            declaration.PointeeIsConst,
            declaration.PointeeIsChar);
    }

    /// <summary>
    /// Maps a &lt;proto&gt; or &lt;param&gt; declaration: the C type is reconstructed from the
    /// text around the ptype and name elements, pointers become nint with the pointee recorded
    /// (group-typed for GLenum pointees), GLenum/GLbitfield and grouped GLint resolve their group
    /// attribute to a typed enum (catch-all when absent) and GLboolean becomes bool over a byte.
    /// </summary>
    private (string Name, (string Managed, string Interop) Type, int PointerDepth, string? PointeeType, bool PointeeIsConst, bool PointeeIsChar)
        MapDeclaration(XElement declaration, string commandName)
    {
        var type = new StringBuilder();
        var name = "";
        foreach (var node in declaration.Nodes())
        {
            if (node is XElement { Name.LocalName: "name" } nameElement)
                name = nameElement.Value;
            else if (node is XElement element)
                type.Append(element.Value);
            else if (node is XText text)
                type.Append(text.Value);
        }

        var cType = type.ToString();
        var pointerDepth = cType.Count(character => character == '*');
        var baseType = cType.Replace("const", "").Replace("struct", "").Replace("*", "").Trim();
        if (!ValueTypes.TryGetValue(baseType, out var valueType))
            throw new InvalidOperationException($"{commandName}: unmapped C type '{cType.Trim()}'.");
        var group = declaration.Attribute("group")?.Value;

        if (pointerDepth > 0)
        {
            var pointeeType = pointerDepth != 1 || valueType == "void" ? null
                : baseType == "GLenum" && group is not null && managedNameByGroup.TryGetValue(group, out var pointeeGroup) ? pointeeGroup
                : valueType;
            return (name, ("nint", "nint"), pointerDepth, pointeeType, cType.TrimStart().StartsWith("const "), baseType == "GLchar");
        }

        if (baseType is "GLenum" or "GLbitfield")
        {
            if (group is not null && managedNameByGroup.TryGetValue(group, out var managedGroup))
                return (name, (managedGroup, "uint"), 0, null, false, false);
            ungroupedEnumUses.Add($"{commandName}({(name.Length > 0 ? name : "return")}: {group ?? "no group"})");
            // An ungrouped GLenum still holds tokens; an ungrouped GLbitfield is arbitrary bits.
            return (name, (baseType == "GLenum" ? CatchAllName : "uint", "uint"), 0, null, false, false);
        }

        // The glTexImage family types its internalformat as GLint for historical reasons.
        if (baseType == "GLint" && group is not null && managedNameByGroup.TryGetValue(group, out var intGroup))
            return (name, (intGroup, "int"), 0, null, false, false);

        if (baseType == "GLboolean")
            return (name, ("bool", "byte"), 0, null, false, false);
        return (name, (valueType, valueType), 0, null, false, false);
    }

    private static void AssertUniqueManagedNames(IEnumerable<(string Managed, string Native)> names, string what)
    {
        var collisions = names.GroupBy(name => name.Managed).Where(group => group.Count() > 1).ToList();
        if (collisions.Count > 0)
            throw new InvalidOperationException(
                $"Colliding managed {what} names: {string.Join("; ", collisions.Take(5).Select(group => $"{group.Key} ({string.Join(", ", group.Select(name => name.Native))})"))}");
    }
}
