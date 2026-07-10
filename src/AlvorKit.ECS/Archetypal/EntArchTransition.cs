namespace AlvorKit.ECS;

internal struct EntArchTransition(int addArchId, int removeArchId)
{
    internal int AddArchId = addArchId;
    internal int RemoveArchId = removeArchId;
}
