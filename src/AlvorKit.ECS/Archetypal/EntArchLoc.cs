namespace AlvorKit.ECS;

internal struct EntArchLoc(int allocatorId, int archId, int row)
{
    public int AllocatorId = allocatorId;
    public int ArchId = archId;
    public int Row = row;
}
