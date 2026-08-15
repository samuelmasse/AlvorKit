namespace AlvorKit;

/// <summary>Owns the exact Roslyn project, PE/PDB baseline, and chained acknowledged generations.</summary>
internal sealed class SourceUpdateProjectBaseline : IDisposable
{
    private static readonly Lock RegistrationGate = new();
    private readonly MSBuildWorkspace workspace;
    private readonly ProjectId projectId;
    private readonly ModuleMetadata moduleMetadata;
    private readonly PEReader peReader;
    private readonly Dictionary<string, string> sources;
    private Solution solution;
    private Compilation compilation;
    private EmitBaseline baseline;
    private int generation;

    private SourceUpdateProjectBaseline(
        SourceUpdateCompilerLaunch launch,
        MSBuildWorkspace workspace,
        Project project,
        Compilation compilation,
        ModuleMetadata moduleMetadata,
        PEReader peReader,
        EmitBaseline baseline,
        Dictionary<string, string> sources)
    {
        Launch = launch;
        this.workspace = workspace;
        projectId = project.Id;
        solution = project.Solution;
        this.compilation = compilation;
        this.moduleMetadata = moduleMetadata;
        this.peReader = peReader;
        this.baseline = baseline;
        this.sources = sources;
    }

    internal SourceUpdateCompilerLaunch Launch { get; }

    internal int Generation => generation;

    internal static async Task<SourceUpdateProjectBaseline> Create(
        SourceUpdateCompilerLaunch launch,
        CancellationToken cancellationToken)
    {
        RegisterMsBuild();
        var diagnostics = new List<string>();
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Configuration"] = "Debug",
            ["Optimize"] = "false",
            ["DebugSymbols"] = "true",
            ["DebugType"] = "portable",
            ["PublishTrimmed"] = "false",
            ["PublishReadyToRun"] = "false",
            ["PublishSingleFile"] = "false"
        };
        var workspace = MSBuildWorkspace.Create(properties);
        workspace.LoadMetadataForReferencedProjects = false;
        using var workspaceFailure = workspace.RegisterWorkspaceFailedHandler(
            args => diagnostics.Add(args.Diagnostic.Message));
        try
        {
            var project = await workspace.OpenProjectAsync(
                Path.GetFullPath(launch.ProjectPath),
                cancellationToken: cancellationToken);
            if (diagnostics.Count > 0)
            {
                throw new InvalidOperationException(
                    $"MSBuildWorkspace reported: {string.Join(" | ", diagnostics)}");
            }

            var compilation = await project.GetCompilationAsync(cancellationToken)
                ?? throw new InvalidOperationException("The editable project produced no compilation.");
            RequireNoErrors(compilation, cancellationToken);
            VerifyLaunchIdentity(compilation, launch);

            var image = ImmutableArray.Create(File.ReadAllBytes(launch.AssemblyPath));
            var moduleMetadata = ModuleMetadata.CreateFromImage(image);
            var peStream = File.OpenRead(launch.AssemblyPath);
            var peReader = new PEReader(peStream);
            var baseline = EmitBaseline.CreateInitialBaseline(
                compilation,
                moduleMetadata,
                static _ => default,
                handle => LocalSignature(peReader, handle),
                hasPortableDebugInformation: true);
            var sources = await ReadSources(project, cancellationToken);
            return new(
                launch,
                workspace,
                project,
                compilation,
                moduleMetadata,
                peReader,
                baseline,
                sources);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal async Task<SourceUpdateDeltaProposal> Prepare(
        string sourcePath,
        string diff,
        string currentSource,
        string repositoryRoot,
        string updateId,
        CancellationToken cancellationToken)
    {
        sourcePath = Path.GetFullPath(sourcePath);
        if (!sources.TryGetValue(sourcePath, out var previousSource))
            throw new InvalidOperationException($"Source file is not part of the editable project: {sourcePath}");
        var diffResult = SourceUpdateDiff.Apply(
            previousSource,
            diff,
            sourcePath,
            repositoryRoot);
        if (diffResult.Source != currentSource)
            throw new InvalidOperationException("The numbered diff result does not exactly match the current source file.");

        var oldDocument = Document(sourcePath);
        var encoding = (await oldDocument.GetTextAsync(cancellationToken)).Encoding
            ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var nextSolution = solution.WithDocumentText(
            oldDocument.Id,
            SourceText.From(currentSource, encoding, SourceHashAlgorithm.Sha256));
        var newDocument = nextSolution.GetDocument(oldDocument.Id)
            ?? throw new InvalidOperationException("Updated source document disappeared from the solution.");
        var newCompilation = await nextSolution.GetProject(projectId)!.GetCompilationAsync(cancellationToken)
            ?? throw new InvalidOperationException("Updated project produced no compilation.");
        RequireNoErrors(newCompilation, cancellationToken);

        var oldModel = await oldDocument.GetSemanticModelAsync(cancellationToken)
            ?? throw new InvalidOperationException("Old source document produced no semantic model.");
        var newModel = await newDocument.GetSemanticModelAsync(cancellationToken)
            ?? throw new InvalidOperationException("Updated source document produced no semantic model.");
        var edit = SourceUpdateEditValidator.Validate(
            previousSource,
            currentSource,
            oldModel,
            newModel,
            cancellationToken);
        return Emit(
            sourcePath,
            currentSource,
            nextSolution,
            newCompilation,
            edit,
            diffResult,
            updateId,
            cancellationToken);
    }

    internal void Commit(SourceUpdateDeltaProposal proposal)
    {
        if (proposal.PreviousGeneration != generation)
            throw new InvalidOperationException("Compiler proposal no longer matches the current generation.");

        solution = proposal.Solution;
        compilation = proposal.Compilation;
        baseline = proposal.Baseline;
        sources[proposal.SourcePath] = proposal.ResultSource;
        generation++;
    }

    public void Dispose()
    {
        peReader.Dispose();
        moduleMetadata.Dispose();
        workspace.Dispose();
    }

    private SourceUpdateDeltaProposal Emit(
        string sourcePath,
        string currentSource,
        Solution nextSolution,
        Compilation newCompilation,
        SourceUpdateValidatedEdit edit,
        SourceUpdateDiffResult diff,
        string updateId,
        CancellationToken cancellationToken)
    {
        using var metadata = new MemoryStream();
        using var il = new MemoryStream();
        using var pdb = new MemoryStream();
        var semanticEdit = new SemanticEdit(
            SemanticEditKind.Update,
            edit.OldSymbol,
            edit.NewSymbol,
            syntaxMap: null,
            runtimeRudeEdit: null,
            instrumentation: default);
        var result = newCompilation.EmitDifference(
            baseline,
            [semanticEdit],
            static _ => false,
            metadata,
            il,
            pdb,
            cancellationToken);
        var diagnostics = result.Diagnostics.Select(static item => item.ToString()).ToArray();
        if (!result.Success)
            throw new InvalidOperationException($"Source Update emit failed: {string.Join(" | ", diagnostics)}");
        var nextBaseline = result.Baseline
            ?? throw new InvalidOperationException("Source Update emit returned no chained baseline.");

        var metadataBytes = metadata.ToArray();
        var ilBytes = il.ToArray();
        var pdbBytes = pdb.ToArray();
        var tokens = SourceUpdateDeltaValidator.Validate(
            result,
            metadataBytes);
        var request = new SourceUpdateApplyRequest(
            Launch.ModuleMvid,
            generation,
            updateId,
            diff.PreviousSourceSha256,
            diff.ResultSourceSha256,
            tokens.MethodToken,
            tokens.ChangedTypeTokens,
            metadataBytes,
            ilBytes,
            pdbBytes,
            Hash(metadataBytes),
            Hash(ilBytes),
            Hash(pdbBytes),
            Launch.ProjectIdentityHash);
        return new(
            generation,
            sourcePath,
            currentSource,
            nextSolution,
            newCompilation,
            nextBaseline,
            request,
            diff.DiffSha256,
            diagnostics);
    }

    private Microsoft.CodeAnalysis.Document Document(string sourcePath) =>
        solution.GetProject(projectId)!.Documents.Single(document =>
            string.Equals(
                Path.GetFullPath(document.FilePath!),
                sourcePath,
                StringComparison.OrdinalIgnoreCase));

    private static void RegisterMsBuild()
    {
        lock (RegistrationGate)
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
        }
    }

