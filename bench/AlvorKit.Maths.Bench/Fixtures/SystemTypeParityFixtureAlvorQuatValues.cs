namespace AlvorKit;

internal class SystemTypeParityFixtureAlvorQuatValues
{
    private readonly Quat[] alvorQuatLeft = new Quat[4096];
    private readonly Quat[] alvorQuatRight = new Quat[4096];
    private readonly Quat[] alvorQuatOutput = new Quat[4096];
    internal Quat[] AlvorQuatLeft => alvorQuatLeft;
    internal Quat[] AlvorQuatRight => alvorQuatRight;
    internal Quat[] AlvorQuatOutput => alvorQuatOutput;
}
