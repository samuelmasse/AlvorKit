namespace AlvorKit;

[Bench]
public class ArchConcreteClassBenchmarks(ArchConcreteClassMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "ConcreteClass",
        "JIT call shape",
        [
            BenchNode.Measure("ScalarGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassScalarGetOne),
            BenchNode.Measure(
                "ScalarGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassScalarGetRotating),
            BenchNode.Measure("ScalarSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassScalarSetOne),
            BenchNode.Measure(
                "ScalarSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassScalarSetRotating),
            BenchNode.Measure("WideGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassWideGetOne),
            BenchNode.Measure(
                "WideGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassWideGetRotating),
            BenchNode.Measure("WideSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassWideSetOne),
            BenchNode.Measure(
                "WideSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassWideSetRotating),
            BenchNode.Measure("ReferenceGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassReferenceGetOne),
            BenchNode.Measure(
                "ReferenceGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassReferenceGetRotating),
            BenchNode.Measure("ReferenceSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassReferenceSetOne),
            BenchNode.Measure(
                "ReferenceSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassReferenceSetRotating),
            BenchNode.Measure("RefStructGetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassRefStructGetOne),
            BenchNode.Measure(
                "RefStructGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassRefStructGetRotating),
            BenchNode.Measure("RefStructSetOne", "1,048,576 calls; one Ent", measurements.HotConcreteClassRefStructSetOne),
            BenchNode.Measure(
                "RefStructSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassRefStructSetRotating),
            BenchNode.Measure(
                "LocRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassLocRotating),
            BenchNode.Measure(
                "DirectoryRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotConcreteClassDirectoryRotating)
    ]);
}
