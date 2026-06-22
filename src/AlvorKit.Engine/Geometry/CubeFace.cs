namespace AlvorKit.Engine;

/// <summary>One cube face with its quad corners and outward normal.</summary>
public readonly record struct CubeFace(Quad Quad, Vec3 Normal);
