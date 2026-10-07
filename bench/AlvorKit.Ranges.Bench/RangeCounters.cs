namespace AlvorKit;

internal class RangeCounters
{
    private int packs;
    private int resizes;
    internal int Packs => packs;
    internal int Resizes => resizes;

    internal void Pack() => packs++;
    internal void Resize(long size) => resizes++;
}
