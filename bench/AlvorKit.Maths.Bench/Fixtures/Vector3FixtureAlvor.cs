namespace AlvorKit;

internal class Vector3FixtureAlvor
{
    private readonly Vec3[] alvorLeft = new Vec3[4096];
    private readonly Vec3[] alvorRight = new Vec3[4096];
    private readonly Vec3[] alvorOutput = new Vec3[4096];
    internal Vec3[] AlvorLeft => alvorLeft;
    internal Vec3[] AlvorRight => alvorRight;
    internal Vec3[] AlvorOutput => alvorOutput;
}
