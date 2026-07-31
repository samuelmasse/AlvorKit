namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>One retained allocation and its leaf-to-root managed stack.</summary>
/// <param name="AllocationOrdinal">One-based allocation position inside the capture.</param>
/// <param name="RuntimeTypeId">Profiler-local runtime class identifier for the allocated object.</param>
/// <param name="StackHResult">CoreCLR stack-walk status retained with the sample.</param>
/// <param name="Frames">Resolved managed frames in leaf-to-root order.</param>
public record InterceptionAllocationSample(
    ulong AllocationOrdinal,
    ulong RuntimeTypeId,
    int StackHResult,
    IReadOnlyList<InterceptionAllocationStackFrame> Frames);

/// <summary>One managed stack frame resolved to a module, method token, and optional IL offset.</summary>
/// <param name="ModuleMvid">Version identifier of the frame's managed module.</param>
/// <param name="MethodToken">Metadata method-definition token inside the module.</param>
/// <param name="IlOffset">Resolved IL offset, or <see langword="null"/> when unavailable.</param>
public readonly record struct InterceptionAllocationStackFrame(
    Guid ModuleMvid,
    int MethodToken,
    int? IlOffset);
