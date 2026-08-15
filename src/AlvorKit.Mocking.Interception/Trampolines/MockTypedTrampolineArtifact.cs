using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Holds one reusable interception prefix and its immutable declared-to-carrier mapping.
/// </summary>
internal sealed class MockTypedTrampolineArtifact
{
    private readonly MockDispatchCacheKey key;
    private readonly MethodInfo prefix;
    private readonly MethodInfo finalizer;
    private readonly ImmutableArray<int> carrierIndices;

    /// <summary>
    /// Creates an immutable generated-code artifact.
    /// </summary>
    internal MockTypedTrampolineArtifact(
        MockDispatchCacheKey key,
        MethodInfo prefix,
        MethodInfo finalizer,
        ImmutableArray<int> carrierIndices)
    {
        this.key = key;
        this.prefix = prefix;
        this.finalizer = finalizer;
        this.carrierIndices = carrierIndices;
    }

    internal MockDispatchCacheKey Key => key;
    internal MethodInfo Prefix => prefix;
    internal MethodInfo Finalizer => finalizer;
    internal ImmutableArray<int> CarrierIndices => carrierIndices;
}
