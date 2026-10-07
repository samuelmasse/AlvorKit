namespace AlvorKit;

internal static class ArchCombinations
{
    internal static uint Next(uint value)
    {
        uint smallest = value & (0u - value);
        uint ripple = value + smallest;
        return ripple | (((value ^ ripple) >> 2) / smallest);
    }
}
