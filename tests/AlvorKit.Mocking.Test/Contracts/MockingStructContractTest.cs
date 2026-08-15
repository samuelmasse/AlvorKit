namespace AlvorKit;

/// <summary>Proves the immutable struct-mocking public-contract foundations.</summary>
[TestClass]
public sealed class MockingStructContractTest
{
    /// <summary>Type, live-value, and site modes remain exclusive and immutable.</summary>
    [TestMethod]
    public void Scope_ExposesThreeExclusiveModes()
    {
        MockStructScope<StructContractValue> typeWide =
            Mock.Struct<StructContractValue>();
        MockStructScope<StructContractValue> matching =
            typeWide.Matching(
                (scoped in value) =>
                    value.Value == 7);
        MockCallSite site = CreateSite(20);
        MockStructScope<StructContractValue> atSite =
            typeWide.AtSite(site);

        Assert.AreEqual(MockStructMode.TypeWide, typeWide.Mode);
        Assert.AreEqual(MockStructMode.ValueMatched, matching.Mode);
        Assert.AreEqual(MockStructMode.CallSite, atSite.Mode);
        Assert.AreSame(site, atSite.Descriptor.Site);
        Assert.ThrowsExactly<MockException>(
            () => matching.AtSite(site));
        Assert.ThrowsExactly<MockException>(
            () => atSite.Matching(
                (scoped in _) => true));
    }

    /// <summary>Assignment and boxing are independent values reevaluated at entry.</summary>
    [TestMethod]
    public void ValuePredicate_ReevaluatesAssignedAndBoxedCopies()
    {
        var original = new StructContractValue { Value = 7 };
        StructContractValue assigned = original;
        object boxed = original;
        original.Value = 9;
        StructContractValue unboxedCopy =
            (StructContractValue)boxed;
        MockStructScopeDescriptor matching =
            Mock.Struct<StructContractValue>()
                .Matching(
                    (scoped in value) =>
                        value.Value == 7)
                .Descriptor;

        Assert.IsFalse(matching.MatchesEntry(in original));
        Assert.IsTrue(matching.MatchesEntry(in assigned));
        Assert.IsTrue(matching.MatchesEntry(in unboxedCopy));
        Assert.IsNull(matching.Site);
        Assert.IsNotNull(matching.Predicate);
    }

    /// <summary>Equal receiver values remain distinguishable by opaque site identity.</summary>
    [TestMethod]
    public void SiteMode_DistinguishesEqualValuesBySite()
    {
        MockCallSite first = CreateSite(20);
        MockCallSite second = CreateSite(24);
        MockStructScopeDescriptor firstScope =
            Mock.Struct<StructContractValue>()
                .AtSite(first)
                .Descriptor;
        MockStructScopeDescriptor secondScope =
            Mock.Struct<StructContractValue>()
                .AtSite(second)
                .Descriptor;

        Assert.AreNotSame(firstScope.Site, secondScope.Site);
        Assert.AreNotEqual(
            firstScope.Site!.ToString(),
            secondScope.Site!.ToString());
        Assert.AreEqual(
            MockStructMode.CallSite,
            firstScope.Mode);
        Assert.AreEqual(
            MockStructMode.CallSite,
            secondScope.Mode);
    }

    /// <summary>Operation descriptors reject closures that could retain a receiver copy.</summary>
    [TestMethod]
    public void OperationDescriptor_RequiresStaticCapture()
    {
        var retained = new StructContractValue { Value = 1 };
        MockStructCall<StructContractValue> capturing =
            (scoped ref value) =>
                value.Value += retained.Value;

        MockException exception =
            Assert.ThrowsExactly<MockException>(
                () => new MockStructSetupDescriptor(
                    Mock.Struct<StructContractValue>()
                        .Descriptor,
                    capturing,
                    typeof(void)));

        StringAssert.Contains(
            exception.Message,
            "must not close over state");
    }

