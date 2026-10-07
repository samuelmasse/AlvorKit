namespace AlvorKit;

internal class Plane3FixtureAlvor
{
    private readonly Plane3FixtureAlvorPlane3Values plane3Values = new();
    private readonly Plane3FixtureAlvorVec3Values vec3Values = new();
    private readonly Plane3FixtureAlvorVec4Values vec4Values = new();
    private readonly Plane3FixtureAlvorQuatValues quatValues = new();
    private readonly Plane3FixtureAlvorMat4Values mat4Values = new();
    internal Plane3FixtureAlvorPlane3Values Plane3Values => plane3Values;
    internal Plane3FixtureAlvorVec3Values Vec3Values => vec3Values;
    internal Plane3FixtureAlvorVec4Values Vec4Values => vec4Values;
    internal Plane3FixtureAlvorQuatValues QuatValues => quatValues;
    internal Plane3FixtureAlvorMat4Values Mat4Values => mat4Values;
}
