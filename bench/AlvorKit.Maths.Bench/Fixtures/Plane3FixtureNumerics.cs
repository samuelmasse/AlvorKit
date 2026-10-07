namespace AlvorKit;

internal class Plane3FixtureNumerics
{
    private readonly Plane3FixtureNumericsPlaneValues planeValues = new();
    private readonly Plane3FixtureNumericsVector3Values vector3Values = new();
    private readonly Plane3FixtureNumericsVector4Values vector4Values = new();
    private readonly Plane3FixtureNumericsQuaternionValues quaternionValues = new();
    private readonly Plane3FixtureNumericsMatrix4x4Values matrix4x4Values = new();
    internal Plane3FixtureNumericsPlaneValues PlaneValues => planeValues;
    internal Plane3FixtureNumericsVector3Values Vector3Values => vector3Values;
    internal Plane3FixtureNumericsVector4Values Vector4Values => vector4Values;
    internal Plane3FixtureNumericsQuaternionValues QuaternionValues => quaternionValues;
    internal Plane3FixtureNumericsMatrix4x4Values Matrix4x4Values => matrix4x4Values;
}
