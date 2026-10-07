namespace AlvorKit;

internal class SystemTypeParityFixtureAlvor
{
    private readonly SystemTypeParityFixtureAlvorMat3x2Values mat3x2Values = new();
    private readonly SystemTypeParityFixtureAlvorMat4Values mat4Values = new();
    private readonly SystemTypeParityFixtureAlvorQuatValues quatValues = new();
    private readonly SystemTypeParityFixtureAlvorVec2Values vec2Values = new();
    private readonly SystemTypeParityFixtureAlvorVec3Values vec3Values = new();
    private readonly SystemTypeParityFixtureAlvorVec4Values vec4Values = new();
    internal SystemTypeParityFixtureAlvorMat3x2Values Mat3x2Values => mat3x2Values;
    internal SystemTypeParityFixtureAlvorMat4Values Mat4Values => mat4Values;
    internal SystemTypeParityFixtureAlvorQuatValues QuatValues => quatValues;
    internal SystemTypeParityFixtureAlvorVec2Values Vec2Values => vec2Values;
    internal SystemTypeParityFixtureAlvorVec3Values Vec3Values => vec3Values;
    internal SystemTypeParityFixtureAlvorVec4Values Vec4Values => vec4Values;
}
