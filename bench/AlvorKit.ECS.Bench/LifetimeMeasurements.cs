namespace AlvorKit;

[Bench]
public class LifetimeMeasurements
{
    private int value;
    private EntHandle handle;

    public BenchResult ComponentSetExisting()
    {
        using var ent = new EntPtr
        {
            LifetimeFirst = 17
        };
        var timer = BenchTimer.Start();
        var observed = LifetimeComponentSetExisting.Run(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        value = observed;
        return result;
    }

    public BenchResult ComponentGetExisting()
    {
        using var ent = new EntPtr
        {
            LifetimeFirst = 17
        };
        var timer = BenchTimer.Start();
        var observed = LifetimeComponentGetExisting.Run(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        value = observed;
        return result;
    }

    public BenchResult ComponentHasExisting()
    {
        using var ent = new EntPtr
        {
            LifetimeFirst = 17
        };
        var timer = BenchTimer.Start();
        var observed = LifetimeComponentHasExisting.Run(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        value = observed;
        return result;
    }

    public BenchResult ComponentUnsetSet()
    {
        using var ent = new EntPtr
        {
            LifetimeFirst = 17
        };
        var timer = BenchTimer.Start();
        var observed = LifetimeComponentUnsetSet.Run(ent, 1048576);
        var result = timer.Stop(1048576, "cycle");
        value = observed;
        return result;
    }

    public BenchResult EntPtrAllocSetDispose()
    {
        var timer = BenchTimer.Start();
        var observed = LifetimeEntPtrAllocSetDispose.Run(1048576);
        var result = timer.Stop(1048576, "cycle");
        handle = observed;
        return result;
    }

    public BenchResult ArenaAllocSetDispose()
    {
        var timer = BenchTimer.Start();
        var observed = LifetimeArenaAllocSetDispose.Run(1048576);
        var result = timer.Stop(1048576, "cycle");
        handle = observed;
        return result;
    }

    public BenchResult ArenaAllocSetBulkDispose()
    {
        var timer = BenchTimer.Start();
        var observed = LifetimeArenaAllocSetBulkDispose.Run(1048576);
        var result = timer.Stop(1048576, "cycle");
        handle = observed;
        return result;
    }
}
