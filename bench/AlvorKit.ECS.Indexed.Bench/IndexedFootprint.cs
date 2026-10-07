namespace AlvorKit;

/// <summary>Measures a warmed, retained 131,072-Ent fixture in a separate process from timing.</summary>
public static class IndexedFootprint
{
    public static void Run()
    {
        using (var warmup = new IndexedBenchFixture(1024, "dirty"))
            GC.KeepAlive(warmup);

        var retainedBefore = GC.GetTotalMemory(true);
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        using var fixture = new IndexedBenchFixture(131072, "dirty");
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var retained = GC.GetTotalMemory(true) - retainedBefore;
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
        {
            EntCount = fixture.Ents.Length,
            HandleBytes = Unsafe.SizeOf<EntPtrIdx>(),
            HandleContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<EntPtrIdx>(),
            FixtureAllocatedBytes = allocated,
            FixtureRetainedManagedBytes = retained,
        }));
        GC.KeepAlive(fixture);
    }
}
