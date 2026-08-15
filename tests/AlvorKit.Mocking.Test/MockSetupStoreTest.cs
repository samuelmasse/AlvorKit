namespace AlvorKit;

[TestClass]
public sealed class MockSetupStoreTest
{
    private static readonly MethodInfo Method =
        typeof(MockSetupStoreTest).GetMethod(
            nameof(Target),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Newest matching setups take precedence deterministically.</summary>
    [TestMethod]
    public void Find_ReturnsNewestMatchingBehavior()
    {
        var store = new MockSetupStore();
        store.Add(Setup(1, 10));
        store.Add(Setup(1, 20));

        var execution = store.Find(Method, [1])!.Claim();

        Assert.AreEqual(20, execution.Value);
    }

    /// <summary>Setup patterns are copied before immutable publication.</summary>
    [TestMethod]
    public void Add_CopiesCapturedArgumentPatterns()
    {
        MockArgumentPattern[] arguments = [new(1)];
        var setup = new MockSetup(
            Method,
            arguments,
            new MockConstantBehavior(10, []));
        arguments[0] = new(2);

        var store = new MockSetupStore();
        store.Add(setup);

        Assert.IsNotNull(store.Find(Method, [1]));
        Assert.IsNull(store.Find(Method, [2]));
    }

    /// <summary>Predicate patterns discriminate actual invocation arguments.</summary>
    [TestMethod]
    public void Find_ExecutesPredicatePattern()
    {
        Func<object, bool> predicate = value => (int)value > 10;
        MockArgumentPattern[] arguments =
        [
            new(new Matcher(MatcherType.Func, predicate))
        ];
        var store = new MockSetupStore();
        store.Add(new(
            Method,
            arguments,
            new MockConstantBehavior(30, [])));

        Assert.IsNull(store.Find(Method, [10]));
        Assert.IsNotNull(store.Find(Method, [11]));
    }

    /// <summary>Constant behaviors own their reference-writeback collection.</summary>
    [TestMethod]
    public void ConstantBehavior_CopiesReferenceValues()
    {
        object?[] references = [10];
        var behavior = new MockConstantBehavior(20, references);
        references[0] = 30;

        var execution = behavior.Claim();

        Assert.AreEqual(10, execution.ReferenceValues[0]);
    }

    private static MockSetup Setup(int argument, int result) =>
        new(
            Method,
            [new(argument)],
            new MockConstantBehavior(result, []));

    private static int Target(int value) => value;
}
