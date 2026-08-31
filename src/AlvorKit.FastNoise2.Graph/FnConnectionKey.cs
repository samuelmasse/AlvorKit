namespace AlvorKit;

/// <summary>Identifies one required or hybrid node-connection slot on a native target node.</summary>
/// <param name="Target">The native node that owns the connection slot.</param>
/// <param name="IsHybrid">Whether the index belongs to the hybrid rather than required-source table.</param>
/// <param name="Index">The zero-based runtime metadata slot index.</param>
internal readonly record struct FnConnectionKey(FnNode Target, bool IsHybrid, int Index);
