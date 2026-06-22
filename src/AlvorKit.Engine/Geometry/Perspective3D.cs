namespace AlvorKit.Engine;

/// <summary>Mutable 3D perspective matrices derived from a canvas and camera.</summary>
public sealed class Perspective3D
{
    private float fov = 70;
    private float near = 0.1f;
    private float far = 1000f;
    private Mat4 view;
    private Mat4 projection;

    /// <summary>Gets a mutable vertical field of view in degrees.</summary>
    public ref float Fov => ref fov;

    /// <summary>Gets a mutable near clipping plane distance.</summary>
    public ref float Near => ref near;

    /// <summary>Gets a mutable far clipping plane distance.</summary>
    public ref float Far => ref far;

    /// <summary>Gets the latest view matrix.</summary>
    public ref Mat4 View => ref view;

    /// <summary>Gets the latest projection matrix.</summary>
    public ref Mat4 Projection => ref projection;

    /// <summary>Computes view and projection matrices for the canvas and camera.</summary>
    public void ComputeMatrix(Vec2 canvas, Camera3D camera)
    {
        var fovy = fov * float.Pi / 180f;
        var aspect = canvas.X / canvas.Y;
        view = Mat4.LookAt(camera.Offset, camera.Offset + camera.LookAt, camera.Up);
        projection = Mat4.CreatePerspectiveFieldOfView(fovy, aspect, near, far);
    }
}
