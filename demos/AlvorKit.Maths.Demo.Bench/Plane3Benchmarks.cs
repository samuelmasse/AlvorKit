namespace AlvorKit.Maths.Demo.Bench;

/// <summary>Operations shared by Plane3 and System.Numerics.Plane.</summary>
public enum Plane3Operation
{
    Normalize,
    Dot,
    Evaluate,
    DotNormal,
    TransformQuaternion,
    TransformMatrix,
    CreateFromPoints,
    Equals,
    GetHashCode,
}

/// <summary>Compares overlapping AlvorKit and System.Numerics plane throughput.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Plane3Benchmarks
{
    private const int BatchSize = 4_096;

    private readonly Plane3[] alvorPlanes = new Plane3[BatchSize];
    private readonly Plane3[] alvorOtherPlanes = new Plane3[BatchSize];
    private readonly Plane3[] alvorOutput = new Plane3[BatchSize];
    private readonly Vec3[] alvorVectors = new Vec3[BatchSize];
    private readonly Vec4[] alvorCoefficients = new Vec4[BatchSize];
    private readonly Vec3[] alvorPoint0 = new Vec3[BatchSize];
    private readonly Vec3[] alvorPoint1 = new Vec3[BatchSize];
    private readonly Vec3[] alvorPoint2 = new Vec3[BatchSize];
    private readonly Quat[] alvorRotations = new Quat[BatchSize];
    private readonly Mat4[] alvorTransforms = new Mat4[BatchSize];

    private readonly System.Numerics.Plane[] systemPlanes = new System.Numerics.Plane[BatchSize];
    private readonly System.Numerics.Plane[] systemOtherPlanes = new System.Numerics.Plane[BatchSize];
    private readonly System.Numerics.Plane[] systemOutput = new System.Numerics.Plane[BatchSize];
    private readonly System.Numerics.Vector3[] systemVectors = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector4[] systemCoefficients = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector3[] systemPoint0 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemPoint1 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemPoint2 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Quaternion[] systemRotations = new System.Numerics.Quaternion[BatchSize];
    private readonly System.Numerics.Matrix4x4[] systemTransforms = new System.Numerics.Matrix4x4[BatchSize];

    private readonly float[] scalarOutput = new float[BatchSize];
    private readonly int[] intOutput = new int[BatchSize];

    [ParamsAllValues]
    public Plane3Operation Operation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            var value = (((index * 17) % 101) - 50) * 0.001f;
            var angle = 0.1f + value;

            var plane = new Plane3(new Vec3(1.25f + value, -2.5f + (value * 0.25f), 3.125f - value), 0.75f + value);
            alvorPlanes[index] = plane;
            alvorOtherPlanes[index] = (index & 1) == 0 ? plane : new Plane3(plane.Normal, plane.Offset + 0.5f);
            alvorVectors[index] = new Vec3(-1.5f + value, 0.5f - value, 2.25f + value);
            alvorCoefficients[index] = new Vec4(alvorVectors[index], 1f + value);

            var point0 = new Vec3(value, 1f + value, -2f);
            alvorPoint0[index] = point0;
            alvorPoint1[index] = point0 + new Vec3(1.25f, 0.25f, 0.5f);
            alvorPoint2[index] = point0 + new Vec3(-0.5f, 1.5f, 0.75f);
            alvorRotations[index] = Quat.CreateFromAxisAngle(Vec3.UnitY, angle);
            alvorTransforms[index] = Mat4.CreateTranslation(new Vec3(1.25f + value, -2.5f, 0.75f)) *
                Mat4.CreateRotationZ(angle) * Mat4.CreateScale(new Vec3(1.1f, 0.9f, 1.2f));

            systemPlanes[index] = plane;
            systemOtherPlanes[index] = alvorOtherPlanes[index];
            systemVectors[index] = alvorVectors[index];
            systemCoefficients[index] = alvorCoefficients[index];
            systemPoint0[index] = alvorPoint0[index];
            systemPoint1[index] = alvorPoint1[index];
            systemPoint2[index] = alvorPoint2[index];
            systemRotations[index] = alvorRotations[index];
            systemTransforms[index] = (System.Numerics.Matrix4x4)alvorTransforms[index];
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void Alvor() => RunAlvor(Operation);

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void Numerics() => RunSystem(Operation);

    private void RunAlvor(Plane3Operation operation)
    {
        switch (operation)
        {
            case Plane3Operation.Normalize:
                for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Plane3.Normalize(alvorPlanes[i]);
                break;
            case Plane3Operation.Dot:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = Plane3.Dot(alvorPlanes[i], alvorCoefficients[i]);
                break;
            case Plane3Operation.Evaluate:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = Plane3.Evaluate(alvorPlanes[i], alvorVectors[i]);
                break;
            case Plane3Operation.DotNormal:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = Plane3.DotNormal(alvorPlanes[i], alvorVectors[i]);
                break;
            case Plane3Operation.TransformQuaternion:
                for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Plane3.Transform(alvorPlanes[i], alvorRotations[i]);
                break;
            case Plane3Operation.TransformMatrix:
                for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Plane3.Transform(alvorPlanes[i], alvorTransforms[i]);
                break;
            case Plane3Operation.CreateFromPoints:
                for (var i = 0; i < BatchSize; i++)
                    alvorOutput[i] = Plane3.CreateFromPoints(alvorPoint0[i], alvorPoint1[i], alvorPoint2[i]);
                break;
            case Plane3Operation.Equals:
                for (var i = 0; i < BatchSize; i++) intOutput[i] = alvorPlanes[i].Equals(alvorOtherPlanes[i]) ? 1 : 0;
                break;
            case Plane3Operation.GetHashCode:
                for (var i = 0; i < BatchSize; i++) intOutput[i] = alvorPlanes[i].GetHashCode();
                break;
        }
    }

    private void RunSystem(Plane3Operation operation)
    {
        switch (operation)
        {
            case Plane3Operation.Normalize:
                for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Plane.Normalize(systemPlanes[i]);
                break;
            case Plane3Operation.Dot:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = System.Numerics.Plane.Dot(systemPlanes[i], systemCoefficients[i]);
                break;
            case Plane3Operation.Evaluate:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = System.Numerics.Plane.DotCoordinate(systemPlanes[i], systemVectors[i]);
                break;
            case Plane3Operation.DotNormal:
                for (var i = 0; i < BatchSize; i++) scalarOutput[i] = System.Numerics.Plane.DotNormal(systemPlanes[i], systemVectors[i]);
                break;
            case Plane3Operation.TransformQuaternion:
                for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Plane.Transform(systemPlanes[i], systemRotations[i]);
                break;
            case Plane3Operation.TransformMatrix:
                for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Plane.Transform(systemPlanes[i], systemTransforms[i]);
                break;
            case Plane3Operation.CreateFromPoints:
                for (var i = 0; i < BatchSize; i++)
                    systemOutput[i] = System.Numerics.Plane.CreateFromVertices(systemPoint0[i], systemPoint1[i], systemPoint2[i]);
                break;
            case Plane3Operation.Equals:
                for (var i = 0; i < BatchSize; i++) intOutput[i] = systemPlanes[i].Equals(systemOtherPlanes[i]) ? 1 : 0;
                break;
            case Plane3Operation.GetHashCode:
                for (var i = 0; i < BatchSize; i++) intOutput[i] = systemPlanes[i].GetHashCode();
                break;
        }
    }
}
