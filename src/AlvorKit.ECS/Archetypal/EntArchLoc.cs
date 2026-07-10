namespace AlvorKit.ECS;

internal struct EntArchLoc(int allocId, int archId, int row)
{
    internal int AllocId = allocId;
    internal int ArchId = archId;
    internal int Row = row;
}
