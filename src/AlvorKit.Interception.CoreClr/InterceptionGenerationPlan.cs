using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>One immutable ABI v3 method generation with exact late-metadata inputs.</summary>
public sealed class InterceptionGenerationPlan
{
    /// <summary>Creates and validates one complete generated method plan.</summary>
    public InterceptionGenerationPlan(
        InterceptionTarget target,
        InterceptionMethodBody methodBody,
        LoadedMethodBodyIdentity baselineBodyIdentity,
        ulong generationId,
        ulong priorGenerationId,
        IEnumerable<InterceptionGenerationRelocation> relocations,
        IEnumerable<InterceptionGenerationIlMapEntry> ilMap,
        InterceptionPatchFlags flags = InterceptionPatchFlags.DisableInlining)
    {
        ArgumentNullException.ThrowIfNull(methodBody);
        ArgumentNullException.ThrowIfNull(baselineBodyIdentity);
        ArgumentNullException.ThrowIfNull(relocations);
        ArgumentNullException.ThrowIfNull(ilMap);
        if (!target.IsValid)
            throw new ArgumentException("A valid interception target is required.", nameof(target));
        if (generationId == 0)
            throw new ArgumentOutOfRangeException(nameof(generationId));
        if ((flags & ~InterceptionPatchFlags.DisableInlining) != 0)
            throw new ArgumentOutOfRangeException(nameof(flags));

        Target = target;
        MethodBody = methodBody;
        BaselineBodyIdentity = baselineBodyIdentity;
        GenerationId = generationId;
        PriorGenerationId = priorGenerationId;
        Relocations = [.. relocations];
        IlMap = [.. ilMap];
        Flags = flags;
    }

    /// <summary>Gets the exact loaded method target.</summary>
    public InterceptionTarget Target { get; }

    /// <summary>Gets the complete body containing four-byte zero token placeholders.</summary>
    public InterceptionMethodBody MethodBody { get; }

    /// <summary>Gets the SHA-256 identity of the authoritative loaded baseline.</summary>
    public LoadedMethodBodyIdentity BaselineBodyIdentity { get; }

    /// <summary>Gets the monotonically increasing candidate generation ID.</summary>
    public ulong GenerationId { get; }

    /// <summary>Gets the active generation this candidate replaces, or zero initially.</summary>
    public ulong PriorGenerationId { get; }

    /// <summary>Gets the bounded exact metadata relocations.</summary>
    public ImmutableArray<InterceptionGenerationRelocation> Relocations { get; }

    /// <summary>Gets the original-to-instrumented IL map.</summary>
    public ImmutableArray<InterceptionGenerationIlMapEntry> IlMap { get; }

    /// <summary>Gets the ReJIT policy flags.</summary>
    public InterceptionPatchFlags Flags { get; }
}
