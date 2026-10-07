namespace AlvorKit;

internal static class ArchRemoveCached
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run<A>(EntMut[] ents)
    {
        long count = 0;

        for (int i = 0; i < ents.Length; i++)
        {
            if (ents[i].UnsetArchetypal<int, FToggle, A>())
                count++;
        }

        return count;
    }
}
