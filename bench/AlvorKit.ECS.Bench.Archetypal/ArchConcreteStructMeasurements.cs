namespace AlvorKit;

[Bench]
public class ArchConcreteStructMeasurements
{
    private object? retained;

    public BenchResult HotConcreteStructScalarGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructScalarGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructScalarGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructScalarGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructScalarSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructScalarSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructScalarSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructScalarSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructWideGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructWideGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructWideGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructWideGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructWideSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructWideSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructWideSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructWideSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructReferenceGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructReferenceGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructReferenceGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructReferenceGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructReferenceSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructReferenceSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructReferenceSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructReferenceSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructRefStructGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructRefStructGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructRefStructGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructRefStructGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructRefStructSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructRefStructSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructRefStructSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteStructRefStructSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteStructLocRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructLocRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteStructDirectoryRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24StructArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteStructDirectoryRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }
}
