namespace AlvorKit;

[Bench]
public class ArchGenericStructBenchmarks(ArchGenericStructMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "GenericStruct",
        "JIT call shape",
        [
            BenchNode.Measure("ScalarGetOne", "1,048,576 calls; one Ent", measurements.HotGenericScalarGetOneStruct),
            BenchNode.Measure(
                "ScalarGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericScalarGetRotatingStruct),
            BenchNode.Measure("ScalarSetOne", "1,048,576 calls; one Ent", measurements.HotGenericScalarSetOneStruct),
            BenchNode.Measure(
                "ScalarSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericScalarSetRotatingStruct),
            BenchNode.Measure("WideGetOne", "1,048,576 calls; one Ent", measurements.HotGenericWideGetOneStruct),
            BenchNode.Measure(
                "WideGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericWideGetRotatingStruct),
            BenchNode.Measure("WideSetOne", "1,048,576 calls; one Ent", measurements.HotGenericWideSetOneStruct),
            BenchNode.Measure(
                "WideSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericWideSetRotatingStruct),
            BenchNode.Measure("ReferenceGetOne", "1,048,576 calls; one Ent", measurements.HotGenericReferenceGetOneStruct),
            BenchNode.Measure(
                "ReferenceGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericReferenceGetRotatingStruct),
            BenchNode.Measure("ReferenceSetOne", "1,048,576 calls; one Ent", measurements.HotGenericReferenceSetOneStruct),
            BenchNode.Measure(
                "ReferenceSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericReferenceSetRotatingStruct),
            BenchNode.Measure("RefStructGetOne", "1,048,576 calls; one Ent", measurements.HotGenericRefStructGetOneStruct),
            BenchNode.Measure(
                "RefStructGetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericRefStructGetRotatingStruct),
            BenchNode.Measure("RefStructSetOne", "1,048,576 calls; one Ent", measurements.HotGenericRefStructSetOneStruct),
            BenchNode.Measure(
                "RefStructSetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericRefStructSetRotatingStruct),
            BenchNode.Measure(
                "LocRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericLocRotatingStruct),
            BenchNode.Measure(
                "DirectoryRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotGenericDirectoryRotatingStruct)
    ]);
}
