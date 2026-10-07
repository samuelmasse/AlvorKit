namespace AlvorKit;

internal class Vector2FixtureNumerics
{
    private readonly System.Numerics.Vector2[] systemLeft = new System.Numerics.Vector2[4096];
    private readonly System.Numerics.Vector2[] systemRight = new System.Numerics.Vector2[4096];
    private readonly System.Numerics.Vector2[] systemOutput = new System.Numerics.Vector2[4096];
    internal System.Numerics.Vector2[] SystemLeft => systemLeft;
    internal System.Numerics.Vector2[] SystemRight => systemRight;
    internal System.Numerics.Vector2[] SystemOutput => systemOutput;
}
