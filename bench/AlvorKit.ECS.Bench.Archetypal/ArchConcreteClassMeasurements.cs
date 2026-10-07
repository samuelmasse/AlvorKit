namespace AlvorKit;

[Bench]
public class ArchConcreteClassMeasurements
{
    private object? retained;

    public BenchResult HotConcreteClassScalarGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassScalarGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassScalarGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassScalarGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassScalarSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassScalarSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassScalarSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassScalarSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassWideGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassWideGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassWideGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassWideGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassWideSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassWideSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassWideSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Wide, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassWideSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassReferenceGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassReferenceGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassReferenceGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassReferenceGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassReferenceSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassReferenceSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassReferenceSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Reference, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassReferenceSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassRefStructGetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassRefStructGetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassRefStructGetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassRefStructGetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassRefStructSetOne()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.One, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassRefStructSetOne.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassRefStructSetRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.RefStruct, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        AlvorKit.HotConcreteClassRefStructSetRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = fixture.Ents[^1];
        return result;
    }

    public BenchResult HotConcreteClassLocRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassLocRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }

    public BenchResult HotConcreteClassDirectoryRotating()
    {
        using var fixture = ArchHotFixture.Create<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, false);
        var timer = BenchTimer.Start();
        var observed = AlvorKit.HotConcreteClassDirectoryRotating.Run(fixture, 1048576);
        var result = timer.Stop(1048576, "operation");
        retained = observed;
        return result;
    }
}
