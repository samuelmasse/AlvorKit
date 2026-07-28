namespace AlvorKit.LivePatch;

/// <summary>Stable resolver called by exact ReJIT wrappers to acquire one selected managed trampoline.</summary>
public static class LivePatchRuntime
{
    private static readonly ConcurrentDictionary<ulong, LivePatchSlot> Slots = [];
    private static readonly nint resolverPointer = CreateResolverPointer();

    internal static nint ResolverPointer => resolverPointer;

    /// <summary>Returns an acquired exact trampoline pointer, or zero to execute original IL.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static nint ResolveHandler(ulong slotId, object? receiver)
    {
        try
        {
            return Slots.TryGetValue(slotId, out var slot)
                ? slot.Resolve(receiver)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    internal static void Attach(ulong slotId, LivePatchSlot slot)
    {
        if (!Slots.TryAdd(slotId, slot))
            throw new InvalidOperationException($"LivePatch slot {slotId} is already attached.");
    }

    internal static void Detach(ulong slotId, LivePatchSlot slot) =>
        Slots.TryRemove(new(slotId, slot));

    private static nint CreateResolverPointer()
    {
        var method = typeof(LivePatchRuntime).GetMethod(
            nameof(ResolveHandler),
            BindingFlags.Public | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }
}
