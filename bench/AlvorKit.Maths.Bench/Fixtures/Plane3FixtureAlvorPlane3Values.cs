namespace AlvorKit;

internal class Plane3FixtureAlvorPlane3Values
{
    private readonly Plane3[] alvorPlanes = new Plane3[4096];
    private readonly Plane3[] alvorOtherPlanes = new Plane3[4096];
    private readonly Plane3[] alvorOutput = new Plane3[4096];
    internal Plane3[] AlvorPlanes => alvorPlanes;
    internal Plane3[] AlvorOtherPlanes => alvorOtherPlanes;
    internal Plane3[] AlvorOutput => alvorOutput;
}
