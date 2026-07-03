namespace AlvorKit.UI;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(NodeArrayDebugView))]
internal record struct NodeArray(int Rank, int Index, int Count, NodeArrayAllocator? Allocator)
{
    private sealed class NodeArrayDebugView(NodeArray array)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public EntMut[] Items
        {
            get
            {
                if (array.Allocator == null || array.Rank == 0)
                    return [];

                return array.Allocator.Span(array)[..array.Count].ToArray();
            }
        }
    }
}
