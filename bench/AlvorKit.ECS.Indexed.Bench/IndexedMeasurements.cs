namespace AlvorKit;

[Bench]
public class IndexedMeasurements
{
    private const int Count = 1024;
    private const int Passes = 4096;
    private const int LifetimeCount = 131072;
    private const int ContextCount = 32768;

    private long retained;

    public BenchResult ContextRegistration()
    {
        var timer = BenchTimer.Start();
        IndexedContextRegistration.Run(ContextCount);
        return timer.Stop(ContextCount, "context");
    }

    public BenchResult PlainSet()
    {
        using var fixture = new IndexedBenchFixture(Count, "none");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedPlainSet.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult PlainUnsetSet()
    {
        using var fixture = new IndexedBenchFixture(Count, "none");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedPlainUnsetSet.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult AbsentUnset()
    {
        using var fixture = new IndexedBenchFixture(Count, "none");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedAbsentUnset.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult ScalarChanged()
    {
        using var fixture = new IndexedBenchFixture(Count, "dirty");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedScalarChanged.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult ScalarEqual()
    {
        using var fixture = new IndexedBenchFixture(Count, "dirty");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedScalarEqual.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult ArrayPublish()
    {
        using var fixture = new IndexedBenchFixture(Count, "dirty");
        var ents = fixture.Ents;
        var array = fixture.Array;
        var timer = BenchTimer.Start();
        IndexedArrayPublish.Run(ents, Passes, array);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult DirtyReset()
    {
        using var fixture = new IndexedBenchFixture(Count, "dirty");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedDirtyReset.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult GatedToggle()
    {
        using var fixture = new IndexedBenchFixture(Count, "bags");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedGatedToggle.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult KeyMove()
    {
        using var fixture = new IndexedBenchFixture(Count, "key");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedKeyMove.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult WideObserved()
    {
        using var fixture = new IndexedBenchFixture(Count, "wide");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedWideObserved.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<int, BenchPlain>();
        return result;
    }

    public BenchResult WideChanged()
    {
        using var fixture = new IndexedBenchFixture(Count, "wide-change");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedWideChanged.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<BenchWideValue, BenchWide>().H;
        return result;
    }

    public BenchResult WideEqual()
    {
        using var fixture = new IndexedBenchFixture(Count, "wide-change");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedWideEqual.Run(ents, Passes);
        var result = timer.Stop(Count * Passes, "operation");
        retained = fixture.Observed + ents[^1].Get<BenchWideValue, BenchWide>().H;
        return result;
    }

    public BenchResult Clear()
    {
        using var fixture = new IndexedBenchFixture(LifetimeCount, "none");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedClear.Run(ents);
        var result = timer.Stop(LifetimeCount, "Ent");
        retained = fixture.Arena.Allocated;
        return result;
    }

    public BenchResult Dispose()
    {
        using var fixture = new IndexedBenchFixture(LifetimeCount, "none");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedDispose.Run(ents);
        var result = timer.Stop(LifetimeCount, "Ent");
        retained = fixture.Arena.Allocated;
        return result;
    }

    public BenchResult ClearBags()
    {
        using var fixture = new IndexedBenchFixture(LifetimeCount, "bags");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedClearBags.Run(ents);
        var result = timer.Stop(LifetimeCount, "Ent");
        retained = fixture.Arena.Allocated;
        return result;
    }

    public BenchResult DisposeBags()
    {
        using var fixture = new IndexedBenchFixture(LifetimeCount, "bags");
        var ents = fixture.Ents;
        var timer = BenchTimer.Start();
        IndexedDisposeBags.Run(ents);
        var result = timer.Stop(LifetimeCount, "Ent");
        retained = fixture.Arena.Allocated;
        return result;
    }
}
