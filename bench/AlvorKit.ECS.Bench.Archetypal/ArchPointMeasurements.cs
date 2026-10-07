namespace AlvorKit;

[Bench]
public class ArchPointMeasurements
{
    private object? retained;

    public BenchResult ScalarGet(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<RunArch>(ent, width);
        var timer = BenchTimer.Start();
        var sum = ArchScalarGet.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult ScalarGetAbsent(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<RunArch>(ent, width);
        GC.KeepAlive(EntArchColumn<int, F32, RunArch>.FieldId);
        var timer = BenchTimer.Start();
        var sum = ArchScalarGetAbsent.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult ScalarHasPresent(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<RunArch>(ent, width);
        var timer = BenchTimer.Start();
        var sum = ArchScalarHasPresent.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult ScalarHasAbsent(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<RunArch>(ent, width);
        GC.KeepAlive(EntArchColumn<int, F32, RunArch>.FieldId);
        var timer = BenchTimer.Start();
        var sum = ArchScalarHasAbsent.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult ScalarSet(int width)
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetWidth<RunArch>(ent, width);
        var timer = BenchTimer.Start();
        ArchScalarSet.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = ent.GetArchetypal<int, F00, RunArch>();
        return result;
    }

    public BenchResult WideGet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var value = new EcsBenchWideValue(1, 2, 3, 4, 5, 6, 7, 8);
        ent.SetArchetypal<EcsBenchWideValue, FWide, RunArch>(value);
        var timer = BenchTimer.Start();
        var sum = ArchWideGet.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult WideSet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var value = new EcsBenchWideValue(1, 2, 3, 4, 5, 6, 7, 8);
        ent.SetArchetypal<EcsBenchWideValue, FWide, RunArch>(value);
        var timer = BenchTimer.Start();
        ArchWideSet.Run<RunArch>(ent, 1048576, value);
        var result = timer.Stop(1048576, "operation");
        retained = ent.GetArchetypal<EcsBenchWideValue, FWide, RunArch>();
        return result;
    }

    public BenchResult ReferenceGet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var value = new EcsBenchReference(17);
        ent.SetArchetypal<EcsBenchReference, FReference, RunArch>(value);
        var timer = BenchTimer.Start();
        var sum = ArchReferenceGet.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult ReferenceSet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var value = new EcsBenchReference(17);
        ent.SetArchetypal<EcsBenchReference, FReference, RunArch>(value);
        var timer = BenchTimer.Start();
        ArchReferenceSet.Run<RunArch>(ent, 1048576, value);
        var result = timer.Stop(1048576, "operation");
        retained = ent.GetArchetypal<EcsBenchReference, FReference, RunArch>();
        return result;
    }

    public BenchResult RefStructGet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var token = new object();
        var value = new EcsBenchRefStruct("bench", token);
        ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, RunArch>(value);
        var timer = BenchTimer.Start();
        var sum = ArchRefStructGet.Run<RunArch>(ent, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = sum;
        return result;
    }

    public BenchResult RefStructSet()
    {
        using var alloc = new EntArena();
        EntMut ent = alloc.Alloc();
        ArchShapes.SetSevenFillers<RunArch>(ent);
        var token = new object();
        var value = new EcsBenchRefStruct("bench", token);
        ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, RunArch>(value);
        var timer = BenchTimer.Start();
        ArchRefStructSet.Run<RunArch>(ent, 1048576, value);
        var result = timer.Stop(1048576, "operation");
        retained = ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, RunArch>();
        return result;
    }
}
