namespace AlvorKit.Engine;

/// <summary>One cube face with its quad corners and outward normal.</summary>
public record struct CubeFace(Quad3 Quad, Vec3 Normal);
