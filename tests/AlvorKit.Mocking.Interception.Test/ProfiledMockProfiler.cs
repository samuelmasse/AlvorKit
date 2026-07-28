namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Shares exact reflection and completion operations for the profiled fixture.</summary>
internal static class ProfiledMockProfiler
{
    /// <summary>The maximum time allowed for one profiler transition.</summary>
    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(10);

    /// <summary>Gets the one caller selected for ReJIT.</summary>
    internal static MethodInfo SelectedCaller =>
        Method(
            typeof(ProfiledMockCaller),
            nameof(ProfiledMockCaller.Selected));

    /// <summary>Gets the same-module raw caller template.</summary>
    internal static MethodInfo RoutedTemplate =>
        Method(
            typeof(ProfiledMockCaller),
            nameof(ProfiledMockCaller.RoutedTemplate));

    /// <summary>Gets the concrete sealed nonvirtual operation.</summary>
    internal static MethodInfo Operation =>
        typeof(ProfiledMockTarget).GetMethod(
            nameof(ProfiledMockTarget.Calculate))!;

    /// <summary>Finds the selected caller's exact ordinary operation instruction.</summary>
    internal static int FindOperationOffset(
        MethodInfo caller,
        MethodInfo operation)
    {
        var il = caller.GetMethodBody()?.GetILAsByteArray() ??
            throw new InvalidOperationException(
                "The selected caller has no readable IL.");
        for (var offset = 0; offset <= il.Length - 5; offset++)
        {
            if (il[offset] is not (0x28 or 0x6F))
                continue;
            if (BinaryPrimitives.ReadInt32LittleEndian(
                    il.AsSpan(offset + 1)) ==
                operation.MetadataToken)
            {
                return offset;
            }
        }

        throw new InvalidOperationException(
            "The selected caller does not contain the expected operation.");
    }

    /// <summary>Waits for a profiler request while driving the selected caller to JIT.</summary>
    internal static InterceptionCompletion WaitFor(
        IInterceptionBackend profiler,
        ulong requestId)
        => WaitFor(
            profiler,
            requestId,
            () => _ = ProfiledMockCaller.Selected(
                new ProfiledMockTarget(),
                1));

    /// <summary>Waits for a profiler request while driving its exact selected caller to JIT.</summary>
    internal static InterceptionCompletion WaitFor(
        IInterceptionBackend profiler,
        ulong requestId,
        Action driveCaller)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < RequestTimeout)
        {
            driveCaller();
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1));
        }

        var timedOut = profiler.GetCompletion(requestId);
        throw new TimeoutException(
            $"Request {requestId} timed out in {timedOut.State}; " +
            $"started={timedOut.RejitStartedCallbacks}, " +
            $"parameters={timedOut.ParameterCallbacks}, " +
            $"finished={timedOut.RejitFinishedCallbacks}, " +
            $"errors={timedOut.RejitErrorCallbacks}.");
    }

    /// <summary>Gets one exact nonpublic static fixture method.</summary>
    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;
}
