namespace AlvorKit.Script.Bindgen;

/// <summary>Shared mutable state for one generated OpenGL extension source file.</summary>
/// <param name="Config">Bindgen configuration that controls generated API names and span hints.</param>
/// <param name="CommandNames">Generated command names selected by the registry.</param>
internal sealed class GlExtensionEmissionState(BindgenConfig Config, IReadOnlySet<string> CommandNames)
{
    /// <summary>Bindgen configuration that controls generated API names and span hints.</summary>
    public BindgenConfig Config { get; } = Config;

    /// <summary>Generated command names selected by the registry.</summary>
    public IReadOnlySet<string> CommandNames { get; } = CommandNames;

    /// <summary>Overload signatures already emitted, used to avoid duplicate generated members.</summary>
    private readonly HashSet<string> signatures = [];

    /// <summary>Parses a registry len expression for a command parameter.</summary>
    public GlExtensionLenInfo ParseLen(GlCommand command, GlParameter parameter) =>
        GlExtensionLenParser.Parse(command, parameter);

    /// <summary>Attempts to reserve a generated overload signature.</summary>
    public bool AddSignature(string signature) => signatures.Add(signature);
}
