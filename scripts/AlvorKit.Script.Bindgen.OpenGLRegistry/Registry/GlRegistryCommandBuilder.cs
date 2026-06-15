namespace AlvorKit.Script.Bindgen;

/// <summary>Builds selected OpenGL commands from registry command declarations.</summary>
/// <param name="config">Bindgen configuration used for naming and configured skips.</param>
/// <param name="catchAllName">Generated catch-all enum name for untyped GLenum positions.</param>
internal sealed class GlRegistryCommandBuilder(BindgenConfig config, string catchAllName)
{
    /// <summary>Declaration mapper shared by command prototype and parameter mapping.</summary>
    private readonly GlDeclarationMapper declarationMapper = new(catchAllName);

    /// <summary>Builds selected commands and returns side-channel metadata they require.</summary>
    public GlCommandBuildResult Build(
        XElement registry,
        HashSet<string> names,
        Func<string, GlAvailability> availability,
        IReadOnlyDictionary<string, XmlDocComment> docs,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        IReadOnlyDictionary<string, string> callbackManagedNames)
    {
        var elements = registry.Elements("commands").Elements("command")
            .ToDictionary(element => element.Element("proto")!.Element("name")!.Value);
        var build = NewBuildState();

        foreach (var name in names.Order(StringComparer.Ordinal))
            AddCommand(name, elements, availability, docs, managedNameByGroup, callbackManagedNames, build);

        GlManagedNameGuard.AssertUnique(build.Commands.Select(command => (command.ManagedName, command.NativeName)), "command");
        return new(
            build.Commands.OrderBy(command => command.ManagedName, StringComparer.Ordinal).ToList(),
            build.UngroupedEnumUses,
            build.SkippedCommands,
            [.. build.HandleTypes],
            build.UsedCallbacks);
    }

    /// <summary>Creates mutable collections used only while building commands.</summary>
    private static GlRegistryCommandBuildState NewBuildState() =>
        new([], [], [], new(StringComparer.Ordinal), new(StringComparer.Ordinal));

    /// <summary>Adds one selected command or configured skip to the build state.</summary>
    private void AddCommand(
        string name,
        IReadOnlyDictionary<string, XElement> elements,
        Func<string, GlAvailability> availability,
        IReadOnlyDictionary<string, XmlDocComment> docs,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        IReadOnlyDictionary<string, string> callbackManagedNames,
        GlRegistryCommandBuildState build)
    {
        if (config.Skip.TryGetValue(name, out var reason))
        {
            build.SkippedCommands.Add($"{name}: {reason}");
            return;
        }
        if (!elements.TryGetValue(name, out var element))
            throw new InvalidOperationException($"{name} is required by a feature but not defined in <commands>.");

        var proto = MapDeclaration(element.Element("proto")!, name, managedNameByGroup, callbackManagedNames, build);
        var objectCommand = element.Elements("param").Any(param => param.Attribute("group")?.Value == "ObjectIdentifier");
        var parameters = element.Elements("param")
            .Select(param => MapParameter(param, name, objectCommand, managedNameByGroup, callbackManagedNames, build))
            .ToList();
        build.Commands.Add(new(
            name,
            CSharpName.FromNativeIdentifier(name, "gl", config.DigitNamePrefix),
            proto.Type.Managed,
            proto.Type.Interop,
            parameters,
            availability(name),
            docs.GetValueOrDefault(name),
            proto is { PointerDepth: 1, PointeeType: "byte", PointeeIsConst: true }));
    }

    /// <summary>Maps one command parameter into its managed model shape.</summary>
    private GlParameter MapParameter(
        XElement param,
        string commandName,
        bool objectCommand,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        IReadOnlyDictionary<string, string> callbackManagedNames,
        GlRegistryCommandBuildState build)
    {
        var declaration = MapDeclaration(param, commandName, managedNameByGroup, callbackManagedNames, build, objectCommand);
        return new(
            declaration.Name,
            CSharpName.Parameter(declaration.Name),
            declaration.Type.Managed,
            declaration.Type.Interop,
            param.Attribute("len")?.Value,
            declaration.PointerDepth,
            declaration.PointeeType,
            declaration.PointeeIsConst,
            declaration.PointeeIsChar,
            declaration.CallbackType);
    }

    /// <summary>Maps one declaration while passing command build diagnostics through.</summary>
    private GlDeclarationShape MapDeclaration(
        XElement declaration,
        string commandName,
        IReadOnlyDictionary<string, string> managedNameByGroup,
        IReadOnlyDictionary<string, string> callbackManagedNames,
        GlRegistryCommandBuildState build,
        bool objectCommand = false) =>
        declarationMapper.Map(
            declaration,
            commandName,
            managedNameByGroup,
            callbackManagedNames,
            build.HandleTypes,
            build.UngroupedEnumUses,
            build.UsedCallbacks,
            objectCommand);
}
