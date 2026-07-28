namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockSpanSnapshotRegistrationTest
{
    /// <summary>Registration validates index, source type, phase, duplicates, and null delegates.</summary>
    [TestMethod]
    public void ProjectorRegistration_RejectsInvalidShapes()
    {
        var target = Mock.CreateLoose<IRefStructMatcherTarget>();
        MockSetupClause<int> observe = Mock.When(
            () => target.Observe(
                Arg.Any<ReadOnlySpan<int>>(0)));
        MockSetupClause<int> produce = Mock.When(
            () => target.Produce(out _));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => observe.SnapshotArgument(
                -1,
                (ReadOnlySpan<int> values) =>
                    values.Length));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => observe.SnapshotArgument(
                1,
                (ReadOnlySpan<int> values) =>
                    values.Length));
        Assert.Throws<ArgumentException>(
            () => observe.SnapshotArgument(
                0,
                (string value) =>
                    value.Length));
        Assert.Throws<MockException>(
            () => observe.SnapshotArgumentOnExit(
                0,
                (
                    scoped in ReadOnlySpan<int> values) =>
                    values.Length));
        Assert.Throws<MockException>(
            () => produce.SnapshotArgument(
                0,
                (
                    scoped in Span<int> values) =>
                    values.Length));

        observe.SnapshotArgument(
            0,
            (ReadOnlySpan<int> values) =>
                values.Length);
        Assert.Throws<MockException>(
            () => observe.SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) =>
                    values.ToArray()));
        Assert.Throws<ArgumentNullException>(
            () => observe.SnapshotArgument<ReadOnlySpan<int>, int>(
                0,
                (SnapshotProjector<ReadOnlySpan<int>, int>)null!));
    }
}
