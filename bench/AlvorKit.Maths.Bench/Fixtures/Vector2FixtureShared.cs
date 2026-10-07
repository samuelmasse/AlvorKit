namespace AlvorKit;

internal class Vector2FixtureShared
{
    private readonly float[] amount = new float[4096];
    private readonly float[] scalar = new float[4096];
    private readonly float[] scalarOutput = new float[4096];
    private readonly int[] intOutput = new int[4096];
    internal float[] Amount => amount;
    internal float[] Scalar => scalar;
    internal float[] ScalarOutput => scalarOutput;
    internal int[] IntOutput => intOutput;
}
