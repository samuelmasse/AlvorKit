namespace AlvorKit;

[Bench]
public class Plane3Measurements
{
    private object? retained;

    public BenchResult NormalizeAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorOutput = fixture.Alvor.Plane3Values.AlvorOutput;
        var timer = BenchTimer.Start();
        Plane3NormalizeAlvor.Run(alvorPlanes, alvorOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorCoefficients = fixture.Alvor.Vec4Values.AlvorCoefficients;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3DotAlvor.Run(alvorPlanes, alvorCoefficients, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult EvaluateAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorVectors = fixture.Alvor.Vec3Values.AlvorVectors;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3EvaluateAlvor.Run(alvorPlanes, alvorVectors, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotNormalAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorVectors = fixture.Alvor.Vec3Values.AlvorVectors;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3DotNormalAlvor.Run(alvorPlanes, alvorVectors, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult TransformQuaternionAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorOutput = fixture.Alvor.Plane3Values.AlvorOutput;
        var alvorRotations = fixture.Alvor.QuatValues.AlvorRotations;
        var timer = BenchTimer.Start();
        Plane3TransformQuaternionAlvor.Run(alvorPlanes, alvorOutput, alvorRotations, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult TransformMatrixAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorOutput = fixture.Alvor.Plane3Values.AlvorOutput;
        var alvorTransforms = fixture.Alvor.Mat4Values.AlvorTransforms;
        var timer = BenchTimer.Start();
        Plane3TransformMatrixAlvor.Run(alvorPlanes, alvorOutput, alvorTransforms, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult CreateFromPointsAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorOutput = fixture.Alvor.Plane3Values.AlvorOutput;
        var alvorPoint0 = fixture.Alvor.Vec3Values.AlvorPoint0;
        var alvorPoint1 = fixture.Alvor.Vec3Values.AlvorPoint1;
        var alvorPoint2 = fixture.Alvor.Vec3Values.AlvorPoint2;
        var timer = BenchTimer.Start();
        Plane3CreateFromPointsAlvor.Run(alvorOutput, alvorPoint0, alvorPoint1, alvorPoint2, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult EqualsAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var alvorOtherPlanes = fixture.Alvor.Plane3Values.AlvorOtherPlanes;
        var intOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Plane3EqualsAlvor.Run(alvorPlanes, alvorOtherPlanes, intOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult GetHashCodeAlvor()
    {
        var fixture = new Plane3Fixture();
        var alvorPlanes = fixture.Alvor.Plane3Values.AlvorPlanes;
        var intOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Plane3GetHashCodeAlvor.Run(alvorPlanes, intOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult NormalizeNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemOutput = fixture.Numerics.PlaneValues.SystemOutput;
        var timer = BenchTimer.Start();
        Plane3NormalizeNumerics.Run(systemPlanes, systemOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemCoefficients = fixture.Numerics.Vector4Values.SystemCoefficients;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3DotNumerics.Run(systemPlanes, systemCoefficients, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult EvaluateNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemVectors = fixture.Numerics.Vector3Values.SystemVectors;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3EvaluateNumerics.Run(systemPlanes, systemVectors, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult DotNormalNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemVectors = fixture.Numerics.Vector3Values.SystemVectors;
        var scalarOutput = fixture.Shared.ScalarOutput;
        var timer = BenchTimer.Start();
        Plane3DotNormalNumerics.Run(systemPlanes, systemVectors, scalarOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult TransformQuaternionNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemOutput = fixture.Numerics.PlaneValues.SystemOutput;
        var systemRotations = fixture.Numerics.QuaternionValues.SystemRotations;
        var timer = BenchTimer.Start();
        Plane3TransformQuaternionNumerics.Run(systemPlanes, systemOutput, systemRotations, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult TransformMatrixNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemOutput = fixture.Numerics.PlaneValues.SystemOutput;
        var systemTransforms = fixture.Numerics.Matrix4x4Values.SystemTransforms;
        var timer = BenchTimer.Start();
        Plane3TransformMatrixNumerics.Run(systemPlanes, systemOutput, systemTransforms, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult CreateFromPointsNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemOutput = fixture.Numerics.PlaneValues.SystemOutput;
        var systemPoint0 = fixture.Numerics.Vector3Values.SystemPoint0;
        var systemPoint1 = fixture.Numerics.Vector3Values.SystemPoint1;
        var systemPoint2 = fixture.Numerics.Vector3Values.SystemPoint2;
        var timer = BenchTimer.Start();
        Plane3CreateFromPointsNumerics.Run(systemOutput, systemPoint0, systemPoint1, systemPoint2, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult EqualsNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var systemOtherPlanes = fixture.Numerics.PlaneValues.SystemOtherPlanes;
        var intOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Plane3EqualsNumerics.Run(systemPlanes, systemOtherPlanes, intOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult GetHashCodeNumerics()
    {
        var fixture = new Plane3Fixture();
        var systemPlanes = fixture.Numerics.PlaneValues.SystemPlanes;
        var intOutput = fixture.Shared.IntOutput;
        var timer = BenchTimer.Start();
        Plane3GetHashCodeNumerics.Run(systemPlanes, intOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
