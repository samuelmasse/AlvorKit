namespace AlvorKit;

internal class Vector4FixtureNumerics
{
    private readonly System.Numerics.Vector4[] systemLeft = new System.Numerics.Vector4[4096];
    private readonly System.Numerics.Vector4[] systemRight = new System.Numerics.Vector4[4096];
    private readonly System.Numerics.Vector4[] systemOutput = new System.Numerics.Vector4[4096];
    internal System.Numerics.Vector4[] SystemLeft => systemLeft;
    internal System.Numerics.Vector4[] SystemRight => systemRight;
    internal System.Numerics.Vector4[] SystemOutput => systemOutput;
}
