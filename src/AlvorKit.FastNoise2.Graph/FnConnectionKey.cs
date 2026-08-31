namespace AlvorKit;

/// <summary>Identifies one required or hybrid node-connection slot on a native target node.</summary>
internal readonly record struct FnConnectionKey(FnNode Target, bool IsHybrid, int Index);
