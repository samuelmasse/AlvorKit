namespace AlvorKit;

/// <summary>
/// Retains the executable callback-normalization proof independently of the
/// production-bound approved source fixture.
/// </summary>
internal static class MockingRefSafeCompilerProof
{
    internal static void ExactReferenceCallbacks(
        RefSafeProofSetupClause clause,
        RefSafeProofSetupClause<int> resultClause)
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
        RefSafeProofSetupClause clause,
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

    internal static void Snapshots(
        RefSafeProofSetupClause clause)
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
        RefSafeProofSetupClause<Task<int>> clause)
    {
        clause.Answer(
            (ReadOnlySpan<byte> bytes) =>
                ConsumeCopiedAsync(bytes.ToArray()));
    }

    internal static void ValueTaskReturning(
        RefSafeProofSetupClause<ValueTask<int>> clause)
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

/// <summary>Models callback storage only for the executable compiler proof.</summary>
internal sealed class RefSafeProofSetupClause
{
    private readonly List<Delegate> projectors = [];
    private readonly MethodInfo? capturedMethod;

    internal RefSafeProofSetupClause()
    {
    }

    internal RefSafeProofSetupClause(MethodInfo capturedMethod)
    {
        this.capturedMethod = capturedMethod;
    }

    internal Delegate? Callback { get; private set; }

    internal RefSafeCallbackKind Kind { get; private set; }

    internal int NormalizationCount { get; private set; }

    internal IReadOnlyList<Delegate> Projectors => projectors;

    internal void Do<T>(Action<T> callback)
        where T : allows ref struct =>
        Set(callback, RefSafeCallbackKind.Action);

    internal void Do<T1, T2>(Action<T1, T2> callback)
        where T1 : allows ref struct
        where T2 : allows ref struct =>
        Set(callback, RefSafeCallbackKind.Action);

    internal void Do(Delegate callback) =>
        Set(callback, RefSafeCallbackKind.NaturalDelegate);

    internal RefSafeProofSetupClause SnapshotArgument<T, TResult>(
        int parameterIndex,
        Func<T, TResult> projector)
        where T : allows ref struct =>
        RegisterProjector(parameterIndex, projector);

    internal RefSafeProofSetupClause SnapshotArgument<T, TResult>(
        int parameterIndex,
        SnapshotProjector<T, TResult> projector)
        where T : allows ref struct =>
        RegisterProjector(parameterIndex, projector);

    internal RefSafeProofSetupClause SnapshotArgumentOnExit<T, TResult>(
        int parameterIndex,
        SnapshotProjector<T, TResult> projector)
        where T : allows ref struct =>
        RegisterProjector(parameterIndex, projector);

    private RefSafeProofSetupClause RegisterProjector(
        int parameterIndex,
        Delegate projector)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
        ArgumentNullException.ThrowIfNull(projector);
        projectors.Add(projector);
        return this;
    }

    private void Set(
        Delegate callback,
        RefSafeCallbackKind kind)
    {
        if (kind == RefSafeCallbackKind.NaturalDelegate)
        {
            callback = RefSafeCallbackContract.Normalize(
                callback,
                capturedMethod ?? throw MissingExactSignature());
            NormalizationCount++;
        }
        else
        {
            RefSafeCallbackContract.ValidateReturn(
                callback,
                typeof(void));
        }

        Callback = callback;
        Kind = kind;
    }

    private static InvalidOperationException MissingExactSignature() =>
        new(
            "Natural callbacks require the closed captured method and stable exact delegate type.");
}

/// <summary>Models result callback storage only for the executable compiler proof.</summary>
internal sealed class RefSafeProofSetupClause<TResult>
{
    private readonly MethodInfo? capturedMethod;

    internal RefSafeProofSetupClause()
    {
    }

    internal RefSafeProofSetupClause(MethodInfo capturedMethod)
    {
        this.capturedMethod = capturedMethod;
    }

    internal Delegate? Callback { get; private set; }

    internal RefSafeCallbackKind Kind { get; private set; }

    internal int NormalizationCount { get; private set; }

    internal void Answer<T>(Func<T, TResult> callback)
        where T : allows ref struct =>
        Set(callback, RefSafeCallbackKind.Func);

    internal void Answer<T1, T2, T3>(
        Func<T1, T2, T3, TResult> callback)
        where T1 : allows ref struct
        where T2 : allows ref struct
        where T3 : allows ref struct =>
        Set(callback, RefSafeCallbackKind.Func);

    internal void Answer(Delegate callback) =>
        Set(callback, RefSafeCallbackKind.NaturalDelegate);

    private void Set(
        Delegate callback,
        RefSafeCallbackKind kind)
    {
        if (kind == RefSafeCallbackKind.NaturalDelegate)
        {
            callback = RefSafeCallbackContract.Normalize(
                callback,
                capturedMethod ?? throw MissingExactSignature());
            NormalizationCount++;
        }
        else
        {
            RefSafeCallbackContract.ValidateReturn(
                callback,
                typeof(TResult));
        }

        Callback = callback;
        Kind = kind;
    }

    private static InvalidOperationException MissingExactSignature() =>
        new(
            "Natural callbacks require the closed captured method and stable exact delegate type.");
}

/// <summary>Models storage-free matcher placeholders for the compiler proof.</summary>
internal static class RefSafeProofArg
{
    internal static T Match<T>(
        int parameterIndex,
        Func<T, bool> predicate)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            parameterIndex);
        ArgumentNullException.ThrowIfNull(predicate);
        return default!;
    }

    internal static ref T AnyRef<T>(int parameterIndex)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            parameterIndex);
        return ref System.Runtime.CompilerServices.Unsafe
            .NullRef<T>();
    }

    internal static ref T Match<T>(
        int parameterIndex,
        RefPredicate<T> predicate)
        where T : allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            parameterIndex);
        ArgumentNullException.ThrowIfNull(predicate);
        return ref System.Runtime.CompilerServices.Unsafe
            .NullRef<T>();
    }
}
