namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Optional loaded-body and immutable-generation interception capability.</summary>
public interface IInterceptionGenerationBackend :
    ILoadedMethodBodySnapshotResolver
{
    /// <summary>Installs one immutable method generation.</summary>
    IInterceptionGenerationPatchHandle Install(
        InterceptionGenerationPlan plan);

    /// <summary>Reads the authoritative loaded body for one exact target.</summary>
    LoadedMethodBodySnapshot GetLoadedMethodBody(
        InterceptionTarget target);

    /// <summary>Reads generation-specific completion evidence.</summary>
    InterceptionGenerationCompletion GetGenerationCompletion(
        ulong requestId);

    /// <summary>Reads one generated metadata relocation result.</summary>
    InterceptionGenerationRelocationResult GetRelocationResult(
        ulong requestId,
        uint relocationIndex);
}
