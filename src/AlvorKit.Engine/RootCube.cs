namespace AlvorKit.Engine;

/// <summary>Root-owned unit cube face geometry used by simple block and test-cube meshers.</summary>
[Root]
public class RootCube
{
    /// <summary>Gets the front face.</summary>
    public readonly CubeFace Front = new(new((0, 1, 1), (1, 1, 1), (0, 0, 1), (1, 0, 1)), (0, 0, 1));

    /// <summary>Gets the back face.</summary>
    public readonly CubeFace Back = new(new((1, 1, 0), (0, 1, 0), (1, 0, 0), (0, 0, 0)), (0, 0, -1));

    /// <summary>Gets the top face.</summary>
    public readonly CubeFace Top = new(new((0, 1, 0), (1, 1, 0), (0, 1, 1), (1, 1, 1)), (0, 1, 0));

    /// <summary>Gets the bottom face.</summary>
    public readonly CubeFace Bottom = new(new((0, 0, 1), (1, 0, 1), (0, 0, 0), (1, 0, 0)), (0, -1, 0));

    /// <summary>Gets the left face.</summary>
    public readonly CubeFace Left = new(new((0, 1, 0), (0, 1, 1), (0, 0, 0), (0, 0, 1)), (-1, 0, 0));

    /// <summary>Gets the right face.</summary>
    public readonly CubeFace Right = new(new((1, 1, 1), (1, 1, 0), (1, 0, 1), (1, 0, 0)), (1, 0, 0));

    private readonly CubeFace[] faces;

    /// <summary>Creates the six unit-cube faces.</summary>
    public RootCube() => faces = [Front, Back, Top, Bottom, Left, Right];

    /// <summary>Gets all six faces without copying.</summary>
    public ReadOnlySpan<CubeFace> Faces => faces;
}
