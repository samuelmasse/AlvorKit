namespace AlvorKit;

[Bench]
public class SystemTypeParityMeasurements
{
    private object? retained;

    public BenchResult Matrix3x2AddAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat3Left = fixture.Alvor.Mat3x2Values.AlvorMat3Left;
        var alvorMat3Right = fixture.Alvor.Mat3x2Values.AlvorMat3Right;
        var alvorMat3Output = fixture.Alvor.Mat3x2Values.AlvorMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2AddAlvor.Run(alvorMat3Left, alvorMat3Right, alvorMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2MultiplyAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat3Left = fixture.Alvor.Mat3x2Values.AlvorMat3Left;
        var alvorMat3Right = fixture.Alvor.Mat3x2Values.AlvorMat3Right;
        var alvorMat3Output = fixture.Alvor.Mat3x2Values.AlvorMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2MultiplyAlvor.Run(alvorMat3Left, alvorMat3Right, alvorMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2InvertAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat3Left = fixture.Alvor.Mat3x2Values.AlvorMat3Left;
        var alvorMat3Output = fixture.Alvor.Mat3x2Values.AlvorMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2InvertAlvor.Run(alvorMat3Left, alvorMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2TransformAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat3Left = fixture.Alvor.Mat3x2Values.AlvorMat3Left;
        var alvorVec2 = fixture.Alvor.Vec2Values.AlvorVec2;
        var alvorVec2Output = fixture.Alvor.Vec2Values.AlvorVec2Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2TransformAlvor.Run(alvorMat3Left, alvorVec2, alvorVec2Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4AddAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat4Left = fixture.Alvor.Mat4Values.AlvorMat4Left;
        var alvorMat4Right = fixture.Alvor.Mat4Values.AlvorMat4Right;
        var alvorMat4Output = fixture.Alvor.Mat4Values.AlvorMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4AddAlvor.Run(alvorMat4Left, alvorMat4Right, alvorMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4MultiplyAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat4Left = fixture.Alvor.Mat4Values.AlvorMat4Left;
        var alvorMat4Right = fixture.Alvor.Mat4Values.AlvorMat4Right;
        var alvorMat4Output = fixture.Alvor.Mat4Values.AlvorMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4MultiplyAlvor.Run(alvorMat4Left, alvorMat4Right, alvorMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4TransposeAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat4Left = fixture.Alvor.Mat4Values.AlvorMat4Left;
        var alvorMat4Output = fixture.Alvor.Mat4Values.AlvorMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4TransposeAlvor.Run(alvorMat4Left, alvorMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4InvertAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat4Left = fixture.Alvor.Mat4Values.AlvorMat4Left;
        var alvorMat4Output = fixture.Alvor.Mat4Values.AlvorMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4InvertAlvor.Run(alvorMat4Left, alvorMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4TransformAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorMat4Left = fixture.Alvor.Mat4Values.AlvorMat4Left;
        var alvorVec4 = fixture.Alvor.Vec4Values.AlvorVec4;
        var alvorVec4Output = fixture.Alvor.Vec4Values.AlvorVec4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4TransformAlvor.Run(alvorMat4Left, alvorVec4, alvorVec4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionAddAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorQuatLeft = fixture.Alvor.QuatValues.AlvorQuatLeft;
        var alvorQuatRight = fixture.Alvor.QuatValues.AlvorQuatRight;
        var alvorQuatOutput = fixture.Alvor.QuatValues.AlvorQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionAddAlvor.Run(alvorQuatLeft, alvorQuatRight, alvorQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionMultiplyAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorQuatLeft = fixture.Alvor.QuatValues.AlvorQuatLeft;
        var alvorQuatRight = fixture.Alvor.QuatValues.AlvorQuatRight;
        var alvorQuatOutput = fixture.Alvor.QuatValues.AlvorQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionMultiplyAlvor.Run(alvorQuatLeft, alvorQuatRight, alvorQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionNormalizeAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorQuatLeft = fixture.Alvor.QuatValues.AlvorQuatLeft;
        var alvorQuatOutput = fixture.Alvor.QuatValues.AlvorQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionNormalizeAlvor.Run(alvorQuatLeft, alvorQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionTransformAlvor()
    {
        var fixture = new SystemTypeParityFixture();
        var alvorQuatLeft = fixture.Alvor.QuatValues.AlvorQuatLeft;
        var alvorVec3 = fixture.Alvor.Vec3Values.AlvorVec3;
        var alvorVec3Output = fixture.Alvor.Vec3Values.AlvorVec3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionTransformAlvor.Run(alvorQuatLeft, alvorVec3, alvorVec3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2AddNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat3Left = fixture.Numerics.Matrix3x2Values.SystemMat3Left;
        var systemMat3Right = fixture.Numerics.Matrix3x2Values.SystemMat3Right;
        var systemMat3Output = fixture.Numerics.Matrix3x2Values.SystemMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2AddNumerics.Run(systemMat3Left, systemMat3Right, systemMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2MultiplyNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat3Left = fixture.Numerics.Matrix3x2Values.SystemMat3Left;
        var systemMat3Right = fixture.Numerics.Matrix3x2Values.SystemMat3Right;
        var systemMat3Output = fixture.Numerics.Matrix3x2Values.SystemMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2MultiplyNumerics.Run(systemMat3Left, systemMat3Right, systemMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2InvertNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat3Left = fixture.Numerics.Matrix3x2Values.SystemMat3Left;
        var systemMat3Output = fixture.Numerics.Matrix3x2Values.SystemMat3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2InvertNumerics.Run(systemMat3Left, systemMat3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix3x2TransformNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat3Left = fixture.Numerics.Matrix3x2Values.SystemMat3Left;
        var systemVec2 = fixture.Numerics.Vector2Values.SystemVec2;
        var systemVec2Output = fixture.Numerics.Vector2Values.SystemVec2Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix3x2TransformNumerics.Run(systemMat3Left, systemVec2, systemVec2Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4AddNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat4Left = fixture.Numerics.Matrix4x4Values.SystemMat4Left;
        var systemMat4Right = fixture.Numerics.Matrix4x4Values.SystemMat4Right;
        var systemMat4Output = fixture.Numerics.Matrix4x4Values.SystemMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4AddNumerics.Run(systemMat4Left, systemMat4Right, systemMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4MultiplyNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat4Left = fixture.Numerics.Matrix4x4Values.SystemMat4Left;
        var systemMat4Right = fixture.Numerics.Matrix4x4Values.SystemMat4Right;
        var systemMat4Output = fixture.Numerics.Matrix4x4Values.SystemMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4MultiplyNumerics.Run(systemMat4Left, systemMat4Right, systemMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4TransposeNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat4Left = fixture.Numerics.Matrix4x4Values.SystemMat4Left;
        var systemMat4Output = fixture.Numerics.Matrix4x4Values.SystemMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4TransposeNumerics.Run(systemMat4Left, systemMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4InvertNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat4Left = fixture.Numerics.Matrix4x4Values.SystemMat4Left;
        var systemMat4Output = fixture.Numerics.Matrix4x4Values.SystemMat4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4InvertNumerics.Run(systemMat4Left, systemMat4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Matrix4x4TransformNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemMat4Left = fixture.Numerics.Matrix4x4Values.SystemMat4Left;
        var systemVec4 = fixture.Numerics.Vector4Values.SystemVec4;
        var systemVec4Output = fixture.Numerics.Vector4Values.SystemVec4Output;
        var timer = BenchTimer.Start();
        SystemTypeParityMatrix4x4TransformNumerics.Run(systemMat4Left, systemVec4, systemVec4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionAddNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemQuatLeft = fixture.Numerics.QuaternionValues.SystemQuatLeft;
        var systemQuatRight = fixture.Numerics.QuaternionValues.SystemQuatRight;
        var systemQuatOutput = fixture.Numerics.QuaternionValues.SystemQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionAddNumerics.Run(systemQuatLeft, systemQuatRight, systemQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionMultiplyNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemQuatLeft = fixture.Numerics.QuaternionValues.SystemQuatLeft;
        var systemQuatRight = fixture.Numerics.QuaternionValues.SystemQuatRight;
        var systemQuatOutput = fixture.Numerics.QuaternionValues.SystemQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionMultiplyNumerics.Run(systemQuatLeft, systemQuatRight, systemQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionNormalizeNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemQuatLeft = fixture.Numerics.QuaternionValues.SystemQuatLeft;
        var systemQuatOutput = fixture.Numerics.QuaternionValues.SystemQuatOutput;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionNormalizeNumerics.Run(systemQuatLeft, systemQuatOutput, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult QuaternionTransformNumerics()
    {
        var fixture = new SystemTypeParityFixture();
        var systemQuatLeft = fixture.Numerics.QuaternionValues.SystemQuatLeft;
        var systemVec3 = fixture.Numerics.Vector3Values.SystemVec3;
        var systemVec3Output = fixture.Numerics.Vector3Values.SystemVec3Output;
        var timer = BenchTimer.Start();
        SystemTypeParityQuaternionTransformNumerics.Run(systemQuatLeft, systemVec3, systemVec3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
