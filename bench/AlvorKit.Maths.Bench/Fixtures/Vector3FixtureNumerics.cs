namespace AlvorKit;

internal class Vector3FixtureNumerics
{
    private readonly System.Numerics.Vector3[] systemLeft = new System.Numerics.Vector3[4096];
    private readonly System.Numerics.Vector3[] systemRight = new System.Numerics.Vector3[4096];
    private readonly System.Numerics.Vector3[] systemOutput = new System.Numerics.Vector3[4096];
    internal System.Numerics.Vector3[] SystemLeft => systemLeft;
    internal System.Numerics.Vector3[] SystemRight => systemRight;
    internal System.Numerics.Vector3[] SystemOutput => systemOutput;
}
