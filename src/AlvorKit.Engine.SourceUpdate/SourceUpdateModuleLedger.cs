namespace AlvorKit;

/// <summary>Validates and advances source generations for an exact allowlisted loaded module.</summary>
public sealed class SourceUpdateModuleLedger
{
    private const int TypeDefinitionTokenPrefix = 0x02000000;
    private const int MethodDefinitionTokenPrefix = 0x06000000;
    private readonly Lock gate = new();
    private readonly int maximumDeltaBytes;
    private readonly ISourceUpdateRuntime runtime;
    private readonly Func<Type[], string[]> notifyHandlers;
    private readonly SourceUpdateModuleState module;

    internal SourceUpdateModuleLedger(
        SourceUpdateHostOptions options,
        ISourceUpdateRuntime? runtime = null,
        Func<Type[], string[]>? notifyHandlers = null,
        bool validateProcessMode = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.MaximumDeltaBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum Source Update delta bytes must be positive.");
        if (validateProcessMode)
            ValidateProcessMode();

        maximumDeltaBytes = options.MaximumDeltaBytes;
        this.runtime = runtime ?? new SourceUpdateRuntime();
        this.notifyHandlers = notifyHandlers ?? SourceUpdateMetadataHandlers.Notify;
        module = CreateModule(options);
    }

    /// <summary>Returns current runtime support and the exact allowlisted module generation.</summary>
    public SourceUpdateCapabilities Capabilities()
    {
        lock (gate)
        {
            return new(
                1,
                runtime.IsSupported,
                Environment.Version.ToString(),
                Environment.ProcessId,
                "source-update",
                false,
                "existing-method-body",
                maximumDeltaBytes,
                module.Identity.RestartRequired,
                [module.Identity]);
        }
    }

    /// <summary>Applies one validated forward generation or rejects it without mutating the module.</summary>
    public SourceUpdateApplyResult Apply(SourceUpdateApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (gate)
        {
            if (module.Results.TryGetValue(request.UpdateId, out var previous))
                return previous;
            if (Validate(request) is { } rejection)
                return Rejected(request, rejection);

            try
            {
                runtime.ApplyUpdate(
                    module.Assembly,
                    request.MetadataDelta,
                    request.IlDelta,
                    request.PdbDelta);
            }
            catch (Exception exception)
            {
                module.Identity = module.Identity with { RestartRequired = true };
                var ambiguous = new SourceUpdateApplyResult(
                    SourceUpdateApplyStatus.RestartRequired,
                    request.UpdateId,
                    module.Identity.ModuleMvid,
                    module.Identity.Generation,
                    module.Identity.SourceHash ?? request.PreviousSourceHash,
                    request.MetadataDeltaHash,
                    request.IlDeltaHash,
                    request.PdbDeltaHash,
                    [],
                    true,
                    $"Metadata update failed and the process state is ambiguous: {exception.Message}");
                module.Results.Add(request.UpdateId, ambiguous);
                return ambiguous;
            }

            module.Identity = module.Identity with
            {
                Generation = module.Identity.Generation + 1,
                SourceHash = request.ResultSourceHash
            };
            var warnings = NotifyHandlers(request.ChangedTypeTokens);
            if (warnings.Length > 0)
                module.Identity = module.Identity with { RestartRequired = true };

            var applied = new SourceUpdateApplyResult(
                warnings.Length == 0
                    ? SourceUpdateApplyStatus.Applied
                    : SourceUpdateApplyStatus.AppliedWithHandlerWarnings,
                request.UpdateId,
                module.Identity.ModuleMvid,
                module.Identity.Generation,
                request.ResultSourceHash,
                request.MetadataDeltaHash,
                request.IlDeltaHash,
                request.PdbDeltaHash,
                warnings,
                module.Identity.RestartRequired,
                null);
            module.Results.Add(request.UpdateId, applied);
            return applied;
        }
    }

