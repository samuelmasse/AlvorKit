namespace AlvorKit;

/// <summary>Identifies one required or hybrid connection slot within its owning node's state.</summary>
/// <param name="IsHybrid">Whether the index belongs to the hybrid rather than required-source table.</param>
/// <param name="Index">The zero-based runtime metadata slot index.</param>
internal readonly record struct FnConnectionKey(bool IsHybrid, int Index);
