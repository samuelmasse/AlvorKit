namespace AlvorKit;

internal class SystemTypeParityFixtureNumericsQuaternionValues
{
    private readonly System.Numerics.Quaternion[] systemQuatLeft = new System.Numerics.Quaternion[4096];
    private readonly System.Numerics.Quaternion[] systemQuatRight = new System.Numerics.Quaternion[4096];
    private readonly System.Numerics.Quaternion[] systemQuatOutput = new System.Numerics.Quaternion[4096];
    internal System.Numerics.Quaternion[] SystemQuatLeft => systemQuatLeft;
    internal System.Numerics.Quaternion[] SystemQuatRight => systemQuatRight;
    internal System.Numerics.Quaternion[] SystemQuatOutput => systemQuatOutput;
}
