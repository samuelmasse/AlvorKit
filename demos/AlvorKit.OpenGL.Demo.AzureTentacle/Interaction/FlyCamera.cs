namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Tracks a Minecraft-style free camera for inspecting the static GLB model.</summary>
public sealed class FlyCamera(Vec3 initialPosition, float initialYaw, float initialPitch)
{
    /// <summary>The normal movement speed in world units per second.</summary>
    private const float MoveSpeed = 2.4f;

    /// <summary>The faster movement speed used while shift is held.</summary>
    private const float FastMoveSpeed = 7.0f;

    /// <summary>The mouse look sensitivity in radians per cursor unit.</summary>
    private const float MouseSensitivity = 0.0022f;

    /// <summary>The maximum pitch angle before the camera reaches straight up or down.</summary>
    private const float MaxPitch = (MathF.PI * 0.5f) - 0.02f;

    /// <summary>The current camera position in world space.</summary>
    private Vec3 position = initialPosition;

    /// <summary>The current camera yaw angle in radians.</summary>
    private float yaw = initialYaw;

    /// <summary>The current camera pitch angle in radians.</summary>
    private float pitch = Math.Clamp(initialPitch, -MaxPitch, MaxPitch);

    /// <summary>Whether the next cursor sample should seed mouse look instead of moving the camera.</summary>
    private bool needsCursorSeed = true;

    /// <summary>Updates mouse look and keyboard movement only while the camera controls are captured.</summary>
    public void Update(RootKeyboard keyboard, RootMouse mouse, float elapsedSeconds, bool mouseTracking)
    {
        if (!mouseTracking)
        {
            ResetMouseTracking();
            return;
        }

        UpdateLook(mouse);
        UpdatePosition(keyboard, elapsedSeconds);
    }

    /// <summary>Forces the next mouse-look update to seed its cursor position instead of applying a delta.</summary>
    public void ResetMouseTracking() => needsCursorSeed = true;

    /// <summary>Creates a right-handed view matrix for the current camera state.</summary>
    public Mat4 CreateViewMatrix() => Mat4.LookTo(position, Forward(), Vec3.UnitY);

    /// <summary>Updates yaw and pitch from GLFW cursor deltas.</summary>
    private void UpdateLook(RootMouse mouse)
    {
        if (needsCursorSeed)
        {
            needsCursorSeed = false;
            return;
        }

        yaw += mouse.Delta.X * MouseSensitivity;
        pitch = Math.Clamp(pitch - mouse.Delta.Y * MouseSensitivity, -MaxPitch, MaxPitch);
    }

    /// <summary>Moves the camera using WASD plus Space and Control for vertical travel.</summary>
    private void UpdatePosition(RootKeyboard keyboard, float elapsedSeconds)
    {
        var forward = ForwardOnGround();
        var right = Right();
        var movement =
            forward * (Axis(keyboard, Keys.W) - Axis(keyboard, Keys.S)) +
            right * (Axis(keyboard, Keys.D) - Axis(keyboard, Keys.A)) +
            Vec3.UnitY * (Axis(keyboard, Keys.Space) - ControlAxis(keyboard));

        if (movement.LengthSquared <= 0f)
            return;

        var speed = ShiftAxis(keyboard) > 0f ? FastMoveSpeed : MoveSpeed;
        position += Vec3.Normalize(movement) * speed * MathF.Max(0f, elapsedSeconds);
    }

    /// <summary>Gets the camera's full forward vector, including pitch.</summary>
    private Vec3 Forward()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        var pitchSin = MathF.Sin(pitch);
        var pitchCos = MathF.Cos(pitch);
        return Vec3.Normalize((yawSin * pitchCos, pitchSin, -yawCos * pitchCos));
    }

    /// <summary>Gets the camera's horizontal forward vector for Minecraft-style walking.</summary>
    private Vec3 ForwardOnGround()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        return Vec3.Normalize((yawSin, 0f, -yawCos));
    }

    /// <summary>Gets the camera's horizontal right vector for strafing.</summary>
    private Vec3 Right()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        return Vec3.Normalize((yawCos, 0f, yawSin));
    }

    /// <summary>Returns one when the supplied key is currently held.</summary>
    private static float Axis(RootKeyboard keyboard, Keys key) =>
        keyboard.IsKeyDown(key) ? 1f : 0f;

    /// <summary>Returns one when either control key is currently held.</summary>
    private static float ControlAxis(RootKeyboard keyboard) =>
        Axis(keyboard, Keys.LeftControl) + Axis(keyboard, Keys.RightControl) > 0f ? 1f : 0f;

    /// <summary>Returns one when either shift key is currently held.</summary>
    private static float ShiftAxis(RootKeyboard keyboard) =>
        Axis(keyboard, Keys.LeftShift) + Axis(keyboard, Keys.RightShift) > 0f ? 1f : 0f;

}
