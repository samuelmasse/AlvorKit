namespace AlvorKit.Mocking.Interception.Test;

internal sealed class ProfiledSubstituteConstructionTag;

/// <summary>Owns the exact allocation site used by construction substitution.</summary>
internal static class ProfiledSubstituteConstructionCaller
{
    /// <summary>Allocates one target through a profiler-selected newobj site.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledReceiverFreeTarget Selected(int value) =>
        new(value);

    /// <summary>Invokes the leased production construction wrapper.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe ProfiledReceiverFreeTarget Invoke(int value)
    {
        if (!ProfiledReceiverFreeRouteState<
                ProfiledSubstituteConstructionTag>
            .TryAcquire(out var entryPoint))
        {
            return ProfiledReceiverFreeOriginal.Construct(value);
        }

        return ((delegate* managed<
            int,
            ProfiledReceiverFreeTarget>)entryPoint)(value);
    }
}
