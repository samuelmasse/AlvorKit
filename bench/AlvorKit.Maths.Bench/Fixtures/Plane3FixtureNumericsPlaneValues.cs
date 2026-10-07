namespace AlvorKit;

internal class Plane3FixtureNumericsPlaneValues
{
    private readonly System.Numerics.Plane[] systemPlanes = new System.Numerics.Plane[4096];
    private readonly System.Numerics.Plane[] systemOtherPlanes = new System.Numerics.Plane[4096];
    private readonly System.Numerics.Plane[] systemOutput = new System.Numerics.Plane[4096];
    internal System.Numerics.Plane[] SystemPlanes => systemPlanes;
    internal System.Numerics.Plane[] SystemOtherPlanes => systemOtherPlanes;
    internal System.Numerics.Plane[] SystemOutput => systemOutput;
}
