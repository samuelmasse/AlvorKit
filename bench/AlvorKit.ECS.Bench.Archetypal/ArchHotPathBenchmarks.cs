namespace AlvorKit;

[Bench]
public class ArchHotPathBenchmarks(
    ArchConcreteClassBenchmarks concreteClass,
    ArchConcreteStructBenchmarks concreteStruct,
    ArchGenericClassBenchmarks genericClass,
    ArchGenericStructBenchmarks genericStruct,
    ArchRowsBenchmarks rows)
{
    public BenchNode Create() => BenchNode.Group(
        "HotPath",
        "concrete and generic access",
        [
            concreteClass.Create(),
            concreteStruct.Create(),
            genericClass.Create(),
            genericStruct.Create(),
            rows.Create()
    ]);
}
