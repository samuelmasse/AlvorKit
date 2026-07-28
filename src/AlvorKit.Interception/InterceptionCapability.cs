namespace AlvorKit.Interception;

/// <summary>Capabilities reported by the loaded native profiler.</summary>
[Flags]
public enum InterceptionCapability : ulong
{
    /// <summary>No capability is available.</summary>
    None = 0,

    /// <summary>The profiler can request method ReJIT.</summary>
    Rejit = 1 << 0,

    /// <summary>The profiler repairs existing inliners.</summary>
    RejitInliners = 1 << 1,

    /// <summary>The profiler can restore original IL.</summary>
    Revert = 1 << 2,

    /// <summary>The profiler accepts complete verified CLR method bodies.</summary>
    RawIl = 1 << 3,

    /// <summary>Several exact methods can be active simultaneously.</summary>
    MultiplePatches = 1 << 4,

    /// <summary>The profiler validates the metadata signature hash before ReJIT.</summary>
    SignatureValidation = 1 << 5,

    /// <summary>The profiler can preserve original IL and call an exact managed function pointer on selector hits.</summary>
    ExactDispatch = 1 << 6,

    /// <summary>The profiler accepts immutable generation and prior-generation identities.</summary>
    MethodGenerations = 1 << 7,

    /// <summary>The profiler creates exact metadata tokens after module load.</summary>
    LateMetadata = 1 << 8,

    /// <summary>The profiler submits original-to-instrumented IL maps.</summary>
    IlMap = 1 << 9,

    /// <summary>The profiler validates authoritative loaded-body SHA-256 identities.</summary>
    BodyIdentity = 1 << 10,

    /// <summary>The profiler returns exact loaded method-body bytes after eligibility checks.</summary>
    LoadedBody = 1 << 11
}
