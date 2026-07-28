namespace AlvorKit.Mocking;

/// <summary>Compares object pairs by identity while traversing capture values.</summary>
internal sealed class MockReferencePairComparer :
    IEqualityComparer<(object First, object Second)>
{
    internal static readonly MockReferencePairComparer Instance = new();

    public bool Equals(
        (object First, object Second) x,
        (object First, object Second) y) =>
        ReferenceEquals(x.First, y.First) &&
        ReferenceEquals(x.Second, y.Second);

    public int GetHashCode(
        (object First, object Second) pair) =>
        HashCode.Combine(
            RuntimeHelpers.GetHashCode(pair.First),
            RuntimeHelpers.GetHashCode(pair.Second));
}
