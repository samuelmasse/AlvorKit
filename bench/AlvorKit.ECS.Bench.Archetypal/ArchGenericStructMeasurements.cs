namespace AlvorKit;

[Bench]
public class ArchGenericStructMeasurements
{
    private object? retained;

    public BenchResult HotGenericScalarGetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericScalarGetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericScalarGetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericScalarGetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericScalarSetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericScalarSetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericScalarSetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericScalarSetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericWideGetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericWideGetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericWideGetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericWideGetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericWideSetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericWideSetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericWideSetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericWideSetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericReferenceGetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericReferenceGetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericReferenceGetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericReferenceGetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericReferenceSetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericReferenceSetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericReferenceSetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericReferenceSetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericRefStructGetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericRefStructGetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericRefStructGetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericRefStructGetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericRefStructSetOneStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericRefStructSetOne.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericRefStructSetRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericRefStructSetRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericLocRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericLocRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericDirectoryRotatingStruct()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericDirectoryRotating.Run<Afr24StructArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }
}
