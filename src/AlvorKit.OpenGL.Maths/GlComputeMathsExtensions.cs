namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped compute dispatch overloads.</summary>
public static class GlComputeMathsExtensions
{
    /// <summary>Calls <see cref="Gl.DispatchCompute(uint, uint, uint)"/> for <c>glDispatchCompute</c>.</summary>
    public static void DispatchCompute(this Gl gl, Vec3u groupCounts) =>
        gl.DispatchCompute(groupCounts.X, groupCounts.Y, groupCounts.Z);
}
