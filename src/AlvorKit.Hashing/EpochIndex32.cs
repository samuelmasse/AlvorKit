namespace AlvorKit.Hashing;

/// <summary>Retains an epoch-cleared mapping from signed 32-bit keys to caller-owned dense slots.</summary>
/// <param name="capacity">The active mapping count for which storage is initially retained.</param>
public class EpochIndex32(int capacity)
{
    private int[] keys = CreateArray<int>(capacity);
    private int[] slots = CreateArray<int>(capacity);
    private uint[] stamps = CreateArray<uint>(capacity);
    private uint epoch = 1;
    private int count;

    /// <summary>Gets the number of mappings active in the current epoch.</summary>
    public int Count => count;

    /// <summary>Discards every current mapping in constant time and begins a new epoch.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Begin()
    {
        count = 0;
        if (epoch != uint.MaxValue)
        {
            epoch++;
            return;
        }

        stamps.AsSpan().Clear();
        epoch = 1;
    }

    /// <summary>Ensures capacity for the requested active mapping count while preserving current mappings.</summary>
    public void EnsureCapacity(int requestedCapacity)
    {
        var tableLength = TableLength(requestedCapacity);
        if (tableLength <= stamps.Length)
            return;

        Resize(tableLength);
    }

    /// <summary>Tries to get the caller-owned dense slot mapped to a key.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(int key, out int slot)
    {
        var index = Find(key, out var found);
        slot = found ? slots[index] : -1;
        return found;
    }

    /// <summary>Gets an existing slot or maps the supplied slot when the key is absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOrAdd(int key, int slot, out bool added)
    {
        var index = Find(key, out var found);
        if (found)
        {
            added = false;
            return slots[index];
        }

        if (count >= stamps.Length >> 1)
        {
            EnsureCapacity(count + 1);
            index = Find(key, out found);
        }

        keys[index] = key;
        slots[index] = slot;
        stamps[index] = epoch;
        count++;
        added = true;
        return slot;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Find(int key, out bool found)
    {
        if (stamps.Length == 0)
        {
            found = false;
            return 0;
        }

        var mask = stamps.Length - 1;
        var index = TableHash.Index(key, mask);
        while (stamps[index] == epoch)
        {
            if (keys[index] == key)
            {
                found = true;
                return index;
            }

            index = (index + 1) & mask;
        }

        found = false;
        return index;
    }

    private void Resize(int tableLength)
    {
        var previousKeys = keys;
        var previousSlots = slots;
        var previousStamps = stamps;
        keys = new int[tableLength];
        slots = new int[tableLength];
        stamps = new uint[tableLength];

        for (var previousIndex = 0; previousIndex < previousStamps.Length; previousIndex++)
        {
            if (previousStamps[previousIndex] != epoch)
                continue;

            var index = Find(previousKeys[previousIndex], out _);
            keys[index] = previousKeys[previousIndex];
            slots[index] = previousSlots[previousIndex];
            stamps[index] = epoch;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int TableLength(int requestedCapacity)
    {
        if (requestedCapacity == 0)
            return 0;

        var doubledCapacity = (uint)requestedCapacity << 1;
        return (int)BitOperations.RoundUpToPowerOf2(doubledCapacity);
    }

    private static T[] CreateArray<T>(int requestedCapacity)
    {
        var tableLength = TableLength(requestedCapacity);
        return tableLength == 0 ? [] : new T[tableLength];
    }
}
