namespace AlvorKit.ECS;

internal class EntPageFieldMap
{
    private List<EntField>[] fields = new List<EntField>[4];

    internal Span<EntField> Fields(int pageIndex)
    {
        if (pageIndex >= fields.Length)
            return [];

        var list = fields[pageIndex];
        if (list == null)
            return [];

        return CollectionsMarshal.AsSpan(list);
    }

    internal void Clear(int pageIndex)
    {
        lock (this)
        {
            if (pageIndex >= fields.Length)
                return;

            fields[pageIndex]?.Clear();
        }
    }

    internal void Add(int pageIndex, EntField field)
    {
        lock (this)
        {
            if (pageIndex >= fields.Length)
            {
                int newSize = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)pageIndex + 1);
                Array.Resize(ref fields, newSize);
            }

            fields[pageIndex] ??= [];
            fields[pageIndex].Add(field);
        }
    }
}

