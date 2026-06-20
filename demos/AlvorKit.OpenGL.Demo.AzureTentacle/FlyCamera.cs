namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Tracks a Minecraft-style free camera for inspecting the static GLB model.</summary>
public sealed class FlyCamera(Vector3 initialPosition, float initialYaw, float initialPitch)
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
    private Vector3 position = initialPosition;

    /// <summary>The current camera yaw angle in radians.</summary>
    private float yaw = initialYaw;

    /// <summary>The current camera pitch angle in radians.</summary>
    private float pitch = Math.Clamp(initialPitch, -MaxPitch, MaxPitch);

    /// <summary>Whether the next cursor sample should seed mouse look instead of moving the camera.</summary>
    private bool needsCursorSeed = true;

    /// <summary>The previous cursor x coordinate used to calculate mouse movement.</summary>
    private double previousCursorX;

    /// <summary>The previous cursor y coordinate used to calculate mouse movement.</summary>
    private double previousCursorY;

    /// <summary>Updates mouse look and keyboard movement only while the camera controls are captured.</summary>
    public void Update(GlfwBackend glfw, GlfwWindow window, float elapsedSeconds, bool mouseTracking)
    {
        if (!mouseTracking)
        {
            ResetMouseTracking();
            return;
        }

        UpdateLook(glfw, window);
        UpdatePosition(glfw, window, elapsedSeconds);
    }

    /// <summary>Forces the next mouse-look update to seed its cursor position instead of applying a delta.</summary>
    public void ResetMouseTracking() => needsCursorSeed = true;

    /// <summary>Writes a column-major view matrix for the current camera state.</summary>
    public void WriteViewMatrix(Span<float> matrix)
    {
        var forward = Forward();
        var right = Right();
        var up = Vector3.Cross(right, forward);
        var back = -forward;

        matrix.Clear();
        matrix[MatrixIndex(0, 0)] = right.X;
        matrix[MatrixIndex(0, 1)] = right.Y;
        matrix[MatrixIndex(0, 2)] = right.Z;
        matrix[MatrixIndex(0, 3)] = -Vector3.Dot(right, position);
        matrix[MatrixIndex(1, 0)] = up.X;
        matrix[MatrixIndex(1, 1)] = up.Y;
        matrix[MatrixIndex(1, 2)] = up.Z;
        matrix[MatrixIndex(1, 3)] = -Vector3.Dot(up, position);
        matrix[MatrixIndex(2, 0)] = back.X;
        matrix[MatrixIndex(2, 1)] = back.Y;
        matrix[MatrixIndex(2, 2)] = back.Z;
        matrix[MatrixIndex(2, 3)] = -Vector3.Dot(back, position);
        matrix[MatrixIndex(3, 3)] = 1f;
    }

    /// <summary>Updates yaw and pitch from GLFW cursor deltas.</summary>
    private void UpdateLook(GlfwBackend glfw, GlfwWindow window)
    {
        glfw.GetCursorPos(window, out var cursorX, out var cursorY);
        if (needsCursorSeed)
        {
            previousCursorX = cursorX;
            previousCursorY = cursorY;
            needsCursorSeed = false;
            return;
        }

        yaw += (float)(cursorX - previousCursorX) * MouseSensitivity;
        pitch = Math.Clamp(pitch - (float)(cursorY - previousCursorY) * MouseSensitivity, -MaxPitch, MaxPitch);
        previousCursorX = cursorX;
        previousCursorY = cursorY;
    }

    /// <summary>Moves the camera using WASD plus Space and Control for vertical travel.</summary>
    private void UpdatePosition(GlfwBackend glfw, GlfwWindow window, float elapsedSeconds)
    {
        var forward = ForwardOnGround();
        var right = Right();
        var movement =
            forward * (Axis(glfw, window, GlfwKey.W) - Axis(glfw, window, GlfwKey.S)) +
            right * (Axis(glfw, window, GlfwKey.D) - Axis(glfw, window, GlfwKey.A)) +
            Vector3.UnitY * (Axis(glfw, window, GlfwKey.Space) - ControlAxis(glfw, window));

        if (movement.LengthSquared() <= 0f)
            return;

        var speed = ShiftAxis(glfw, window) > 0f ? FastMoveSpeed : MoveSpeed;
        position += Vector3.Normalize(movement) * speed * MathF.Max(0f, elapsedSeconds);
    }

    /// <summary>Gets the camera's full forward vector, including pitch.</summary>
    private Vector3 Forward()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        var pitchSin = MathF.Sin(pitch);
        var pitchCos = MathF.Cos(pitch);
        return Vector3.Normalize(new Vector3(yawSin * pitchCos, pitchSin, -yawCos * pitchCos));
    }

    /// <summary>Gets the camera's horizontal forward vector for Minecraft-style walking.</summary>
    private Vector3 ForwardOnGround()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        return Vector3.Normalize(new Vector3(yawSin, 0f, -yawCos));
    }

    /// <summary>Gets the camera's horizontal right vector for strafing.</summary>
    private Vector3 Right()
    {
        var yawSin = MathF.Sin(yaw);
        var yawCos = MathF.Cos(yaw);
        return Vector3.Normalize(new Vector3(yawCos, 0f, yawSin));
    }

    /// <summary>Returns one when the supplied key is currently held.</summary>
    private static float Axis(GlfwBackend glfw, GlfwWindow window, GlfwKey key) =>
        glfw.GetKey(window, key) == GlfwInputAction.Press ? 1f : 0f;

    /// <summary>Returns one when either control key is currently held.</summary>
    private static float ControlAxis(GlfwBackend glfw, GlfwWindow window) =>
        Axis(glfw, window, GlfwKey.LeftControl) + Axis(glfw, window, GlfwKey.RightControl) > 0f ? 1f : 0f;

    /// <summary>Returns one when either shift key is currently held.</summary>
    private static float ShiftAxis(GlfwBackend glfw, GlfwWindow window) =>
        Axis(glfw, window, GlfwKey.LeftShift) + Axis(glfw, window, GlfwKey.RightShift) > 0f ? 1f : 0f;

    /// <summary>Maps row and column coordinates to a column-major 4x4 matrix span index.</summary>
    private static int MatrixIndex(int row, int column) => (column * 4) + row;
}
