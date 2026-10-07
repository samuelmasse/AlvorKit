namespace AlvorKit;

internal class SystemTypeParityFixture
{
    private const int BatchSize = 4096;

    private readonly SystemTypeParityFixtureAlvor alvor = new();
    private readonly SystemTypeParityFixtureNumerics numerics = new();
    internal SystemTypeParityFixtureAlvor Alvor => alvor;
    internal SystemTypeParityFixtureNumerics Numerics => numerics;

    internal SystemTypeParityFixture()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            var value = (((index * 17) % 101) - 50) * 0.001f;
            var angle = 0.1f + value;
            var AlvorMat3LeftPose = Mat3x2.CreateTranslation(new Vec2(1.25f + value, -2.5f)) * Mat3x2.CreateRotation(angle);
            Alvor.Mat3x2Values.AlvorMat3Left[index] = AlvorMat3LeftPose * Mat3x2.CreateScale(new Vec2(1.1f, 0.9f));
            var AlvorMat3RightPose = Mat3x2.CreateTranslation(new Vec2(-0.75f, 3.5f + value)) * Mat3x2.CreateRotation(-angle);
            Alvor.Mat3x2Values.AlvorMat3Right[index] = AlvorMat3RightPose * Mat3x2.CreateScale(new Vec2(0.8f, 1.2f));
            Numerics.Matrix3x2Values.SystemMat3Left[index] = (System.Numerics.Matrix3x2)Alvor.Mat3x2Values.AlvorMat3Left[index];
            Numerics.Matrix3x2Values.SystemMat3Right[index] = (System.Numerics.Matrix3x2)Alvor.Mat3x2Values.AlvorMat3Right[index];
            var AlvorMat4LeftPose = Mat4.CreateTranslation(new Vec3(1.25f + value, -2.5f, 0.75f)) * Mat4.CreateRotationZ(angle);
            Alvor.Mat4Values.AlvorMat4Left[index] = AlvorMat4LeftPose * Mat4.CreateScale(new Vec3(1.1f, 0.9f, 1.2f));
            var AlvorMat4RightPose = Mat4.CreateTranslation(new Vec3(-0.75f, 3.5f + value, -1.25f)) * Mat4.CreateRotationY(-angle);
            Alvor.Mat4Values.AlvorMat4Right[index] = AlvorMat4RightPose * Mat4.CreateScale(new Vec3(0.8f, 1.2f, 0.95f));
            var systemLeft = (System.Numerics.Matrix4x4)Alvor.Mat4Values.AlvorMat4Left[index];
            Numerics.Matrix4x4Values.SystemMat4Left[index] = System.Numerics.Matrix4x4.Transpose(systemLeft);
            var systemRight = (System.Numerics.Matrix4x4)Alvor.Mat4Values.AlvorMat4Right[index];
            Numerics.Matrix4x4Values.SystemMat4Right[index] = System.Numerics.Matrix4x4.Transpose(systemRight);
            Alvor.QuatValues.AlvorQuatLeft[index] = Quat.CreateFromAxisAngle(Vec3.UnitY, angle);
            Alvor.QuatValues.AlvorQuatRight[index] = Quat.CreateFromAxisAngle(Vec3.UnitZ, -angle * 0.75f);
            Numerics.QuaternionValues.SystemQuatLeft[index] = (System.Numerics.Quaternion)Alvor.QuatValues.AlvorQuatLeft[index];
            Numerics.QuaternionValues.SystemQuatRight[index] = (System.Numerics.Quaternion)Alvor.QuatValues.AlvorQuatRight[index];
            Alvor.Vec2Values.AlvorVec2[index] = new Vec2(1.5f + value, -0.5f);
            Numerics.Vector2Values.SystemVec2[index] = Alvor.Vec2Values.AlvorVec2[index];
            Alvor.Vec3Values.AlvorVec3[index] = new Vec3(1.5f + value, -0.5f, 2.25f);
            Numerics.Vector3Values.SystemVec3[index] = Alvor.Vec3Values.AlvorVec3[index];
            Alvor.Vec4Values.AlvorVec4[index] = new Vec4(Alvor.Vec3Values.AlvorVec3[index], 1f);
            Numerics.Vector4Values.SystemVec4[index] = Alvor.Vec4Values.AlvorVec4[index];
        }
    }
}
