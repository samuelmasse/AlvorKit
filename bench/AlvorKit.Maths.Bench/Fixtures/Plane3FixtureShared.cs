namespace AlvorKit;

internal class Plane3FixtureShared
{
    private readonly float[] scalarOutput = new float[4096];
    private readonly int[] intOutput = new int[4096];
    internal float[] ScalarOutput => scalarOutput;
    internal int[] IntOutput => intOutput;
}
