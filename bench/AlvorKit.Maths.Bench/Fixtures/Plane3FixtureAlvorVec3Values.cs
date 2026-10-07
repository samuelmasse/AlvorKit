namespace AlvorKit;

internal class Plane3FixtureAlvorVec3Values
{
    private readonly Vec3[] alvorVectors = new Vec3[4096];
    private readonly Vec3[] alvorPoint0 = new Vec3[4096];
    private readonly Vec3[] alvorPoint1 = new Vec3[4096];
    private readonly Vec3[] alvorPoint2 = new Vec3[4096];
    internal Vec3[] AlvorVectors => alvorVectors;
    internal Vec3[] AlvorPoint0 => alvorPoint0;
    internal Vec3[] AlvorPoint1 => alvorPoint1;
    internal Vec3[] AlvorPoint2 => alvorPoint2;
}
