namespace AlvorKit;

[Bench]
public class ArchBenchmarks(
    ArchPointBenchmarks point,
    ArchConcurrencyBenchmarks concurrency,
    ArchStructuralBenchmarks structural,
    ArchMembershipBenchmarks membership,
    ArchHotPathBenchmarks hotPath) : IBenchSuiteProvider
{
    public BenchSuite Create() => new(
        "AlvorKit.ECS.Bench.Archetypal",
        [point.Create(), concurrency.Create(), structural.Create(), membership.Create(), hotPath.Create()]);
}
