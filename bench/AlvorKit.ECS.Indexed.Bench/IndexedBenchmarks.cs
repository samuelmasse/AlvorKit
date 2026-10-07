namespace AlvorKit;

[Bench]
public class IndexedBenchmarks(IndexedMeasurements measurements) : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.ECS.Indexed.Bench",
    [
        BenchNode.Measure("ContextRegistration", "create/register/dispose two bags and a scalar reaction",
            measurements.ContextRegistration),
        BenchNode.Measure("PlainSet", "existing sparse Set without observers", measurements.PlainSet),
        BenchNode.Measure("PlainUnsetSet", "present Unset followed by Set; one pair per operation", measurements.PlainUnsetSet),
        BenchNode.Measure("AbsentUnset", "absent sparse Unset", measurements.AbsentUnset),
        BenchNode.Measure("ScalarChanged", "scalar write marks an already-dirty Ent through a nested bag write",
            measurements.ScalarChanged),
        BenchNode.Measure("ScalarEqual", "equal scalar write with dirty tracking; no dirty mark", measurements.ScalarEqual),
        BenchNode.Measure("ArrayPublish", "publish the same array reference; tracker always marks dirty", measurements.ArrayPublish),
        BenchNode.Measure("DirtyReset", "changed scalar marks dirty, then clear the marker; one cycle per operation",
            measurements.DirtyReset),
        BenchNode.Measure("GatedToggle", "remove and re-add gated bag membership; one cycle per operation", measurements.GatedToggle),
        BenchNode.Measure("KeyMove", "unique key moves between two pre-sized dictionary ranges", measurements.KeyMove),
        BenchNode.Measure("WideObserved", "64-byte value write with a read-only reaction", measurements.WideObserved),
        BenchNode.Measure("WideChanged", "64-byte value change; compare once and consume old/new fields", measurements.WideChanged),
        BenchNode.Measure("WideEqual", "equal 64-byte write with change tracking", measurements.WideEqual),
        BenchNode.Measure("Clear", "Clear prepared Ents; seven values, none observers; setup excluded", measurements.Clear),
        BenchNode.Measure("Dispose", "Dispose prepared Ents; seven values, none observers; setup excluded", measurements.Dispose),
        BenchNode.Measure("ClearBags", "Clear prepared Ents; seven values, bags observers; setup excluded", measurements.ClearBags),
        BenchNode.Measure("DisposeBags", "Dispose prepared Ents; seven values, bags observers; setup excluded", measurements.DisposeBags),
    ]);
}