    private static void VerifyLaunchIdentity(
        Compilation compilation,
        SourceUpdateCompilerLaunch launch)
    {
        if (!string.Equals(
                compilation.AssemblyName,
                Path.GetFileNameWithoutExtension(launch.AssemblyPath),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The loaded project assembly identity does not match the editable launch assembly.");
        }
        if (!File.Exists(launch.AssemblyPath) || !File.Exists(launch.PdbPath))
            throw new InvalidOperationException("The editable launch PE/PDB pair no longer exists.");
        if (!string.Equals(HashFile(launch.AssemblyPath), launch.AssemblySha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(HashFile(launch.PdbPath), launch.PdbSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The editable launch PE/PDB pair changed after launch.");
        }

        using var stream = File.OpenRead(launch.AssemblyPath);
        using var reader = new PEReader(stream);
        var metadata = reader.GetMetadataReader();
        var mvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString("N");
        if (!string.Equals(mvid, launch.ModuleMvid, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The editable launch PE MVID no longer matches its immutable manifest.");
        }

        var codeView = reader.ReadDebugDirectory()
            .SingleOrDefault(static entry => entry.Type == DebugDirectoryEntryType.CodeView);
        if (codeView.DataSize == 0 ||
            !string.Equals(
                reader.ReadCodeViewDebugDirectoryData(codeView).Path,
                launch.CodeViewPath,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The editable launch PE CodeView identity no longer matches its immutable manifest.");
        }
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static StandaloneSignatureHandle LocalSignature(
        PEReader reader,
        MethodDefinitionHandle method)
    {
        var definition = reader.GetMetadataReader().GetMethodDefinition(method);
        return definition.RelativeVirtualAddress == 0
            ? default
            : reader.GetMethodBody(definition.RelativeVirtualAddress).LocalSignature;
    }

    private static async Task<Dictionary<string, string>> ReadSources(
        Project project,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var document in project.Documents)
        {
            if (document.FilePath is null)
                continue;
            result.Add(
                Path.GetFullPath(document.FilePath),
                (await document.GetTextAsync(cancellationToken)).ToString());
        }
        return result;
    }

    private static void RequireNoErrors(
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var errors = compilation.GetDiagnostics(cancellationToken)
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException($"Editable project has compiler errors: {string.Join(" | ", errors)}");
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