    private string? Validate(SourceUpdateApplyRequest request)
    {
        if (!runtime.IsSupported)
            return "Metadata updates are not supported by this process.";
        if (module.Identity.RestartRequired)
            return "The Source Update module is restart-required.";
        if (!string.Equals(request.ModuleMvid, module.Identity.ModuleMvid, StringComparison.OrdinalIgnoreCase))
            return "The requested module MVID is not allowlisted by this editable launch.";
        if (request.ExpectedGeneration != module.Identity.Generation)
            return $"Expected generation {request.ExpectedGeneration}, but the target is at {module.Identity.Generation}.";
        if (module.Identity.SourceHash is { } sourceHash &&
            !string.Equals(request.PreviousSourceHash, sourceHash, StringComparison.OrdinalIgnoreCase))
        {
            return "The previous source hash does not match the acknowledged runtime generation.";
        }
        if (!string.Equals(request.ProjectIdentityHash, module.Identity.ProjectIdentityHash, StringComparison.OrdinalIgnoreCase))
            return "The project/build identity does not match the editable launch.";
        if (string.IsNullOrWhiteSpace(request.UpdateId) || request.UpdateId.Length > 128)
            return "Update id must contain 1 to 128 characters.";
        if ((request.ExpectedMethodToken & unchecked((int)0xFF000000)) != MethodDefinitionTokenPrefix)
            return "Expected method token is not a MethodDef.";
        if (request.ChangedTypeTokens.Length != 1 ||
            (request.ChangedTypeTokens[0] & unchecked((int)0xFF000000)) != TypeDefinitionTokenPrefix)
        {
            return "Source Update v1 requires exactly one changed TypeDef token.";
        }
        try
        {
            var method = module.Assembly.ManifestModule.ResolveMethod(request.ExpectedMethodToken);
            if (method?.DeclaringType?.MetadataToken != request.ChangedTypeTokens[0])
                return "The changed TypeDef does not declare the expected MethodDef.";
        }
        catch (Exception exception)
        {
            return $"The expected MethodDef could not be resolved in the allowlisted module: {exception.Message}";
        }
        if (request.MetadataDelta.Length > maximumDeltaBytes ||
            request.IlDelta.Length > maximumDeltaBytes ||
            request.PdbDelta.Length > maximumDeltaBytes)
        {
            return $"A Source Update delta exceeds the {maximumDeltaBytes}-byte limit.";
        }
        if (!HashMatches(request.MetadataDelta, request.MetadataDeltaHash) ||
            !HashMatches(request.IlDelta, request.IlDeltaHash) ||
            !HashMatches(request.PdbDelta, request.PdbDeltaHash))
        {
            return "One or more Source Update delta hashes do not match their payloads.";
        }
        return null;
    }

    private SourceUpdateApplyResult Rejected(
        SourceUpdateApplyRequest request,
        string error) =>
        new(
            SourceUpdateApplyStatus.Rejected,
            request.UpdateId,
            module.Identity.ModuleMvid,
            module.Identity.Generation,
            module.Identity.SourceHash ?? request.PreviousSourceHash,
            request.MetadataDeltaHash,
            request.IlDeltaHash,
            request.PdbDeltaHash,
            [],
            module.Identity.RestartRequired,
            error);

    private string[] NotifyHandlers(int[] changedTypeTokens)
    {
        try
        {
            var changedTypes = changedTypeTokens
                .Select(module.Assembly.ManifestModule.ResolveType)
                .ToArray();
            return notifyHandlers(changedTypes);
        }
        catch (Exception exception)
        {
            return [$"Changed-type resolution failed after apply: {exception.Message}"];
        }
    }

    private static SourceUpdateModuleState CreateModule(SourceUpdateHostOptions options)
    {
        var launch = options.Launch;
        if (launch.SchemaVersion != 1)
            throw new InvalidOperationException($"Source Update launch schema {launch.SchemaVersion} is unsupported.");

        var assemblyPath = Path.GetFullPath(options.Assembly.Location);
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        if (!string.Equals(assemblyPath, Path.GetFullPath(launch.AssemblyPath), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(pdbPath, Path.GetFullPath(launch.PdbPath), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The loaded assembly does not match the editable launch artifact paths.");
        }
        if (!File.Exists(assemblyPath) || !File.Exists(pdbPath))
            throw new InvalidOperationException("The editable launch assembly or portable PDB is missing.");

        var assemblyHash = HashFile(assemblyPath);
        var pdbHash = HashFile(pdbPath);
        var mvid = options.Assembly.ManifestModule.ModuleVersionId.ToString("N");
        if (!string.Equals(assemblyHash, launch.AssemblySha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(pdbHash, launch.PdbSha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(mvid, launch.ModuleMvid, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The loaded module identity does not match the immutable editable launch manifest.");
        }

        return new(
            options.Assembly,
            new(
                options.Assembly.GetName().Name ?? throw new InvalidOperationException("Editable assembly has no name."),
                mvid,
                assemblyPath,
                assemblyHash,
                pdbPath,
                pdbHash,
                launch.ProjectIdentityHash,
                0,
                null,
                false));
    }

    private static void ValidateProcessMode()
    {
        if (!string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_MODIFIABLE_ASSEMBLIES"),
            "debug",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Source Update requires DOTNET_MODIFIABLE_ASSEMBLIES=debug.");
        }
        if (Debugger.IsAttached)
            throw new InvalidOperationException("Source Update cannot share a process with a managed debugger.");
        if (IsEnabled("CORECLR_ENABLE_PROFILING") || IsEnabled("COR_ENABLE_PROFILING"))
            throw new InvalidOperationException("Source Update cannot share a process with a profiler/ReJIT session.");
    }

    private static bool IsEnabled(string variable) =>
        Environment.GetEnvironmentVariable(variable) is { } value &&
        value != "0" &&
        !value.Equals("false", StringComparison.OrdinalIgnoreCase);

    private static bool HashMatches(byte[] payload, string expected) =>
        string.Equals(Hash(payload), expected, StringComparison.OrdinalIgnoreCase);

    private static string Hash(byte[] payload) =>
        Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
