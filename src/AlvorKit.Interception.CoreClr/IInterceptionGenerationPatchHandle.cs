namespace AlvorKit;

/// <summary>Patch ownership that can replace an active immutable method generation.</summary>
public interface IInterceptionGenerationPatchHandle :
    IInterceptionPatchHandle
{
    /// <summary>Requests another immutable generation for the same exact target.</summary>
    ulong Replace(InterceptionGenerationPlan plan);
}
