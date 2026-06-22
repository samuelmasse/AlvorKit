namespace AlvorKit.Engine;

/// <summary>Root-owned unit cube face geometry used by simple block and test-cube meshers.</summary>
[Root]
public sealed class RootCube
{
    private readonly CubeFace front = new(new((0, 1, 1), (1, 1, 1), (0, 0, 1), (1, 0, 1)), (0, 0, 1));
    private readonly CubeFace back = new(new((1, 1, 0), (0, 1, 0), (1, 0, 0), (0, 0, 0)), (0, 0, -1));
    private readonly CubeFace top = new(new((0, 1, 0), (1, 1, 0), (0, 1, 1), (1, 1, 1)), (0, 1, 0));
    private readonly CubeFace bottom = new(new((0, 0, 1), (1, 0, 1), (0, 0, 0), (1, 0, 0)), (0, -1, 0));
    private readonly CubeFace left = new(new((0, 1, 0), (0, 1, 1), (0, 0, 0), (0, 0, 1)), (-1, 0, 0));
    private readonly CubeFace right = new(new((1, 1, 1), (1, 1, 0), (1, 0, 1), (1, 0, 0)), (1, 0, 0));
    private readonly CubeFace[] faces;

    /// <summary>Creates the six unit-cube faces.</summary>
    public RootCube() => faces = [front, back, top, bottom, left, right];

    /// <summary>Gets the front face.</summary>
    public CubeFace Front => front;

    /// <summary>Gets the back face.</summary>
    public CubeFace Back => back;

    /// <summary>Gets the top face.</summary>
    public CubeFace Top => top;

    /// <summary>Gets the bottom face.</summary>
    public CubeFace Bottom => bottom;

    /// <summary>Gets the left face.</summary>
    public CubeFace Left => left;

    /// <summary>Gets the right face.</summary>
    public CubeFace Right => right;

    /// <summary>Gets all six faces without copying.</summary>
    public ReadOnlySpan<CubeFace> Faces => faces;
}
