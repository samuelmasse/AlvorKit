namespace AlvorKit;

/// <summary>An exact FastNoise2 metadata name and optional XYZW component.</summary>
/// <param name="Name">The case-sensitive runtime metadata name.</param>
/// <param name="Dimension"><c>-1</c> for a scalar or <c>0..3</c> for X, Y, Z, or W.</param>
internal readonly record struct FnMemberKey(string Name, int Dimension)
{
    /// <summary>Creates a member key whose runtime dimension index is <c>-1</c>.</summary>
    public static FnMemberKey Scalar(string name) => new(name, -1);
}
