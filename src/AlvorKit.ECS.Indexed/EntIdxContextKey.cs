namespace AlvorKit;

/// <summary>A borrowed context identity. Its slot is recycled only after all arenas and callbacks end.</summary>
internal readonly record struct EntIdxContextKey(int Index, int Generation);
