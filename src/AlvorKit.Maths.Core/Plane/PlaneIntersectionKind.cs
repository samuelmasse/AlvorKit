namespace AlvorKit.Maths;

/// <summary>Describes which side of a plane contains a point or bounded shape.</summary>
public enum PlaneIntersectionKind
{
    /// <summary>The value is on the negative side where plane evaluation is less than zero.</summary>
    Negative,

    /// <summary>The value touches or crosses the plane.</summary>
    Intersecting,

    /// <summary>The value is on the positive side where plane evaluation is greater than zero.</summary>
    Positive,
}
