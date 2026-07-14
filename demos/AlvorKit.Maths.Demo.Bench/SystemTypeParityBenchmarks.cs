namespace AlvorKit.Maths.Demo.Bench;

/// <summary>Shared matrix and quaternion operations that also exist in System.Numerics.</summary>
public enum SystemTypeOperation
{
    Matrix3x2Add,
    Matrix3x2Multiply,
    Matrix3x2Invert,
    Matrix3x2Transform,
    Matrix4x4Add,
    Matrix4x4Multiply,
    Matrix4x4Transpose,
    Matrix4x4Invert,
    Matrix4x4Transform,
    QuaternionAdd,
    QuaternionMultiply,
    QuaternionNormalize,
    QuaternionTransform,
}

/// <summary>Compares overlapping AlvorKit and System.Numerics matrix and quaternion throughput.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SystemTypeParityBenchmarks
{
    private const int BatchSize = 4_096;

    private readonly Mat3x2[] alvorMat3Left = new Mat3x2[BatchSize];
    private readonly Mat3x2[] alvorMat3Right = new Mat3x2[BatchSize];
    private readonly Mat3x2[] alvorMat3Output = new Mat3x2[BatchSize];
    private readonly System.Numerics.Matrix3x2[] systemMat3Left = new System.Numerics.Matrix3x2[BatchSize];
    private readonly System.Numerics.Matrix3x2[] systemMat3Right = new System.Numerics.Matrix3x2[BatchSize];
    private readonly System.Numerics.Matrix3x2[] systemMat3Output = new System.Numerics.Matrix3x2[BatchSize];

    private readonly Mat4[] alvorMat4Left = new Mat4[BatchSize];
    private readonly Mat4[] alvorMat4Right = new Mat4[BatchSize];
    private readonly Mat4[] alvorMat4Output = new Mat4[BatchSize];
    private readonly System.Numerics.Matrix4x4[] systemMat4Left = new System.Numerics.Matrix4x4[BatchSize];
    private readonly System.Numerics.Matrix4x4[] systemMat4Right = new System.Numerics.Matrix4x4[BatchSize];
    private readonly System.Numerics.Matrix4x4[] systemMat4Output = new System.Numerics.Matrix4x4[BatchSize];

    private readonly Quat[] alvorQuatLeft = new Quat[BatchSize];
    private readonly Quat[] alvorQuatRight = new Quat[BatchSize];
    private readonly Quat[] alvorQuatOutput = new Quat[BatchSize];
    private readonly System.Numerics.Quaternion[] systemQuatLeft = new System.Numerics.Quaternion[BatchSize];
    private readonly System.Numerics.Quaternion[] systemQuatRight = new System.Numerics.Quaternion[BatchSize];
    private readonly System.Numerics.Quaternion[] systemQuatOutput = new System.Numerics.Quaternion[BatchSize];

    private readonly Vec2[] alvorVec2 = new Vec2[BatchSize];
    private readonly Vec2[] alvorVec2Output = new Vec2[BatchSize];
    private readonly System.Numerics.Vector2[] systemVec2 = new System.Numerics.Vector2[BatchSize];
    private readonly System.Numerics.Vector2[] systemVec2Output = new System.Numerics.Vector2[BatchSize];
    private readonly Vec3[] alvorVec3 = new Vec3[BatchSize];
    private readonly Vec3[] alvorVec3Output = new Vec3[BatchSize];
    private readonly System.Numerics.Vector3[] systemVec3 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemVec3Output = new System.Numerics.Vector3[BatchSize];
    private readonly Vec4[] alvorVec4 = new Vec4[BatchSize];
    private readonly Vec4[] alvorVec4Output = new Vec4[BatchSize];
    private readonly System.Numerics.Vector4[] systemVec4 = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector4[] systemVec4Output = new System.Numerics.Vector4[BatchSize];

    [ParamsAllValues]
    public SystemTypeOperation Operation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            var value = (((index * 17) % 101) - 50) * 0.001f;
            var angle = 0.1f + value;

            alvorMat3Left[index] = Mat3x2.CreateTranslation(new Vec2(1.25f + value, -2.5f)) *
                Mat3x2.CreateRotation(angle) * Mat3x2.CreateScale(new Vec2(1.1f, 0.9f));
            alvorMat3Right[index] = Mat3x2.CreateTranslation(new Vec2(-0.75f, 3.5f + value)) *
                Mat3x2.CreateRotation(-angle) * Mat3x2.CreateScale(new Vec2(0.8f, 1.2f));
            systemMat3Left[index] = (System.Numerics.Matrix3x2)alvorMat3Left[index];
            systemMat3Right[index] = (System.Numerics.Matrix3x2)alvorMat3Right[index];

            alvorMat4Left[index] = Mat4.CreateTranslation(new Vec3(1.25f + value, -2.5f, 0.75f)) *
                Mat4.CreateRotationZ(angle) * Mat4.CreateScale(new Vec3(1.1f, 0.9f, 1.2f));
            alvorMat4Right[index] = Mat4.CreateTranslation(new Vec3(-0.75f, 3.5f + value, -1.25f)) *
                Mat4.CreateRotationY(-angle) * Mat4.CreateScale(new Vec3(0.8f, 1.2f, 0.95f));
            systemMat4Left[index] = System.Numerics.Matrix4x4.Transpose(
                (System.Numerics.Matrix4x4)alvorMat4Left[index]);
            systemMat4Right[index] = System.Numerics.Matrix4x4.Transpose(
                (System.Numerics.Matrix4x4)alvorMat4Right[index]);

            alvorQuatLeft[index] = Quat.CreateFromAxisAngle(Vec3.UnitY, angle);
            alvorQuatRight[index] = Quat.CreateFromAxisAngle(Vec3.UnitZ, -angle * 0.75f);
            systemQuatLeft[index] = (System.Numerics.Quaternion)alvorQuatLeft[index];
            systemQuatRight[index] = (System.Numerics.Quaternion)alvorQuatRight[index];

            alvorVec2[index] = new Vec2(1.5f + value, -0.5f);
            systemVec2[index] = alvorVec2[index];
            alvorVec3[index] = new Vec3(1.5f + value, -0.5f, 2.25f);
            systemVec3[index] = alvorVec3[index];
            alvorVec4[index] = new Vec4(alvorVec3[index], 1f);
            systemVec4[index] = alvorVec4[index];
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void Alvor() => RunAlvor(Operation);

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void Numerics() => RunSystem(Operation);

    private void RunAlvor(SystemTypeOperation operation)
    {
        switch (operation)
        {
            case SystemTypeOperation.Matrix3x2Add:
                for (var i = 0; i < BatchSize; i++) alvorMat3Output[i] = alvorMat3Left[i] + alvorMat3Right[i];
                break;
            case SystemTypeOperation.Matrix3x2Multiply:
                for (var i = 0; i < BatchSize; i++) alvorMat3Output[i] = alvorMat3Left[i] * alvorMat3Right[i];
                break;
            case SystemTypeOperation.Matrix3x2Invert:
                for (var i = 0; i < BatchSize; i++) Mat3x2.TryInvert(alvorMat3Left[i], out alvorMat3Output[i]);
                break;
            case SystemTypeOperation.Matrix3x2Transform:
                for (var i = 0; i < BatchSize; i++) alvorVec2Output[i] = Mat3x2.TransformPoint(alvorMat3Left[i], alvorVec2[i]);
                break;
            case SystemTypeOperation.Matrix4x4Add:
                for (var i = 0; i < BatchSize; i++) alvorMat4Output[i] = alvorMat4Left[i] + alvorMat4Right[i];
                break;
            case SystemTypeOperation.Matrix4x4Multiply:
                for (var i = 0; i < BatchSize; i++) alvorMat4Output[i] = alvorMat4Left[i] * alvorMat4Right[i];
                break;
            case SystemTypeOperation.Matrix4x4Transpose:
                for (var i = 0; i < BatchSize; i++) alvorMat4Output[i] = Mat4.Transpose(alvorMat4Left[i]);
                break;
            case SystemTypeOperation.Matrix4x4Invert:
                for (var i = 0; i < BatchSize; i++) Mat4.TryInvert(alvorMat4Left[i], out alvorMat4Output[i]);
                break;
            case SystemTypeOperation.Matrix4x4Transform:
                for (var i = 0; i < BatchSize; i++) alvorVec4Output[i] = alvorMat4Left[i] * alvorVec4[i];
                break;
            case SystemTypeOperation.QuaternionAdd:
                for (var i = 0; i < BatchSize; i++) alvorQuatOutput[i] = alvorQuatLeft[i] + alvorQuatRight[i];
                break;
            case SystemTypeOperation.QuaternionMultiply:
                for (var i = 0; i < BatchSize; i++) alvorQuatOutput[i] = alvorQuatLeft[i] * alvorQuatRight[i];
                break;
            case SystemTypeOperation.QuaternionNormalize:
                for (var i = 0; i < BatchSize; i++) alvorQuatOutput[i] = Quat.Normalize(alvorQuatLeft[i]);
                break;
            case SystemTypeOperation.QuaternionTransform:
                for (var i = 0; i < BatchSize; i++) alvorVec3Output[i] = Quat.TransformVector(alvorQuatLeft[i], alvorVec3[i]);
                break;
        }
    }

    private void RunSystem(SystemTypeOperation operation)
    {
        switch (operation)
        {
            case SystemTypeOperation.Matrix3x2Add:
                for (var i = 0; i < BatchSize; i++) systemMat3Output[i] = systemMat3Left[i] + systemMat3Right[i];
                break;
            case SystemTypeOperation.Matrix3x2Multiply:
                for (var i = 0; i < BatchSize; i++) systemMat3Output[i] = systemMat3Right[i] * systemMat3Left[i];
                break;
            case SystemTypeOperation.Matrix3x2Invert:
                for (var i = 0; i < BatchSize; i++) System.Numerics.Matrix3x2.Invert(systemMat3Left[i], out systemMat3Output[i]);
                break;
            case SystemTypeOperation.Matrix3x2Transform:
                for (var i = 0; i < BatchSize; i++) systemVec2Output[i] = System.Numerics.Vector2.Transform(systemVec2[i], systemMat3Left[i]);
                break;
            case SystemTypeOperation.Matrix4x4Add:
                for (var i = 0; i < BatchSize; i++) systemMat4Output[i] = systemMat4Left[i] + systemMat4Right[i];
                break;
            case SystemTypeOperation.Matrix4x4Multiply:
                for (var i = 0; i < BatchSize; i++) systemMat4Output[i] = systemMat4Right[i] * systemMat4Left[i];
                break;
            case SystemTypeOperation.Matrix4x4Transpose:
                for (var i = 0; i < BatchSize; i++) systemMat4Output[i] = System.Numerics.Matrix4x4.Transpose(systemMat4Left[i]);
                break;
            case SystemTypeOperation.Matrix4x4Invert:
                for (var i = 0; i < BatchSize; i++) System.Numerics.Matrix4x4.Invert(systemMat4Left[i], out systemMat4Output[i]);
                break;
            case SystemTypeOperation.Matrix4x4Transform:
                for (var i = 0; i < BatchSize; i++) systemVec4Output[i] = System.Numerics.Vector4.Transform(systemVec4[i], systemMat4Left[i]);
                break;
            case SystemTypeOperation.QuaternionAdd:
                for (var i = 0; i < BatchSize; i++) systemQuatOutput[i] = systemQuatLeft[i] + systemQuatRight[i];
                break;
            case SystemTypeOperation.QuaternionMultiply:
                for (var i = 0; i < BatchSize; i++) systemQuatOutput[i] = systemQuatLeft[i] * systemQuatRight[i];
                break;
            case SystemTypeOperation.QuaternionNormalize:
                for (var i = 0; i < BatchSize; i++) systemQuatOutput[i] = System.Numerics.Quaternion.Normalize(systemQuatLeft[i]);
                break;
            case SystemTypeOperation.QuaternionTransform:
                for (var i = 0; i < BatchSize; i++) systemVec3Output[i] = System.Numerics.Vector3.Transform(systemVec3[i], systemQuatLeft[i]);
                break;
        }
    }
}
