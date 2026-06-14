namespace AlvorKit.Script.Bindgen;

public sealed class BindingGenerationRunner(
    RepositoryLayout repository,
    BindgenOptions options,
    IEnumerable<INativeLibrarySpec> librarySpecs)
{
    private static readonly string PrimaryTarget = "x86_64-pc-windows-msvc";
    private static readonly string[] AdditionalLayoutTargets =
    [
        "i686-pc-windows-msvc",
        "armv7-unknown-linux-gnueabihf",
        "aarch64-unknown-linux-gnu"
    ];

    private readonly NativeSourceResolver sourceResolver = new();
    private readonly TranslationUnitWriter translationUnitWriter = new();

    public async Task RunAsync()
    {
        foreach (var spec in SelectedSpecs())
            await GenerateLibraryAsync(NativeLibraryBinding.Load(repository, spec));
    }

    private IEnumerable<INativeLibrarySpec> SelectedSpecs()
    {
        var specsByName = librarySpecs.ToDictionary(spec => spec.Name, StringComparer.OrdinalIgnoreCase);
        if (options.Selection == "all")
            return specsByName.Values.OrderBy(spec => spec.Name, StringComparer.OrdinalIgnoreCase);
        if (specsByName.TryGetValue(options.Selection, out var selected))
            return [selected];
        throw new InvalidOperationException($"Unknown bindgen library '{options.Selection}'. Known libraries: {string.Join(", ", specsByName.Keys.Order(StringComparer.OrdinalIgnoreCase))}");
    }

    private async Task GenerateLibraryAsync(NativeLibraryBinding library)
    {
        if (library.Config.Kind == BindgenConfig.GlRegistryKind)
        {
            await GenerateGlRegistryLibraryAsync(library);
            return;
        }

        await sourceResolver.EnsureSourceAsync(library);
        var translationUnitPath = translationUnitWriter.Write(library);
        EnsureShimFileExists(library);

        Console.WriteLine($"Parsing {library.Config.ImplFile ?? "tu"} + {library.Config.Header} ({library.Config.NativeLibrary} {library.Tag})");
        var model = ParsePrimaryModel(library, translationUnitPath);
        model = UpdateSizeofShimIfNeeded(library, translationUnitPath, model);

        Console.WriteLine($"Model: {model.Functions.Count} functions, {model.Enums.Count} enums, {model.Structs.Count} structs, {model.SkippedFunctions.Count} skipped");
        ValidateLayouts(library, translationUnitPath, model);

        new BindingCodeEmitter(library.Config, library.Tag).Emit(model, library.RepositoryRoot, library.Version);
        Console.WriteLine($"Emitted {library.Config.ApiProject} and {library.Config.BackendProject} ({library.Version})");

        VerifyExports(library, model);
        PrintSkippedFunctions(model.SkippedFunctions);
        Console.WriteLine();
    }

    /// <summary>
    /// Generates registry-backed bindings. OpenGL loads from the platform driver at runtime, so this
    /// path has no C compilation, shim, layout validation, or native export verification stage.
    /// </summary>
    private async Task GenerateGlRegistryLibraryAsync(NativeLibraryBinding library)
    {
        await sourceResolver.EnsureSourceAsync(library);
        await sourceResolver.EnsureDocSourceAsync(library);
        var config = library.Config;
        Console.WriteLine($"Parsing {config.Header} ({config.GlApi} {config.GlVersion} {config.GlProfile}, registry {library.Tag[..Math.Min(12, library.Tag.Length)]})");

        var docs = library.DocReadDirectory is { } docDirectory && Directory.Exists(docDirectory)
            ? new GlDocParser().Parse(docDirectory)
            : [];
        var model = new GlRegistryParser(config).Parse(library.HeaderPath, docs);
        Console.WriteLine($"Model: {model.Commands.Count} commands, {model.Groups.Count} enum groups " +
            $"({model.Groups.Sum(group => group.Members.Count)} members), {model.AllTokens.Members.Count} tokens, " +
            $"{model.WideConstants.Count} wide constants, {model.SkippedCommands.Count} skipped");
        if (model.UngroupedEnumUses.Count > 0)
            Console.WriteLine($"Ungrouped enum uses (typed as {config.ApiClass}Enum): {model.UngroupedEnumUses.Count} - " +
                string.Join(", ", model.UngroupedEnumUses.Take(8)) + (model.UngroupedEnumUses.Count > 8 ? ", ..." : ""));
        Console.WriteLine($"Docs: {docs.Count} reference pages indexed, {model.Commands.Count(command => command.Documentation is not null)} of {model.Commands.Count} commands documented");

        new GlCodeEmitter(config, library.Tag, library.DocTag).Emit(model, library.RepositoryRoot, library.Version);
        Console.WriteLine($"Emitted {config.ApiProject} and {config.BackendProject} ({library.Version})");
        PrintSkippedFunctions(model.SkippedCommands);
        Console.WriteLine();
    }

    private static BindingModel ParsePrimaryModel(NativeLibraryBinding library, string translationUnitPath) =>
        new CHeaderBindingParser(library.Config, library.Config.ApiClass).Parse(
            translationUnitPath,
            library.IncludeDirectory,
            library.SourceDirectory,
            library.Directory,
            PrimaryTarget);

    private static void EnsureShimFileExists(NativeLibraryBinding library)
    {
        if (library.SizeofShimPath is not null && !File.Exists(library.SizeofShimPath))
            File.WriteAllText(library.SizeofShimPath, "// <auto-generated/>" + Environment.NewLine);
    }

    private BindingModel UpdateSizeofShimIfNeeded(NativeLibraryBinding library, string translationUnitPath, BindingModel model)
    {
        if (library.SizeofShimPath is null)
            return model;

        var shim = new BindingCodeEmitter(library.Config, library.Tag).EmitSizeofShim(model);
        if (File.ReadAllText(library.SizeofShimPath) == shim)
            return model;

        if (options.Strict)
            throw new InvalidOperationException($"{library.Config.SizeofShim} is out of date - regenerate locally, rebuild the native library and bump REVISION.");

        File.WriteAllText(library.SizeofShimPath, shim);
        Console.WriteLine($"Shim: {library.Config.SizeofShim} updated with {model.SizeofTypes.Count} sizeof exports - rebuild the native library and bump REVISION");
        return ParsePrimaryModel(library, translationUnitPath);
    }

    private static void ValidateLayouts(NativeLibraryBinding library, string translationUnitPath, BindingModel model)
    {
        var nativeStructNames = model.Structs.Select(structType => structType.NativeName).ToList();
        foreach (var target in AdditionalLayoutTargets)
        {
            CHeaderBindingParser.ValidateNaturalLayout(
                library.Config,
                translationUnitPath,
                library.IncludeDirectory,
                library.SourceDirectory,
                library.Directory,
                target,
                nativeStructNames);
        }
        Console.WriteLine($"Layout: natural on all {AdditionalLayoutTargets.Length + 1} validated targets");
    }

    private void VerifyExports(NativeLibraryBinding library, BindingModel model)
    {
        var verification = NativeExportVerifier.Verify(library.HostNativeLibraryPath, model);
        if (!verification.LibraryExists)
        {
            if (options.Strict)
                throw new FileNotFoundException($"strict mode requires the native library for export verification: {verification.LibraryPath}");
            Console.WriteLine($"Exports: skipped ({verification.LibraryPath} not built locally)");
            return;
        }

        if (verification.MissingFunctions.Count > 0 && options.Strict)
            throw new InvalidOperationException($"{verification.MissingFunctions.Count} entry points missing from {Path.GetFileName(verification.LibraryPath)}: {MissingFunctionList(verification)}");

        Console.WriteLine(verification.MissingFunctions.Count == 0
            ? $"Exports: all {model.Functions.Count} entry points found in {Path.GetFileName(verification.LibraryPath)}"
            : $"WARNING: {verification.MissingFunctions.Count} entry points missing from {Path.GetFileName(verification.LibraryPath)}: {MissingFunctionList(verification)}");
    }

    private static string MissingFunctionList(NativeExportVerification verification) =>
        string.Join(", ", verification.MissingFunctions.Select(function => function.NativeName).Take(10));

    private static void PrintSkippedFunctions(List<string> skippedFunctions)
    {
        if (skippedFunctions.Count == 0)
            return;

        Console.WriteLine("Skipped functions:");
        foreach (var skipped in skippedFunctions)
            Console.WriteLine($"  {skipped}");
    }
}
