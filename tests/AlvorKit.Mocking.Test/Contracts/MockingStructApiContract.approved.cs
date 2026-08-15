namespace AlvorKit;

// These methods compile the public struct interception surface without executing
// setup.
internal static class MockingStructApiContract
{
    private static readonly int[] WindowOwner = new int[4];

    internal static void TypeWide(
        StructReplacement replacement)
    {
        Mock.Struct<StructCounter>()
            .When(
                static (scoped ref value) =>
                    value.Advance(Arg.Any<int>()))
            .SnapshotThisOnEntry(
                (scoped in value) =>
                    value.Value)
            .MutateThisOnEntry(
                (scoped ref value) =>
                    value.Value++)
            .MutateThisOnExit(
                (scoped ref value) =>
                    value.Value++)
            .SnapshotThisOnExit(
                (scoped in value) =>
                    value.Value)
            .Do(replacement);
    }

    internal static void LiveValueMatched(
        int expectedKey)
    {
        MockStructScope<StructCounter> scope =
            Mock.Struct<StructCounter>()
                .Matching(
                    (scoped in value) =>
                        value.Key == expectedKey);

        scope.When(
                static (scoped ref value) =>
                    value.Read())
            .Return(42);

        scope.Verify(
                static (scoped ref value) =>
                    value.Read())
            .AtLeast(1);
    }

    internal static void EqualValuesAtDifferentSites(
        MockCallSite firstSite,
        MockCallSite secondSite)
    {
        Mock.Struct<StructCounter>()
            .AtSite(firstSite)
            .When(
                static (scoped ref value) =>
                    value.Read())
            .Return(1);

        Mock.Struct<StructCounter>()
            .AtSite(secondSite)
            .When(
                static (scoped ref value) =>
                    value.Read())
            .Return(2);

        Mock.Struct<StructCounter>()
            .AtSite(firstSite)
            .Verify(
                static (scoped ref value) =>
                    value.Read())
            .Once();
    }

    internal static void RefSafeReturn()
    {
        Mock.Struct<StructCounter>()
            .When(
                static (scoped ref value) =>
                    value.Window())
            .ReturnFactory(static () => WindowOwner);
    }

    internal static void ReadonlyAndUnmanaged()
    {
        Mock.Struct<ReadonlyCounter>()
            .When(
                static (scoped ref value) =>
                    value.Read())
            .Passthrough();

        Mock.Struct<UnmanagedCounter>()
            .When(
                static (scoped ref value) =>
                    value.Increment())
            .Strict();
    }

    internal static void CopiesAreValues(
        StructCounter original)
    {
        // Assignment and boxing each create a copy. No public API accepts
        // either value as stable receiver identity; selection remains by type,
        // live entry predicate, or site.
        StructCounter assignedCopy = original;
        object boxedCopy = original;

        _ = assignedCopy;
        _ = boxedCopy;
    }
}

internal delegate void StructReplacement(
    scoped ref StructCounter value,
    int amount);

internal struct StructCounter(int key)
{
    internal int Key { get; } = key;

    internal int Value { get; set; }

    internal void Advance(int amount) => Value += amount;

    internal readonly int Read() => Value;

    internal readonly Span<int> Window() => default;
}

internal readonly record struct ReadonlyCounter(int Value)
{
    internal int Read() => Value;
}

internal struct UnmanagedCounter
{
    internal int Value;

    internal void Increment() => Value++;
}
