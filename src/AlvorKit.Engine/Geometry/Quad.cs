namespace AlvorKit.Engine;

/// <summary>Four corners that describe one quad face in 3D space.</summary>
public readonly record struct Quad(Vec3 TopLeft, Vec3 TopRight, Vec3 BottomLeft, Vec3 BottomRight);
