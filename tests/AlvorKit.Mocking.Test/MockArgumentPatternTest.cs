namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockArgumentPatternTest
{
    private static readonly MethodInfo Method =
        typeof(MockArgumentPatternTest).GetMethod(
            nameof(Target),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Exact patterns use ordinary shallow equality for values and references.</summary>
    [TestMethod]
    public void Matches_ExactPattern_UsesShallowEquality()
    {
        int[] expected = [1, 2];
        var valuePattern = new MockArgumentPattern(42);
        var referencePattern = new MockArgumentPattern(expected);

        Assert.IsTrue(valuePattern.Matches(42));
        Assert.IsFalse(valuePattern.Matches(43));
        Assert.IsTrue(referencePattern.Matches(expected));
        Assert.IsFalse(referencePattern.Matches(new[] { 1, 2 }));
    }

    /// <summary>An exact null pattern matches only null.</summary>
    [TestMethod]
    public void Matches_ExactNullPattern_MatchesOnlyNull()
    {
        var pattern = new MockArgumentPattern(null);

        Assert.IsTrue(pattern.Matches(null));
        Assert.IsFalse(pattern.Matches("value"));
    }

    /// <summary>An Any pattern accepts both null and non-null arguments.</summary>
    [TestMethod]
    public void Matches_AnyPattern_AcceptsEveryValue()
    {
        var pattern = Pattern(new(MatcherType.Any, null));

        Assert.IsTrue(pattern.Matches(null));
        Assert.IsTrue(pattern.Matches(42));
    }

    /// <summary>A predicate pattern evaluates once and returns the predicate result.</summary>
    [TestMethod]
    public void Matches_PredicatePattern_EvaluatesPredicateOnce()
    {
        var calls = 0;
        var pattern = Predicate(value =>
        {
            calls++;
            return (int)value > 10;
        });

        Assert.IsFalse(pattern.Matches(10));
        Assert.IsTrue(pattern.Matches(11));
        Assert.AreEqual(2, calls);
    }

    /// <summary>A predicate is not called for null because ordinary predicate capture requires a value.</summary>
    [TestMethod]
    public void Matches_PredicatePattern_DoesNotEvaluateNull()
    {
        var calls = 0;
        var pattern = Predicate(_ =>
        {
            calls++;
            return true;
        });

        Assert.IsFalse(pattern.Matches(null));
        Assert.AreEqual(0, calls);
    }

    /// <summary>Predicate exceptions escape matching unchanged.</summary>
    [TestMethod]
    public void Matches_PredicateThrows_PropagatesSameException()
    {
        var expected = new InvalidOperationException("predicate failed");
        var pattern = Predicate(_ => throw expected);

        var actual = Assert.Throws<InvalidOperationException>(
            () => pattern.Matches(1));

        Assert.AreSame(expected, actual);
    }

    /// <summary>Pattern descriptions never evaluate a captured predicate.</summary>
    [TestMethod]
    public void Description_PredicatePattern_DoesNotEvaluatePredicate()
    {
        var calls = 0;
        var pattern = Predicate(_ =>
        {
            calls++;
            return true;
        });

        var description = pattern.Description;

        Assert.AreEqual("predicate", description);
        Assert.AreEqual(0, calls);
    }

    /// <summary>A reentrant predicate can publish a setup without changing the active immutable search snapshot.</summary>
    [TestMethod]
    public void Find_ReentrantPredicate_PreservesActiveSnapshot()
    {
        var store = new MockSetupStore();
        var predicateCalls = 0;
        var original = new MockConstantBehavior(10, []);
        var reentrant = new MockConstantBehavior(20, []);

        store.Add(new(
            Method,
            [Predicate(_ =>
            {
                predicateCalls++;
                store.Add(new(Method, [new(1)], reentrant));
                return true;
            })],
            original));

        var first = store.Find(Method, [1]);
        var second = store.Find(Method, [1]);

        Assert.AreSame(original, first);
        Assert.AreSame(reentrant, second);
        Assert.AreEqual(1, predicateCalls);
    }

    /// <summary>Descriptions distinguish exact null, exact values, Any, and predicates without user formatting.</summary>
    [TestMethod]
    public void Description_AllPatternKinds_IsDeterministic()
    {
        Assert.AreEqual("exact null", new MockArgumentPattern(null).Description);
        Assert.AreEqual("exact value", new MockArgumentPattern(new object()).Description);
        Assert.AreEqual("any value", Pattern(new(MatcherType.Any, null)).Description);
        Assert.AreEqual("predicate", Predicate(_ => true).Description);
    }

    private static MockArgumentPattern Predicate(Func<object, bool> predicate) =>
        Pattern(new(MatcherType.Func, predicate));

    private static MockArgumentPattern Pattern(Matcher matcher) => new(matcher);

    private static int Target(int value) => value;
}
