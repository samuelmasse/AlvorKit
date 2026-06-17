namespace AlvorKit.ECS;

/// <summary>Provides mutation convenience extensions for mutable entity handles.</summary>
public static class EntMutate
{
    extension<T>(T ent) where T : IEntMut
    {
        /// <summary>Wraps the entity in a builder-style mutator.</summary>
        public EntMutator<T> Mutate() => new(ent);

        /// <summary>Runs a mutation action and returns the builder-style mutator.</summary>
        public EntMutator<T> Mutate(Action<T> action) => new EntMutator<T>(ent).Mutate(action);

        /// <summary>Removes every component currently attached to the entity.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            if (!ent.IsAlive)
                return;

            foreach (var field in EntReg.PageFields.Fields(ent.Handle.PageIndex))
            {
                if (field.Has(ent))
                    field.Unset(ent);
            }
        }
    }
}
