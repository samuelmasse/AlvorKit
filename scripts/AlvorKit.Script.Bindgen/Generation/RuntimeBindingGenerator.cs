namespace AlvorKit;

/// <summary>Generates and packages separate managed assemblies for native ABIs with different record layouts.</summary>
internal class RuntimeBindingGenerator(BindgenOptions options)
{
    /// <summary>Parses every configured ABI and writes runtime source sets and release verification inputs.</summary>
    public async Task GenerateAsync(NativeLibraryBinding library, string translationUnitPath, string? outputRoot)
    {
        var models = new Dictionary<string, BindingModel>();

        foreach (var runtime in library.Config.RuntimeIdentifiers)
        {
            var model = new CHeaderBindingParser(library.Config, library.Config.ApiClass).Parse(
                translationUnitPath, library.IncludeDirectory, library.SourceDirectory,
                library.Directory, BindingRuntimeTarget.Triple(runtime));
            models.Add(runtime, model);
            Console.WriteLine($"ABI {runtime}: {model.Functions.Count} functions, {model.Structs.Count} records");
            SkippedFunctionReporter.Print(model.SkippedFunctions);
        }

        RuntimeBindingContract.Validate(models);
        var emitter = new BindingCodeEmitter(library.Config, library.Tag);
        emitter.Emit(models.First().Value, library.RepositoryRoot, outputRoot, library.BindingVersion, library.NativeVersion);
        var layout = GeneratedOutput.ResolveProjectLayout(
            library.RepositoryRoot, outputRoot, library.Config.ApiProject, library.Config.BackendProject);

        foreach (var (runtime, model) in models)
            emitter.EmitRuntimeSources(model, layout, runtime);

        WriteProjectSettings(library, layout);
        new BindingAbiEmitter(library.Config).Emit(models, library.Directory, layout.Root, options.Strict);
        var host = library.HostRuntimeIdentifier;

        if (!models.TryGetValue(host, out var hostModel))
            throw new PlatformNotSupportedException($"No binding ABI configured for generator host {host}.");

        await new BindingExportVerifier(options).VerifyAsync(library, hostModel);
    }

    /// <summary>Isolates compiler inputs and outputs per ABI and refuses to pack an incomplete runtime matrix.</summary>
    private static void WriteProjectSettings(NativeLibraryBinding library, GeneratedProjectLayout layout)
    {
        var root = library.RepositoryRoot.Replace('\\', '/');
        var runtimeItems = string.Concat(library.Config.RuntimeIdentifiers.Select(runtime => TemplateResource.Render(
            typeof(RuntimeBindingGenerator), "res/templates/bindgen/c-headers/runtime-item.xml.tmpl",
            ("RepositoryRoot", root), ("Runtime", runtime))));
        var runtimeChecks = string.Concat(library.Config.RuntimeIdentifiers.Select(runtime => TemplateResource.Render(
            typeof(RuntimeBindingGenerator), "res/templates/bindgen/c-headers/runtime-check.xml.tmpl",
            ("RepositoryRoot", root), ("Runtime", runtime))));
        var properties = TemplateResource.Render(
            typeof(RuntimeBindingGenerator), "res/templates/bindgen/c-headers/runtime-directory.props.tmpl",
            ("RepositoryRoot", root), ("HostRuntimeIdentifier", library.HostRuntimeIdentifier),
            ("RuntimeItems", runtimeItems), ("RuntimeChecks", runtimeChecks));
        File.WriteAllText(Path.Combine(layout.ApiDirectory, "Directory.Build.props"), properties);
        File.WriteAllText(Path.Combine(layout.BackendDirectory, "Directory.Build.props"), properties);
    }
}
