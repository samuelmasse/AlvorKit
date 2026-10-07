namespace AlvorKit;

public static class EntMutate
{
    extension<T>(T ent) where T : IEntMut
    {
        public EntMutator<T> Mutate() => new(ent);

        public EntMutator<T> Mutate(Action<T> action) => new EntMutator<T>(ent).Mutate(action);

        /// <summary>Dispatches to the handle's lifetime-aware Clear contract, including Indexed notifications.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Clear() => ent.Clear();
    }

    /// <summary>Clears raw storage; Indexed teardown calls this only after its lifecycle and index phases.</summary>
    internal static void ClearComponents<T>(T ent) where T : IEntMut
    {
        if (!ent.IsAlive)
            return;

        var handle = ent.Handle;
        new EntMut(handle.Index, handle.Generation).ResetArchetypal();

        foreach (var field in EntReg.PageFields.Fields(handle.PageIndex))
        {
            if (field.Has(ent))
                field.Unset(ent);
        }
    }
}
