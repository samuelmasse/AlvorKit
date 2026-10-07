namespace AlvorKit;

internal static class ArchUniqueSignature
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run<A>(EntMut ent, int archCount)
    {
        uint previous = 0;

        for (int i = 1; i <= archCount; i++)
        {
            uint signature = (uint)i ^ ((uint)i >> 1);
            uint changed = previous ^ signature;
            int field = BitOperations.TrailingZeroCount(changed);
            ArchShapes.ToggleField<A>(ent, field, (signature & changed) != 0);
            previous = signature;
        }
    }
}
