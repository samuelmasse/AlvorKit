namespace AlvorKit;

internal readonly record struct EntArchEdge(
    int FieldId,
    int DstArchId,
    int NextEdgeIndex);