    /// <summary>Projection and mutation registration returns new immutable descriptors.</summary>
    [TestMethod]
    public void Clause_PublishesImmutableThisPhases()
    {
        MockStructSetupDescriptor? published = null;
        MockStructBehavior? behavior = null;
        MockStructSetupPublisher publisher = Publisher(
            (descriptor, selected) =>
            {
                published = descriptor;
                behavior = selected;
            });
        var original = new MockStructSetupClause<
            StructContractValue>(publisher);

        original
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
            .Passthrough();

        Assert.AreEqual(0, publisher.Descriptor.Projections.Length);
        Assert.AreEqual(0, publisher.Descriptor.Mutations.Length);
        Assert.AreEqual(2, published!.Projections.Length);
        Assert.AreEqual(2, published.Mutations.Length);
        Assert.AreEqual(
            MockSnapshotPhase.Entry,
            published.Projections[0].Phase);
        Assert.AreEqual(
            MockSnapshotPhase.Exit,
            published.Projections[1].Phase);
        Assert.AreEqual(
            MockSnapshotPhase.Entry,
            published.Mutations[0].Phase);
        Assert.AreEqual(
            MockSnapshotPhase.Exit,
            published.Mutations[1].Phase);
        Assert.AreEqual(
            MockStructBehaviorKind.Passthrough,
            behavior!.Kind);
    }

    /// <summary>Struct verification forwards exact count and immutable scope metadata.</summary>
    [TestMethod]
    public void Verification_ForwardsExactContract()
    {
        MockStructSetupDescriptor descriptor =
            Descriptor();
        MockStructSetupDescriptor? verified = null;
        MockVerificationCountKind? observedKind = null;
        var contract = new MockStructVerificationContract(
            descriptor,
            (scope, kind, _, _, _, _) =>
            {
                verified = scope;
                observedKind = kind;
            });

        new MockStructVerification(contract).Once();

        Assert.AreSame(descriptor, verified);
        Assert.AreEqual(
            MockVerificationCountKind.Exactly,
            observedKind);
    }

    /// <summary>An operation outside Interception's selected closure fails during capture.</summary>
    [TestMethod]
    public void Boundary_UninterceptionOperationFailsActionably()
    {
        using var session = Mock.Session();
        MockStructSetupClause<StructContractValue> clause =
            Mock.Struct<StructContractValue>()
                .When(
                    static (
                        scoped ref value) =>
                        value.Increment());

        MockException exception =
            Assert.ThrowsExactly<MockException>(
                clause.Passthrough);

        StringAssert.Contains(
            exception.Message,
            "Failed to capture one mocked call");
    }

    /// <summary>Struct capture requires the setup-owning session to be current.</summary>
    [TestMethod]
    public void Boundary_RequiresCurrentSession()
    {
        MockException exception =
            Assert.ThrowsExactly<MockException>(
                () => Mock.Struct<StructContractValue>()
                    .When(
                        static (
                            scoped ref value) =>
                            value.Increment()));

        StringAssert.Contains(
            exception.Message,
            "requires this mock session to be current");
    }

    private static MockStructSetupPublisher Publisher(
        Action<MockStructSetupDescriptor, MockStructBehavior> publish) =>
        new(Descriptor(), publish);

    private static MockStructSetupDescriptor Descriptor()
    {
        MockStructCall<StructContractValue> operation =
            static (scoped ref value) =>
                value.Increment();
        return new(
            Mock.Struct<StructContractValue>().Descriptor,
            operation,
            typeof(void));
    }

    private static MockCallSite CreateSite(int offset)
    {
        MethodInfo operation = typeof(MockingStructContractTest)
            .GetMethod(
                nameof(SiteOperation),
                BindingFlags.NonPublic |
                BindingFlags.Static)!;
        return new(
            new(
                Guid.Parse(
                    "41afc6bd-c454-4ee0-abcf-53475faacfb1"),
                0x06000001,
                offset,
                MockInvocationOperationKind.StaticMethod),
            operation);
    }

    private static void SiteOperation()
    {
    }
}

internal struct StructContractValue
{
    internal int Value;

    internal void Increment() => Value++;
}
