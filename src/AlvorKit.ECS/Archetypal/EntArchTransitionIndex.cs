namespace AlvorKit.ECS;

/// <summary>Stores the shared sparse transition hash used only by high-degree archs in one group.</summary>
/// <param name="capacity">Power-of-two slot capacity maintained at no more than 50% load.</param>
internal sealed class EntArchTransitionIndex(int capacity)
{
    /// <summary>Stores packed arch-and-field keys, with zero reserved for an empty slot.</summary>
    internal readonly ulong[] Keys = new ulong[capacity];
    /// <summary>Stores the destination arch aligned with each occupied key slot.</summary>
    internal readonly int[] DstArchIds = new int[capacity];
    /// <summary>Counts occupied slots.</summary>
    internal int Count;
}
