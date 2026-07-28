namespace AlvorKit.LivePatch;

internal sealed class LivePatchSlot(InjectorScopeGraph graph)
{
    private readonly Lock gate = new();
    private Entry[] entries = [];

    internal int Count => Volatile.Read(ref entries).Length;

    internal string SelectorDescription =>
        string.Join(
            ", ",
            Volatile.Read(ref entries)
                .Select(static entry => entry.Selector.ToString())
                .OrderBy(static selector => selector, StringComparer.Ordinal));

    internal void Add(
        ulong patchId,
        LivePatchSelector selector,
        IInterceptionHandlerTrampoline trampoline)
    {
        lock (gate)
        {
            foreach (var entry in entries)
            {
                if (entry.Selector.Overlaps(selector, graph))
                {
                    throw new InvalidOperationException(
                        $"Selector '{selector}' overlaps patch {entry.PatchId} selector '{entry.Selector}'. " +
                        "LivePatch requires explicit composition instead of registration-order precedence.");
                }
            }

            var next = new Entry[entries.Length + 1];
            entries.CopyTo(next, 0);
            next[^1] = new(patchId, selector, trampoline);
            Volatile.Write(ref entries, next);
        }
    }

    internal IInterceptionHandlerTrampoline Replace(
        ulong patchId,
        IInterceptionHandlerTrampoline trampoline)
    {
        lock (gate)
        {
            var next = (Entry[])entries.Clone();
            for (var index = 0; index < next.Length; index++)
            {
                if (next[index].PatchId != patchId)
                    continue;

                var previous = next[index].Trampoline;
                next[index] = next[index] with { Trampoline = trampoline };
                Volatile.Write(ref entries, next);
                return previous;
            }
        }

        throw new KeyNotFoundException($"LivePatch {patchId} is not registered.");
    }

    internal IInterceptionHandlerTrampoline? Remove(ulong patchId)
    {
        lock (gate)
        {
            var index = Array.FindIndex(entries, x => x.PatchId == patchId);
            if (index < 0)
                return null;

            var removed = entries[index].Trampoline;
            var next = new Entry[entries.Length - 1];
            entries.AsSpan(0, index).CopyTo(next);
            entries.AsSpan(index + 1).CopyTo(next.AsSpan(index));
            Volatile.Write(ref entries, next);
            return removed;
        }
    }

    internal Exception? GetFailure(ulong patchId)
    {
        var snapshot = Volatile.Read(ref entries);
        foreach (var entry in snapshot)
        {
            if (entry.PatchId == patchId)
                return entry.Trampoline.ConsumeFailure();
        }

        return null;
    }

    internal nint Resolve(object? receiver)
    {
        var snapshot = Volatile.Read(ref entries);
        foreach (var entry in snapshot)
        {
            if (entry.Selector.Matches(receiver, graph) &&
                entry.Trampoline.TryAcquire(out var entryPoint))
            {
                return entryPoint;
            }
        }

        return 0;
    }

    private sealed record Entry(
        ulong PatchId,
        LivePatchSelector Selector,
        IInterceptionHandlerTrampoline Trampoline);
}
