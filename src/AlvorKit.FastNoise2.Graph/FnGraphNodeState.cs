namespace AlvorKit;

/// <summary>Owns one native reference and only the managed dependencies connected to that node.</summary>
/// <param name="owner">The service used for metadata access and same-graph validation.</param>
/// <param name="handle">The independently finalizable external native reference.</param>
/// <param name="isEncoded">Whether the node has opaque connections loaded from an encoded tree.</param>
internal class FnGraphNodeState(FnGraph owner, FnNodeHandle handle, bool isEncoded)
{
    private readonly Dictionary<FnConnectionKey, FnGraphNodeState> connections = [];

    /// <summary>Gets the configuration service, which does not retain this state.</summary>
    internal FnGraph Owner => owner;

    /// <summary>Borrows the native reference; keep this state alive through the native call.</summary>
    internal FnNode Native => new(handle.DangerousGetHandle());

    /// <summary>Gets whether preexisting hybrid connections are opaque.</summary>
    internal bool IsEncoded => isEncoded;

    /// <summary>Tests whether a wrapper-created connection occupies the slot.</summary>
    internal bool HasConnection(FnConnectionKey key) => connections.ContainsKey(key);

    /// <summary>Retains a successfully connected source and releases the previous managed dependency.</summary>
    internal void RetainConnection(FnConnectionKey key, FnGraphNodeState source) => connections[key] = source;

    /// <summary>Rejects self-reference or a dependency path back to this node before native mutation.</summary>
    internal void RequireAcyclicConnection(FnGraphNodeState source)
    {
        if (source.CanReach(this, []))
            throw new InvalidOperationException("FastNoise2 node connections must form an acyclic graph.");
    }

    /// <summary>Traverses managed dependencies by object identity, independent of recycled native handles.</summary>
    private bool CanReach(FnGraphNodeState wanted, HashSet<FnGraphNodeState> visited)
    {
        if (ReferenceEquals(this, wanted))
            return true;

        if (!visited.Add(this))
            return false;

        foreach (var source in connections.Values)
        {
            if (source.CanReach(wanted, visited))
                return true;
        }

        return false;
    }
}
