namespace AlvorKit.Engine;

/// <summary>Mutable 3D camera orientation that computes front, right, up, and look-at vectors.</summary>
public class Camera3D
{
    private Vec3 offset;
    private Vec3 rotation;
    private Vec3 lookAt;
    private Vec3 up;
    private Vec3 right;
    private Vec3 front;

    /// <summary>Gets a mutable camera position offset.</summary>
    public ref Vec3 Offset => ref offset;

    /// <summary>Gets mutable Euler rotation in radians.</summary>
    public ref Vec3 Rotation => ref rotation;

    /// <summary>Gets the computed look direction.</summary>
    public Vec3 LookAt => lookAt;

    /// <summary>Gets the computed forward direction before pitch is applied.</summary>
    public Vec3 Front => front;

    /// <summary>Gets the computed up direction.</summary>
    public Vec3 Up => up;

    /// <summary>Gets the computed right direction.</summary>
    public Vec3 Right => right;

    /// <summary>Adds radians to the current rotation, wrapping each component into one turn.</summary>
    public void Rotate(Vec3 delta)
    {
        rotation.X = RotateAngle(rotation.X, delta.X);
        rotation.Y = RotateAngle(rotation.Y, delta.Y);
        rotation.Z = RotateAngle(rotation.Z, delta.Z);
    }

    /// <summary>Adds yaw and pitch radians to the current rotation.</summary>
    public void Rotate(Vec2 delta) => Rotate((delta.X, delta.Y, 0));

    /// <summary>Clamps pitch to avoid crossing the poles.</summary>
    public void PreventBackFlipsAndFrontFlips()
    {
        if (rotation.Y < DegreesToRadians(270) && rotation.Y >= float.Pi)
            rotation.Y = DegreesToRadians(270);
        else if (rotation.Y > DegreesToRadians(89.9f) && rotation.Y <= float.Pi)
            rotation.Y = DegreesToRadians(89.9f);
    }

    /// <summary>Recomputes look, up, right, and front vectors from the current rotation.</summary>
    public void ComputeVectors()
    {
        lookAt = (0, 0, -1);
        up = (0, 1, 0);
        right = (1, 0, 0);
        front = (0, 0, -1);

        var yaw = Quat.CreateFromAxisAngle(up, rotation.X);
        lookAt = yaw * lookAt;
        right = yaw * right;
        front = yaw * front;

        var pitch = Quat.CreateFromAxisAngle(right, rotation.Y);
        lookAt = pitch * lookAt;
        up = pitch * up;
    }

    private static float RotateAngle(float angle, float delta)
    {
        angle += delta;
        if (angle > float.Tau)
            angle -= float.Tau;
        else if (angle < 0)
            angle += float.Tau;
        return angle;
    }

    private static float DegreesToRadians(float degrees) => degrees * float.Pi / 180f;
}
