namespace AlvorKit;

/// <summary>Captures process-level retained storage separately from benchmark timing.</summary>
internal static class ArchFootprint
{
    internal static int Run(string occupancy)
    {
        using var arena = new EntArena();
        var count = occupancy switch
        {
            "low" => 2048,
            "high" => 4096,
            _ => throw new ArgumentException("Use --footprint low or --footprint high in a fresh process."),
        };
        var ents = new EntMut[count];
        ArchShapes.RegisterFields<RunArch>(32);
        var before = GC.GetTotalMemory(true);

        if (occupancy == "low")
            ArchLowOccupancy.Run<RunArch>(arena, ents);
        else ArchHighOccupancy.Run<RunArch>(arena, ents);
        var retained = GC.GetTotalMemory(true) - before;
        var metrics = EntArchDiagnostics<RunArch>.Capture();
        Console.WriteLine(
            JsonSerializer.Serialize(
                new
                {
                    Occupancy = occupancy,
                    Count = count,
                    HeapDeltaBytes = retained,
                    Metrics = new Dictionary<string, long>
                    {
                        ["RegisteredFieldCount"] = metrics.RegisteredFieldCount,
                        ["FieldCapacity"] = metrics.FieldCapacity,
                        ["MaterializedArchCount"] = metrics.MaterializedArchCount,
                        ["ArchCapacity"] = metrics.ArchCapacity,
                        ["SignatureMembershipCount"] = metrics.SignatureMembershipCount,
                        ["SignatureMembershipCapacity"] = metrics.SignatureMembershipCapacity,
                        ["SignatureIndexCount"] = metrics.SignatureIndexCount,
                        ["SignatureIndexCapacity"] = metrics.SignatureIndexCapacity,
                        ["SignatureScratchCapacity"] = metrics.SignatureScratchCapacity,
                        ["SingletonArchCount"] = metrics.SingletonArchCount,
                        ["SingletonDirectoryCapacity"] = metrics.SingletonDirectoryCapacity,
                        ["DirectedStructuralEdgeCount"] = metrics.DirectedStructuralEdgeCount,
                        ["TransitionCellCapacity"] = metrics.TransitionCellCapacity,
                        ["StoredTransitionEdgeCount"] = metrics.StoredTransitionEdgeCount,
                        ["TransitionEdgeCapacity"] = metrics.TransitionEdgeCapacity,
                        ["EdgeHeadCapacity"] = metrics.EdgeHeadCapacity,
                        ["HighDegreeArchCount"] = metrics.HighDegreeArchCount,
                        ["TransitionIndexCount"] = metrics.TransitionIndexCount,
                        ["TransitionIndexCapacity"] = metrics.TransitionIndexCapacity,
                        ["AllocDirectoryCount"] = metrics.AllocDirectoryCount,
                        ["AllocDirectoryCapacity"] = metrics.AllocDirectoryCapacity,
                        ["ArchDirectorySlotCapacity"] = metrics.ArchDirectorySlotCapacity,
                        ["OwnedRowSetCount"] = metrics.OwnedRowSetCount,
                        ["RowSetSlotCapacity"] = metrics.RowSetSlotCapacity,
                        ["ActiveRowSetSlotCapacity"] = metrics.ActiveRowSetSlotCapacity,
                        ["RetainedStateCount"] = metrics.RetainedStateCount,
                        ["ActiveStateCount"] = metrics.ActiveStateCount,
                        ["ActiveRowCount"] = metrics.ActiveRowCount,
                        ["RowCapacity"] = metrics.RowCapacity,
                        ["RowSlack"] = metrics.RowSlack,
                        ["ComponentBufferCount"] = metrics.ComponentBufferCount,
                        ["ComponentCapacity"] = metrics.ComponentCapacity,
                        ["CatalogLogicalPayloadBytes"] = metrics.CatalogLogicalPayloadBytes,
                        ["CatalogUsedLogicalPayloadBytes"] = metrics.CatalogUsedLogicalPayloadBytes,
                        ["CatalogSlackLogicalPayloadBytes"] = metrics.CatalogSlackLogicalPayloadBytes,
                        ["RowLogicalPayloadBytes"] = metrics.RowLogicalPayloadBytes,
                        ["EntUsedLogicalPayloadBytes"] = metrics.EntUsedLogicalPayloadBytes,
                        ["EntSlackLogicalPayloadBytes"] = metrics.EntSlackLogicalPayloadBytes,
                        ["ColumnLogicalPayloadBytes"] = metrics.ColumnLogicalPayloadBytes,
                        ["ComponentLogicalPayloadBytes"] = metrics.ComponentLogicalPayloadBytes,
                        ["ComponentUsedLogicalPayloadBytes"] = metrics.ComponentUsedLogicalPayloadBytes,
                        ["ComponentSlackLogicalPayloadBytes"] = metrics.ComponentSlackLogicalPayloadBytes,
                        ["EstimatedManagedBytes"] = metrics.EstimatedManagedBytes,
                        ["CatalogManagedObjectCount"] = metrics.CatalogManagedObjectCount,
                        ["StorageManagedObjectCount"] = metrics.StorageManagedObjectCount,
                        ["OwnedManagedObjectCount"] = metrics.OwnedManagedObjectCount,
                        ["TotalLogicalRetainedBytes"] = metrics.TotalLogicalRetainedBytes,
                    },
                },
                new JsonSerializerOptions { WriteIndented = true }));
        GC.KeepAlive(ents);
        return 0;
    }
}
