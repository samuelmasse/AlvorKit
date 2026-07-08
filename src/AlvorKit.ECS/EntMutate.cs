namespace AlvorKit.ECS;

public static class EntMutate
{
    extension<T>(T ent) where T : IEntMut
    {
                public EntMutator<T> Mutate() => new(ent);

                public EntMutator<T> Mutate(Action<T> action) => new EntMutator<T>(ent).Mutate(action);

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
