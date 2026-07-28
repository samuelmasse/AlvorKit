namespace AlvorKit.Script.LiveCode;

/// <summary>Compiles one source submission against references reported by its exact running target.</summary>
internal sealed class LiveCodeCompiler
{
    private readonly CSharpParseOptions parseOptions = new(LanguageVersion.Preview);

    internal LiveCodeCompilation Compile(string source, LiveCodeReferenceManifest manifest)
        => Compile(source, manifest, FindCommandEntryType);

    /// <summary>Compiles one class containing exactly one exact LivePatch handler.</summary>
    internal LiveCodeCompilation CompilePatch(
        string source,
        LiveCodeReferenceManifest manifest) =>
        Compile(source, manifest, FindPatchEntryType);

    private LiveCodeCompilation Compile(
        string source,
        LiveCodeReferenceManifest manifest,
        Func<CSharpCompilation, SyntaxTree, string> findEntryType)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            SourceText.From(source, Encoding.UTF8),
            parseOptions,
            "LiveCodeSubmission.cs");
        var trees = new List<SyntaxTree> { sourceTree };
        if (manifest.GlobalUsings.Length > 0)
        {
            var imports = string.Join(
                Environment.NewLine,
                manifest.GlobalUsings.Select(static x => $"global using {x};"));
            trees.Add(CSharpSyntaxTree.ParseText(
                SourceText.From(imports, Encoding.UTF8),
                parseOptions,
                "LiveCodeGlobalUsings.g.cs"));
        }

        var references = manifest.AssemblyPaths
            .Select(static path => MetadataReference.CreateFromFile(path))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "AlvorKit.LiveCode.Submission." + Guid.NewGuid().ToString("N"),
            trees,
            references,
            new(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                allowUnsafe: true,
                nullableContextOptions: NullableContextOptions.Enable));
        var entryType = findEntryType(compilation, sourceTree);

        using var assembly = new MemoryStream();
        using var symbols = new MemoryStream();
        var result = compilation.Emit(
            assembly,
            symbols,
            options: new(debugInformationFormat: DebugInformationFormat.PortablePdb));
        if (!result.Success)
        {
            var diagnostics = result.Diagnostics
                .Where(static x => x.Severity == DiagnosticSeverity.Error)
                .Select(static x => x.ToString());
            throw new LiveCodeCompilationException(string.Join(Environment.NewLine, diagnostics));
        }

        return new(assembly.ToArray(), symbols.ToArray(), entryType);
    }

    private static string FindCommandEntryType(
        CSharpCompilation compilation,
        SyntaxTree sourceTree)
    {
        var contract = compilation.GetTypeByMetadataName(typeof(ILiveCodeCommand).FullName!)
            ?? throw new LiveCodeCompilationException($"Compilation could not resolve {nameof(ILiveCodeCommand)}.");
        var model = compilation.GetSemanticModel(sourceTree);
        var commands = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Select(x => model.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Where(x => x.TypeKind == TypeKind.Class
                && x.ContainingType is null
                && x.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, contract)))
            .ToArray();

        if (commands.Length != 1)
        {
            throw new LiveCodeCompilationException(
                $"Source must declare exactly one top-level class implementing {nameof(ILiveCodeCommand)}; found {commands.Length}.");
        }

        var command = commands[0];
        return command.ContainingNamespace.IsGlobalNamespace
            ? command.MetadataName
            : command.ContainingNamespace.ToDisplayString() + "." + command.MetadataName;
    }

    private static string FindPatchEntryType(
        CSharpCompilation compilation,
        SyntaxTree sourceTree)
    {
        var contract = compilation.GetTypeByMetadataName(
            typeof(LivePatchHandlerAttribute).FullName!)
            ?? throw new LiveCodeCompilationException(
                $"Compilation could not resolve {nameof(LivePatchHandlerAttribute)}.");
        var model = compilation.GetSemanticModel(sourceTree);
        var methods = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(x => model.GetDeclaredSymbol(x))
            .OfType<IMethodSymbol>()
            .Where(x => x.GetAttributes().Any(attribute =>
                SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    contract)))
            .ToArray();
        if (methods.Length != 1)
        {
            throw new LiveCodeCompilationException(
                $"Source must declare exactly one [{nameof(LivePatchHandlerAttribute)}] method; found {methods.Length}.");
        }

        var type = methods[0].ContainingType;
        if (type.TypeKind != TypeKind.Class ||
            type.IsAbstract ||
            type.ContainingType is not null)
        {
            throw new LiveCodeCompilationException(
                $"The [{nameof(LivePatchHandlerAttribute)}] method must belong to one concrete top-level class.");
        }

        return type.ContainingNamespace.IsGlobalNamespace
            ? type.MetadataName
            : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
    }
}
