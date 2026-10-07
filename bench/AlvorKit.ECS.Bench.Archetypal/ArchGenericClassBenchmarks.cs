namespace AlvorKit;

[Bench]
public class ArchGenericClassBenchmarks(ArchGenericClassMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "GenericClass",
        "JIT call shape",
        [
            BenchNode.Measure("ScalarGetOne", "1,048,576 calls; one Ent", measurements.HotGenericScalarGetOneClass),
            BenchNode.Measure(
                "ScalarGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericScalarGetRotatingClass),
            BenchNode.Measure("ScalarSetOne", "1,048,576 calls; one Ent", measurements.HotGenericScalarSetOneClass),
            BenchNode.Measure(
                "ScalarSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericScalarSetRotatingClass),
            BenchNode.Measure("WideGetOne", "1,048,576 calls; one Ent", measurements.HotGenericWideGetOneClass),
            BenchNode.Measure(
                "WideGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericWideGetRotatingClass),
            BenchNode.Measure("WideSetOne", "1,048,576 calls; one Ent", measurements.HotGenericWideSetOneClass),
            BenchNode.Measure(
                "WideSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericWideSetRotatingClass),
            BenchNode.Measure("ReferenceGetOne", "1,048,576 calls; one Ent", measurements.HotGenericReferenceGetOneClass),
            BenchNode.Measure(
                "ReferenceGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericReferenceGetRotatingClass),
            BenchNode.Measure("ReferenceSetOne", "1,048,576 calls; one Ent", measurements.HotGenericReferenceSetOneClass),
            BenchNode.Measure(
                "ReferenceSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericReferenceSetRotatingClass),
            BenchNode.Measure("RefStructGetOne", "1,048,576 calls; one Ent", measurements.HotGenericRefStructGetOneClass),
            BenchNode.Measure(
                "RefStructGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericRefStructGetRotatingClass),
            BenchNode.Measure("RefStructSetOne", "1,048,576 calls; one Ent", measurements.HotGenericRefStructSetOneClass),
            BenchNode.Measure(
                "RefStructSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericRefStructSetRotatingClass),
            BenchNode.Measure(
                "LocRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericLocRotatingClass),
            BenchNode.Measure(
                "DirectoryRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericDirectoryRotatingClass)
    ]);
}
