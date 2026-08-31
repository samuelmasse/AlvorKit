namespace AlvorKit;

/// <summary>An exact FastNoise2 metadata name and optional XYZW component.</summary>
internal readonly record struct FnMemberKey(string Name, int Dimension)
{
    public static FnMemberKey Scalar(string name) => new(name, -1);
}
