namespace AlvorKit.Engine.Loop;

/// <summary>Drains the root deletion bin tree from the root loop.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootBinEmptyer(RootBin bin)
{
    /// <summary>Deletes all queued GL objects in the root bin tree.</summary>
    public void Empty() => Empty(bin);

    private static void Empty(GlBin current)
    {
        foreach (var child in current.Children)
            Empty(child);
        current.Empty();
    }
}
