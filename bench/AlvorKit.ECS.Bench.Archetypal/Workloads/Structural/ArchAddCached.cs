namespace AlvorKit;

internal static class ArchAddCached
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut[] ents)
    {
        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);
    }
}
