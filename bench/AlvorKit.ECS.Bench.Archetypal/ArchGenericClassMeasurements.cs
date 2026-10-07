namespace AlvorKit;

[Bench]
public class ArchGenericClassMeasurements
{
    private object? retained;

    public BenchResult HotGenericScalarGetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericScalarGetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericScalarGetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericScalarGetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericScalarSetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericScalarSetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericScalarSetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericScalarSetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericWideGetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericWideGetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericWideGetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericWideGetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericWideSetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericWideSetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericWideSetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericWideSetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericReferenceGetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericReferenceGetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericReferenceGetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericReferenceGetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericReferenceSetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericReferenceSetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericReferenceSetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericReferenceSetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericRefStructGetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericRefStructGetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericRefStructGetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericRefStructGetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericRefStructSetOneClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericRefStructSetOne.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericRefStructSetRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotGenericRefStructSetRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotGenericLocRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericLocRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotGenericDirectoryRotatingClass()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotGenericDirectoryRotating.Run<Afr24ClassArch>(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }
}
