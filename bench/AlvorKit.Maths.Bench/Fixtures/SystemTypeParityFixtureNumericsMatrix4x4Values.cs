namespace AlvorKit;

internal class SystemTypeParityFixtureNumericsMatrix4x4Values
{
    private readonly System.Numerics.Matrix4x4[] systemMat4Left = new System.Numerics.Matrix4x4[4096];
    private readonly System.Numerics.Matrix4x4[] systemMat4Right = new System.Numerics.Matrix4x4[4096];
    private readonly System.Numerics.Matrix4x4[] systemMat4Output = new System.Numerics.Matrix4x4[4096];
    internal System.Numerics.Matrix4x4[] SystemMat4Left => systemMat4Left;
    internal System.Numerics.Matrix4x4[] SystemMat4Right => systemMat4Right;
    internal System.Numerics.Matrix4x4[] SystemMat4Output => systemMat4Output;
}
