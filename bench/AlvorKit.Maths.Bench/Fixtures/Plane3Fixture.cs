namespace AlvorKit;

internal class Plane3Fixture
{
    private const int BatchSize = 4096;

    private readonly Plane3FixtureAlvor alvor = new();
    private readonly Plane3FixtureNumerics numerics = new();
    private readonly Plane3FixtureShared shared = new();
    internal Plane3FixtureAlvor Alvor => alvor;
    internal Plane3FixtureNumerics Numerics => numerics;
    internal Plane3FixtureShared Shared => shared;

    internal Plane3Fixture()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            var value = (((index * 17) % 101) - 50) * 0.001f;
            var angle = 0.1f + value;
            var plane = new Plane3(new Vec3(1.25f + value, -2.5f + (value * 0.25f), 3.125f - value), 0.75f + value);
            Alvor.Plane3Values.AlvorPlanes[index] = plane;
            Alvor.Plane3Values.AlvorOtherPlanes[index] = (index & 1) == 0 ? plane : new Plane3(plane.Normal, plane.Offset + 0.5f);
            Alvor.Vec3Values.AlvorVectors[index] = new Vec3(-1.5f + value, 0.5f - value, 2.25f + value);
            Alvor.Vec4Values.AlvorCoefficients[index] = new Vec4(Alvor.Vec3Values.AlvorVectors[index], 1f + value);
            var point0 = new Vec3(value, 1f + value, -2f);
            Alvor.Vec3Values.AlvorPoint0[index] = point0;
            Alvor.Vec3Values.AlvorPoint1[index] = point0 + new Vec3(1.25f, 0.25f, 0.5f);
            Alvor.Vec3Values.AlvorPoint2[index] = point0 + new Vec3(-0.5f, 1.5f, 0.75f);
            Alvor.QuatValues.AlvorRotations[index] = Quat.CreateFromAxisAngle(Vec3.UnitY, angle);
            var AlvorTransformsPose = Mat4.CreateTranslation(new Vec3(1.25f + value, -2.5f, 0.75f)) * Mat4.CreateRotationZ(angle);
            Alvor.Mat4Values.AlvorTransforms[index] = AlvorTransformsPose * Mat4.CreateScale(new Vec3(1.1f, 0.9f, 1.2f));
            Numerics.PlaneValues.SystemPlanes[index] = plane;
            Numerics.PlaneValues.SystemOtherPlanes[index] = Alvor.Plane3Values.AlvorOtherPlanes[index];
            Numerics.Vector3Values.SystemVectors[index] = Alvor.Vec3Values.AlvorVectors[index];
            Numerics.Vector4Values.SystemCoefficients[index] = Alvor.Vec4Values.AlvorCoefficients[index];
            Numerics.Vector3Values.SystemPoint0[index] = Alvor.Vec3Values.AlvorPoint0[index];
            Numerics.Vector3Values.SystemPoint1[index] = Alvor.Vec3Values.AlvorPoint1[index];
            Numerics.Vector3Values.SystemPoint2[index] = Alvor.Vec3Values.AlvorPoint2[index];
            Numerics.QuaternionValues.SystemRotations[index] = Alvor.QuatValues.AlvorRotations[index];
            Numerics.Matrix4x4Values.SystemTransforms[index] = (System.Numerics.Matrix4x4)Alvor.Mat4Values.AlvorTransforms[index];
        }
    }
}
