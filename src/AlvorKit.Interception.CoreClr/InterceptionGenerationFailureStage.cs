namespace AlvorKit;

/// <summary>Identifies the native stage that rejected a method generation.</summary>
public enum InterceptionGenerationFailureStage
{
    /// <summary>No failure occurred.</summary>
    None,
    /// <summary>The ABI envelope or generation relationship was invalid.</summary>
    Validation,
    /// <summary>The exact loaded target could not be resolved.</summary>
    Target,
    /// <summary>The authoritative loaded-body identity was stale.</summary>
    Baseline,
    /// <summary>Metadata emission or relocation failed.</summary>
    Metadata,
    /// <summary>The original-to-instrumented IL map was invalid.</summary>
    IlMap,
    /// <summary>The ReJIT request or callback failed.</summary>
    Rejit
}
