namespace AlvorKit.Script.Bindgen;

/// <summary>Coordinates binding generation for the selected native library metadata.</summary>
/// <param name="repository">Repository layout used to discover configured native libraries.</param>
/// <param name="options">Command-line options that control selection and strict validation.</param>
[ExcludeFromCodeCoverage]
public sealed class BindingGenerationRunner(RepositoryLayout repository, BindgenOptions options)
{
    /// <summary>Optional output root used for generated project snapshots.</summary>
    private readonly string? outputRoot = repository.ResolveGeneratedOutputRoot(options.OutputRoot);

    /// <summary>Generator for C header-backed bindings.</summary>
    private readonly CHeaderBindingGenerator cHeaderGenerator = new(options);

    /// <summary>Generator for registry-backed OpenGL bindings.</summary>
    private readonly GlRegistryBindingGenerator glRegistryGenerator = new();

    /// <summary>Generates bindings for every repository library selected by the parsed options.</summary>
    public async Task RunAsync()
    {
        foreach (var libraryName in repository.SelectedLibraries(options.Selection))
            await GenerateLibraryAsync(NativeLibraryBinding.Load(repository, libraryName));
    }

    /// <summary>Routes a loaded native library to the generation path declared by its bindgen config.</summary>
    private async Task GenerateLibraryAsync(NativeLibraryBinding library)
    {
        if (library.Config.Kind == BindgenConfig.GlRegistryKind)
        {
            await glRegistryGenerator.GenerateAsync(library, outputRoot);
            return;
        }

        await cHeaderGenerator.GenerateAsync(library, outputRoot);
    }
}
