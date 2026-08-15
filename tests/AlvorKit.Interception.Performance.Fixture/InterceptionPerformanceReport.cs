namespace AlvorKit;

/// <summary>Records end-to-end latency and native callback evidence for one cold operation.</summary>
internal sealed record ColdInterceptionMeasurement(
    string Name,
    double WallMilliseconds,
    double NativeMilliseconds,
    uint RejitStartedCallbacks,
    uint ParameterCallbacks,
    uint RejitFinishedCallbacks);

/// <summary>Records warm latency samples and current-thread allocation evidence for one route.</summary>
internal sealed record WarmInterceptionMeasurement(
    string Name,
    double MedianNanosecondsPerCall,
    double MinimumNanosecondsPerCall,
    double MaximumNanosecondsPerCall,
    long AllocatedBytes);

/// <summary>Proves a managed route swap issued no profiler request or patch transition.</summary>
internal sealed record HandlerSwapInterceptionEvidence(
    ulong BeforeLastRequestId,
    ulong AfterLastRequestId,
    uint BeforePendingRequests,
    uint AfterPendingRequests,
    uint BeforeActivePatches,
    uint AfterActivePatches);

/// <summary>Contains reproducible environment, method, latency, and allocation evidence.</summary>
internal sealed record InterceptionPerformanceReport(
    string Schema,
    DateTimeOffset RecordedAtUtc,
    string Framework,
    string OperatingSystem,
    string ProcessArchitecture,
    int TierWarmupIterations,
    int TimedIterations,
    int TimedSamples,
    int AllocationIterations,
    ColdInterceptionMeasurement ColdInstall,
    WarmInterceptionMeasurement WarmDirect,
    WarmInterceptionMeasurement WarmInertRoute,
    WarmInterceptionMeasurement WarmActiveExact,
    WarmInterceptionMeasurement WarmSwappedExact,
    HandlerSwapInterceptionEvidence HandlerSwap,
    ColdInterceptionMeasurement ColdRemove);
