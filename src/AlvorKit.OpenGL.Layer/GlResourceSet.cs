namespace AlvorKit.OpenGL.Layer;

internal sealed unsafe class GlResourceSet<THandle>(
    string name,
    Func<uint, THandle> fromId,
    Func<THandle, uint> toId) where THandle : struct
{
    private readonly HashSet<THandle> items = [];

    internal IReadOnlySet<THandle> Items => items;
    internal int Count => items.Count;

    internal bool Contains(THandle handle) => items.Contains(handle);

    internal void Track(THandle handle) => items.Add(handle);

    internal void Track(int count, nint handles)
    {
        var ids = (uint*)handles;
        for (var i = 0; i < count; i++)
            Track(fromId(ids[i]));
    }

    internal void Untrack(string function, THandle handle)
    {
        if (!items.Remove(handle))
            throw new GlResourceNotTrackedException<THandle>(function, name, handle);
    }

    internal THandle[] Untrack(string function, int count, nint handles)
    {
        var values = Read(count, handles);
        foreach (var value in values)
        {
            if (!items.Contains(value))
                throw new GlResourceNotTrackedException<THandle>(function, name, value);
        }
        foreach (var value in values)
            items.Remove(value);
        return values;
    }

    internal THandle[] Drain()
    {
        var values = new THandle[items.Count];
        items.CopyTo(values);
        items.Clear();
        return values;
    }

    internal uint[] DrainIds()
    {
        var values = Drain();
        var ids = new uint[values.Length];
        for (var i = 0; i < values.Length; i++)
            ids[i] = toId(values[i]);
        return ids;
    }

    private THandle[] Read(int count, nint handles)
    {
        var ids = (uint*)handles;
        var values = new THandle[count];
        for (var i = 0; i < count; i++)
            values[i] = fromId(ids[i]);
        return values;
    }
}
