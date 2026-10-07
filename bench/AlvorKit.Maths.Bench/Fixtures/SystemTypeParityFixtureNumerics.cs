namespace AlvorKit;

internal class SystemTypeParityFixtureNumerics
{
    private readonly SystemTypeParityFixtureNumericsMatrix3x2Values matrix3x2Values = new();
    private readonly SystemTypeParityFixtureNumericsMatrix4x4Values matrix4x4Values = new();
    private readonly SystemTypeParityFixtureNumericsQuaternionValues quaternionValues = new();
    private readonly SystemTypeParityFixtureNumericsVector2Values vector2Values = new();
    private readonly SystemTypeParityFixtureNumericsVector3Values vector3Values = new();
    private readonly SystemTypeParityFixtureNumericsVector4Values vector4Values = new();
    internal SystemTypeParityFixtureNumericsMatrix3x2Values Matrix3x2Values => matrix3x2Values;
    internal SystemTypeParityFixtureNumericsMatrix4x4Values Matrix4x4Values => matrix4x4Values;
    internal SystemTypeParityFixtureNumericsQuaternionValues QuaternionValues => quaternionValues;
    internal SystemTypeParityFixtureNumericsVector2Values Vector2Values => vector2Values;
    internal SystemTypeParityFixtureNumericsVector3Values Vector3Values => vector3Values;
    internal SystemTypeParityFixtureNumericsVector4Values Vector4Values => vector4Values;
}
