namespace AlvorKit;

internal static class ArchRemoveUnknown
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut[] ents)
    {
        long removed = 0;

        for (int i = 0; i < ents.Length; i++)
        {
            if (ents[i].UnsetArchetypal<int, FToggle, A>())
                removed++;
        }

        return removed;
    }
}
