namespace AlvorKit;

[Bench]
public class ArchConcreteStructBenchmarks(ArchConcreteStructMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "ConcreteStruct",
        "JIT call shape",
        [
            BenchNode.Measure("ScalarGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructScalarGetOne),
            BenchNode.Measure(
                "ScalarGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructScalarGetRotating),
            BenchNode.Measure("ScalarSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructScalarSetOne),
            BenchNode.Measure(
                "ScalarSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructScalarSetRotating),
            BenchNode.Measure("WideGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructWideGetOne),
            BenchNode.Measure(
                "WideGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructWideGetRotating),
            BenchNode.Measure("WideSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructWideSetOne),
            BenchNode.Measure(
                "WideSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructWideSetRotating),
            BenchNode.Measure("ReferenceGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructReferenceGetOne),
            BenchNode.Measure(
                "ReferenceGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructReferenceGetRotating),
            BenchNode.Measure("ReferenceSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructReferenceSetOne),
            BenchNode.Measure(
                "ReferenceSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructReferenceSetRotating),
            BenchNode.Measure("RefStructGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructRefStructGetOne),
            BenchNode.Measure(
                "RefStructGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructRefStructGetRotating),
            BenchNode.Measure("RefStructSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteStructRefStructSetOne),
            BenchNode.Measure(
                "RefStructSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructRefStructSetRotating),
            BenchNode.Measure(
                "LocRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructLocRotating),
            BenchNode.Measure(
                "DirectoryRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteStructDirectoryRotating)
    ]);
}
