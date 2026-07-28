namespace AlvorKit.Mocking.Test.Contracts.RefSafe;

// Every clause, matcher, projector, and delegate shape below binds the
// production public surface.
internal static class MockingRefSafeApiContract
{
    internal static void ByValueCallbacks(
        global::AlvorKit.Mocking.MockSetupClause clause,
        global::AlvorKit.Mocking.MockSetupClause<int> resultClause,
        int[] observed)
    {
        clause.Do(
            (ReadOnlySpan<int> values) =>
                observed[0] = values.Length);
        clause.Do(
            (
                ReadOnlySpan<int> source,
                Span<int> destination) =>
            {
                source.CopyTo(destination);
                observed[1] = destination.Length;
            });
        resultClause.Answer(
            (
                int offset,
                ReadOnlySpan<int> source,
                Span<int> destination) =>
            {
                source.CopyTo(destination);
                return offset + source.Length;
            });
    }

    internal static void ExactReferenceCallbacks(
        global::AlvorKit.Mocking.MockSetupClause clause,
        global::AlvorKit.Mocking.MockSetupClause<int> resultClause)
    {
        clause.Do(
            (
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                source.CopyTo(destination);
                written = new(destination[..source.Length]);
            });
        resultClause.Answer(
            (
                int offset,
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                source.CopyTo(destination);
                written = new(destination[..source.Length]);
                return offset + source.Length;
            });
    }

    internal static void WideCallback(
        global::AlvorKit.Mocking.MockSetupClause clause,
        int[] observed)
    {
        clause.Do(
            (
                int v0, int v1, int v2, int v3, int v4, int v5,
                int v6, int v7, int v8, int v9, int v10, int v11,
                int v12, int v13, int v14, int v15, int v16) =>
            {
                observed[0] =
                    v0 + v1 + v2 + v3 + v4 + v5 +
                    v6 + v7 + v8 + v9 + v10 + v11 +
                    v12 + v13 + v14 + v15 + v16;
            });
    }

    internal static void Matchers(
        IRefSafeContractTarget target,
        int[] expected)
    {
        target.Observe(
            global::AlvorKit.Mocking.Arg
                .Any<ReadOnlySpan<int>>(0));
        target.Observe(
            global::AlvorKit.Mocking.Arg
                .Match<ReadOnlySpan<int>>(
                0,
                values => values.SequenceEqual(expected)));
        target.Transform(
            ref global::AlvorKit.Mocking.Arg
                .AnyRef<Span<int>>(0));
        target.Transform(
            ref global::AlvorKit.Mocking.Arg
                .Match<Span<int>>(
                0,
                (scoped in values) =>
                    values.Length >= expected.Length));
        target.TransformExact(
            global::AlvorKit.Mocking.Arg
                .Match<ReadOnlySpan<int>>(
                0,
                values => values.SequenceEqual(expected)),
            ref global::AlvorKit.Mocking.Arg
                .AnyRef<Span<int>>(1),
            out _);
    }

    internal static void SpanConvenience(
        IRefSafeContractTarget target,
        ReadOnlySpan<int> expected)
    {
        target.Observe(
            global::AlvorKit.Mocking.Arg
                .ReadOnlySpanEqual(0, expected));
        target.TransformByValue(
            global::AlvorKit.Mocking.Arg
                .SpanEqual(0, expected));
    }

    internal static void Snapshots(
        global::AlvorKit.Mocking.MockSetupClause clause)
    {
        clause
            .SnapshotArgument(
                0,
                (ReadOnlySpan<int> values) => values.ToArray())
            .SnapshotArgument(
                1,
                (
                    scoped in BorrowedWindow values) =>
                    values.Values.Length)
            .SnapshotArgumentOnExit(
                1,
                (
                    scoped in BorrowedWindow values) =>
                    values.Values.ToArray());
    }

    internal static void TaskReturning(
        global::AlvorKit.Mocking.MockSetupClause<Task<int>> clause)
    {
        clause.Answer(
            (ReadOnlySpan<byte> bytes) =>
                ConsumeCopiedAsync(bytes.ToArray()));
    }

    internal static void ValueTaskReturning(
        global::AlvorKit.Mocking.MockSetupClause<ValueTask<int>> clause)
    {
        clause.Answer(
            (ReadOnlySpan<byte> bytes) =>
                ConsumeCopiedValueTaskAsync(
                    bytes.ToArray()));
    }

    private static async Task<int> ConsumeCopiedAsync(
        byte[] bytes)
    {
        await Task.Yield();
        return bytes.Length;
    }

    private static async ValueTask<int> ConsumeCopiedValueTaskAsync(
        byte[] bytes)
    {
        await Task.Yield();
        return bytes.Length;
    }
}
