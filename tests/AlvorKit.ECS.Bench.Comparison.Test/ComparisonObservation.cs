namespace AlvorKit;

internal readonly record struct ComparisonObservation(int Count, long First, long Second, long Third)
{
    internal void Verify(int count, int components, int passes, bool creation)
    {
        Assert.AreEqual(count, Count, "Number of matching Ents.");
        Assert.AreEqual(creation ? 0L : (long)count * passes * Math.Max(1, components - 1), First, "First component sum.");
        Assert.AreEqual(creation || components < 2 ? 0L : count, Second, "Second component sum.");
        Assert.AreEqual(creation || components < 3 ? 0L : count, Third, "Third component sum.");
    }
}
