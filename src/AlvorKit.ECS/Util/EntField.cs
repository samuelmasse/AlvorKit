namespace AlvorKit.ECS;

internal abstract class EntField : EntComponentView
{
    internal override Type? ArchGroupType() => null;
    internal abstract override Type NameType();
    internal abstract override Type ValueType();
    internal abstract override bool Has(Ent ent);
    internal abstract override object? Get(Ent ent);
    internal abstract bool Has<T>(T ent) where T : IEnt;
    internal abstract object? Get<T>(T ent) where T : IEnt;
    internal abstract bool Unset<T>(T ent) where T : IEntMut;
    internal abstract void Reset(EntMut ent);
    internal abstract void ResetPage(int pageIndex);
    internal abstract void ExpandCapacity();
}

internal class EntField<T, N> : EntField
{
    internal override Type NameType() => typeof(N);
    internal override Type ValueType() => typeof(T);
    internal override bool Has(Ent ent) => ent.Has<T, N>();
    internal override object? Get(Ent ent) => ent.Get<T, N>();
    internal override bool Has<T1>(T1 ent) => ent.Has<T, N>();
    internal override object? Get<T1>(T1 ent) => ent.Get<T, N>();
    internal override bool Unset<T1>(T1 ent) => ent.Unset<T, N>();
    internal override void Reset(EntMut ent) => ent.Reset<T, N>();
    internal override void ResetPage(int pageIndex) => EntStorage<T, N>.Sparse[pageIndex] = null;
    internal override void ExpandCapacity()
    {
        int size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)EntReg.NextPage);
        ref var sparse = ref EntStorage<T, N>.Sparse;

        if (sparse.Length < size)
            Array.Resize(ref sparse, size);
    }
}
