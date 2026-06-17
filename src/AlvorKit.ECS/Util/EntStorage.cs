namespace AlvorKit.ECS;

internal static class EntStorage<T, N>
{
    internal static readonly object Lock = new();
    internal static readonly EntField Field = new EntField<T, N>();
    internal static readonly EntStorageView<T, N> View = new();

    internal static (int Generation, T? Value)[]?[] Sparse = [];

    static EntStorage()
    {
        lock (EntReg.Lock)
        {
            EntReg.Fields.Add(Field);
            EntReg.Storage.Add(View);
            Field.ExpandCapacity();
        }
    }
}

internal class EntStorageView;

internal class EntStorageView<T, N> : EntStorageView
{
    internal EntField Field => EntStorage<T, N>.Field;
    internal (int Generation, T? Value)[]?[] Sparse => EntStorage<T, N>.Sparse;
}
