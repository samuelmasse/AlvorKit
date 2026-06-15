namespace AlvorKit.Script.Bindgen;

/// <summary>Runs the OpenGL registry binding pipeline for one configured native library.</summary>
internal sealed class GlRegistryBindingGenerator
{
    /// <summary>Downloads or locates the OpenGL registry and optional reference-page sources.</summary>
    private readonly NativeSourceResolver sourceResolver = new();

    /// <summary>Generates bindings from an OpenGL registry configuration.</summary>
    public async Task GenerateAsync(NativeLibraryBinding library, string? outputRoot)
    {
        await sourceResolver.EnsureSourceAsync(library);
        await sourceResolver.EnsureDocSourceAsync(library);
        var config = library.Config;
        Console.WriteLine(
            $"Parsing {config.Header} ({config.GlApi} {config.GlVersion} {config.GlProfile}, " +
            $"registry {library.Tag[..Math.Min(12, library.Tag.Length)]})");

        var docs = library.DocReadDirectory is { } docDirectory && Directory.Exists(docDirectory)
            ? new GlDocParser().Parse(docDirectory)
            : [];
        var model = new GlRegistryParser(config).Parse(library.HeaderPath, docs);
        Console.WriteLine(
            $"Model: {model.Commands.Count} commands, {model.Groups.Count} enum groups " +
            $"({model.Groups.Sum(group => group.Members.Count)} members), {model.AllTokens.Members.Count} tokens, " +
            $"{model.WideConstants.Count} wide constants, {model.SkippedCommands.Count} skipped");

        if (model.UngroupedEnumUses.Count > 0)
            Console.WriteLine(
                $"Ungrouped enum uses (typed as {config.ApiClass}Enum): {model.UngroupedEnumUses.Count} - " +
                string.Join(", ", model.UngroupedEnumUses.Take(8)) +
                (model.UngroupedEnumUses.Count > 8 ? ", ..." : ""));

        var documentedCommands = model.Commands.Count(command => command.Documentation is not null);
        Console.WriteLine(
            $"Docs: {docs.Count} reference pages indexed, {documentedCommands} of " +
            $"{model.Commands.Count} commands documented");

        new GlCodeEmitter(config, library.Tag, library.DocTag).Emit(
            model,
            library.RepositoryRoot,
            outputRoot,
            library.BindingVersion);
        Console.WriteLine($"Emitted {config.ApiProject} and {config.BackendProject} ({library.BindingVersion})");
        SkippedFunctionReporter.Print(model.SkippedCommands);
        Console.WriteLine();
    }
}
