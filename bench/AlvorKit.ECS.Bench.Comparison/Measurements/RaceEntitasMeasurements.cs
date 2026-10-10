namespace AlvorKit;

internal static class RaceEntitasMeasurements
{
    internal static BenchResult Create1(int count, int padding, int passes)
    {
        using var fixture = new RaceEntitasContext(1);
        var timer = BenchTimer.Start();
        RaceEntitasCreate1Default.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Create2(int count, int padding, int passes)
    {
        using var fixture = new RaceEntitasContext(2);
        var timer = BenchTimer.Start();
        RaceEntitasCreate2Default.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Create3(int count, int padding, int passes)
    {
        using var fixture = new RaceEntitasContext(3);
        var timer = BenchTimer.Start();
        RaceEntitasCreate3Default.Run(fixture, count, 1);
        var result = timer.Stop(count, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Update1(int count, int padding, int passes)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateUpdate(count, padding, 1);
        var timer = BenchTimer.Start();
        RaceEntitasUpdate1GroupDirect.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Update2(int count, int padding, int passes)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateUpdate(count, padding, 2);
        var timer = BenchTimer.Start();
        RaceEntitasUpdate2GroupDirect.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Update3(int count, int padding, int passes)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateUpdate(count, padding, 3);
        var timer = BenchTimer.Start();
        RaceEntitasUpdate3GroupDirect.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }

    internal static BenchResult Mixed(int count, int padding, int passes)
    {
        using var fixture = RaceEntitasUpdateFixture.CreateMixed(count);
        var timer = BenchTimer.Start();
        RaceEntitasMixedGroupDirect.Run(fixture, count, passes);
        var result = timer.Stop(count * passes, "Ent");
        GC.KeepAlive(fixture);
        return result;
    }
}
