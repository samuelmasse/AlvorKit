namespace AlvorKit;

internal static class RaceAlvorCreate3Measurements
{
    internal static BenchResult SparseMutator(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        var last = RaceAlvorCreate3SparseMutator.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        BenchRetain.Value(in last);
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ArchetypalMutator(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        var last = RaceAlvorCreate3ArchetypalMutator.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        BenchRetain.Value(in last);
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ArchetypalReusedBuilder(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        var last = RaceAlvorCreate3ArchetypalReusedBuilder.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        BenchRetain.Value(in last);
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Sparse(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        RaceAlvorCreate3Sparse.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ArchetypalSetters(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        RaceAlvorCreate3ArchetypalSetters.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult ArchetypalFinalShape(int count, int padding, int passes)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var timer = BenchTimer.Start();
        RaceAlvorCreate3ArchetypalFinalShape.Run(fixture, count, 1);
        var result = timer.Stop(count * 1, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
