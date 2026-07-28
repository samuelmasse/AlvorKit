namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockConfiguredBehaviorTest
{
    private static readonly MethodInfo Method =
        typeof(MockConfiguredBehaviorTest).GetMethod(
            nameof(Target),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Throw behavior rejects a null configured exception immediately.</summary>
    [TestMethod]
    public void ThrowBehavior_NullException_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MockThrowBehavior(null!));
    }

    /// <summary>Throw behavior claims preserve the configured exception instance.</summary>
    [TestMethod]
    public void ThrowBehavior_Claim_PreservesExceptionIdentity()
    {
        var expected = new InvalidOperationException("configured");
        var behavior = new MockThrowBehavior(expected);

        var execution = behavior.Claim();

        Assert.AreEqual(MockBehaviorExecutionKind.Throw, execution.Kind);
        Assert.AreSame(expected, execution.Value);
        Assert.IsEmpty(execution.ReferenceValues);
        Assert.IsNull(execution.Callback);
    }

    /// <summary>A one-value return sequence repeats its only value.</summary>
    [TestMethod]
    public void ReturnSequence_OneValue_RepeatsValue()
    {
        var behavior = Sequence(7);

        var first = behavior.Claim();

        Assert.AreEqual(MockBehaviorExecutionKind.Return, first.Kind);
        Assert.AreEqual(7, first.Value);
        Assert.IsEmpty(first.ReferenceValues);
        Assert.IsNull(first.Callback);
        Assert.AreEqual(7, behavior.Claim().Value);
        Assert.AreEqual(7, behavior.Claim().Value);
    }

    /// <summary>A return sequence advances in order and repeats its final value.</summary>
    [TestMethod]
    public void ReturnSequence_SeveralValues_AdvancesThenRepeatsFinal()
    {
        var behavior = Sequence(10, 20, 30);

        Assert.AreEqual(10, behavior.Claim().Value);
        Assert.AreEqual(20, behavior.Claim().Value);
        Assert.AreEqual(30, behavior.Claim().Value);
        Assert.AreEqual(30, behavior.Claim().Value);
    }

    /// <summary>A return sequence owns a shallow copy of its caller's collection.</summary>
    [TestMethod]
    public void ReturnSequence_SourceMutated_KeepsCapturedValues()
    {
        object?[] source = [10, null, 30];
        var behavior = new MockReturnSequenceBehavior(source);
        source[0] = 99;
        source[1] = 99;
        source[2] = 99;

        Assert.AreEqual(10, behavior.Claim().Value);
        Assert.IsNull(behavior.Claim().Value);
        Assert.AreEqual(30, behavior.Claim().Value);
    }

    /// <summary>An empty return sequence is rejected before any claim can occur.</summary>
    [TestMethod]
    public void ReturnSequence_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new MockReturnSequenceBehavior([]));
    }

    /// <summary>A non-matching setup does not consume its return sequence.</summary>
    [TestMethod]
    public void ReturnSequence_FailedMatch_DoesNotClaimValue()
    {
        var behavior = Sequence(10, 20);
        var store = Store(new MockSetup(Method, [new(1)], behavior));

        var selected = store.Find(Method, [2]);

        Assert.IsNull(selected);
        Assert.AreEqual(10, behavior.Claim().Value);
    }

    /// <summary>A newer matching setup leaves an older return sequence unclaimed.</summary>
    [TestMethod]
    public void ReturnSequence_SupersededSetup_DoesNotClaimValue()
    {
        var sequence = Sequence(10, 20);
        var store = Store(
            new MockSetup(Method, [new(1)], sequence),
            new MockSetup(
                Method,
                [new(1)],
                new MockConstantBehavior(99, [])));

        var selected = store.Find(Method, [1])!.Claim();

        Assert.AreEqual(99, selected.Value);
        Assert.AreEqual(10, sequence.Claim().Value);
    }

    /// <summary>Concurrent claims consume every pre-terminal position once and repeat only the final value.</summary>
    [TestMethod]
    public void ReturnSequence_ConcurrentClaims_HaveExactMultiset()
    {
        const int callerCount = 16;
        var behavior = Sequence(10, 20, 30, 40);
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var results = new int[callerCount];
        var callers = new Task[callerCount];

        for (var i = 0; i < callerCount; i++)
        {
            var resultIndex = i;
            callers[i] = Task.Run(async () =>
            {
                await start.Task;
                results[resultIndex] = (int)behavior.Claim().Value!;
            });
        }

        start.SetResult();
        Task.WaitAll(callers);

        int[] expected =
        [
            10,
            20,
            30,
            40, 40, 40, 40, 40, 40, 40,
            40, 40, 40, 40, 40, 40
        ];
        CollectionAssert.AreEquivalent(expected, results);
    }

    private static MockReturnSequenceBehavior Sequence(params object?[] values) =>
        new(values);

    private static MockSetupStore Store(params MockSetup[] setups)
    {
        var store = new MockSetupStore();
        foreach (var setup in setups)
            store.Add(setup);

        return store;
    }

    private static int Target(int value) => value;
}
