namespace AlvorKit;

/// <summary>Identifies one actionable operation-route preparation failure.</summary>
public enum MockInterceptionPreparationFailureReason
{
    /// <summary>The CoreCLR profiler is not connected or ready.</summary>
    ProfilerUnavailable,

    /// <summary>The managed adapter and connected profiler use incompatible ABIs.</summary>
    AbiMismatch,

    /// <summary>The profiler rejected the caller module's exact allowlist identity.</summary>
    ModuleAllowlistRejected,

    /// <summary>The loaded caller body no longer matches the planned baseline.</summary>
    StaleBody,

    /// <summary>The selected operation cannot use an exact supported wrapper signature.</summary>
    UnsupportedSignature,

    /// <summary>Managed route preparation threw before returning a structured result.</summary>
    PreparationFailed,

    /// <summary>The route conflicts with another physical or logical interception claim.</summary>
    Collision,

    /// <summary>CoreCLR did not activate the requested ReJIT generation.</summary>
    RejitFailed,

    /// <summary>A rollback could not restore a known pristine route state.</summary>
    RollbackFailed
}
