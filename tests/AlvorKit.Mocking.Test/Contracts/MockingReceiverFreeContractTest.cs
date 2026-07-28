namespace AlvorKit.Mocking.Test.Contracts;

/// <summary>Proves the immutable receiver-free public contract foundations.</summary>
[TestClass]
public sealed class MockingReceiverFreeContractTest
{
    /// <summary>Field handles preserve exact metadata and reject false type claims.</summary>
    [TestMethod]
    public void FieldHandle_ValidatesExactMetadata()
    {
        MockField<int> instance =
            Mock.Field<ReceiverFreeFieldOwner, int>("instanceValue");
        MockField<int> global =
            Mock.Field<ReceiverFreeFieldOwner, int>("globalValue");

        Assert.IsFalse(instance.IsStatic);
        Assert.IsTrue(global.IsStatic);
        Assert.AreEqual("instanceValue", instance.Metadata.Name);
        Assert.AreEqual(typeof(int), instance.Metadata.FieldType);

        Assert.ThrowsExactly<MockException>(
            () => Mock.Field<ReceiverFreeFieldOwner, string>("instanceValue"));
        Assert.ThrowsExactly<MockException>(
            () => Mock.Field<ReceiverFreeFieldOwner, int>("missing"));
        Assert.ThrowsExactly<MockException>(
            () => Mock.Field<ReceiverFreeFieldOwner, int>("Literal"));
    }

    /// <summary>Call-site handles reject a different member or operation kind.</summary>
    [TestMethod]
    public void CallSite_ValidatesExactOperation()
    {
        MethodInfo operation = StaticOperation();
        var descriptor = SiteDescriptor(
            MockInvocationOperationKind.StaticMethod);
        var site = new MockCallSite(descriptor, operation);

        site.Validate(
            operation,
            MockInvocationOperationKind.StaticMethod);

        MethodInfo other = typeof(MockingReceiverFreeContractTest)
            .GetMethod(
                nameof(OtherStaticOperation),
                BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.ThrowsExactly<MockException>(
            () => site.Validate(
                other,
                MockInvocationOperationKind.StaticMethod));
        Assert.ThrowsExactly<MockException>(
            () => site.Validate(
                operation,
                MockInvocationOperationKind.Construction));
    }

    /// <summary>AtSite creates a new scope while preserving the member-wide descriptor.</summary>
    [TestMethod]
    public void StaticClause_AtSitePublishesImmutableScope()
    {
        MethodInfo operation = StaticOperation();
        var site = new MockCallSite(
            SiteDescriptor(
                MockInvocationOperationKind.StaticMethod),
            operation);
        MockReceiverFreeSetupDescriptor? publishedScope = null;
        MockReceiverFreeBehavior? publishedBehavior = null;
        var memberScope = new MockReceiverFreeSetupDescriptor(
            operation,
            MockInvocationOperationKind.StaticMethod,
            null,
            []);
        var publisher = new MockReceiverFreeSetupPublisher(
            memberScope,
            (scope, behavior) =>
            {
                publishedScope = scope;
                publishedBehavior = behavior;
            });

        new MockSetupClause<int>(publisher)
            .AtSite(site)
            .Return(42);

        Assert.IsNull(memberScope.Site);
        Assert.AreSame(site, publishedScope!.Site);
        Assert.AreEqual(
            MockReceiverFreeBehaviorKind.Return,
            publishedBehavior!.Kind);
        Assert.AreEqual(42, publishedBehavior.Value);
    }

    /// <summary>Construction and constructor-body clauses retain distinct terminal semantics.</summary>
    [TestMethod]
    public void ConstructionClauses_PublishDistinctBehaviorKinds()
    {
        ConstructorInfo constructor = typeof(ReceiverFreeConstructionTarget)
            .GetConstructor([typeof(int)])!;
        var observed = new List<MockReceiverFreeBehaviorKind>();

        var construction = new MockConstructionSetupClause<
            ReceiverFreeConstructionTarget>(
                Publisher(
                    constructor,
                    MockInvocationOperationKind.Construction,
                    observed));
        construction.Substitute(
            new ReceiverFreeConstructionTarget(1));
        construction.Passthrough();

        var body = new MockConstructorBodySetupClause<
            ReceiverFreeConstructionTarget>(
                Publisher(
                    constructor,
                    MockInvocationOperationKind.ConstructorBody,
                    observed));
        body.Observe(_ => { });
        body.Replace(_ => { });

        var broadConstruction = new MockConstructionSetupClause<object>(
            Publisher(
                constructor,
                MockInvocationOperationKind.Construction,
                observed));
        Assert.ThrowsExactly<MockException>(
            () => broadConstruction.Substitute(new object()));
        Assert.ThrowsExactly<ArgumentNullException>(
            () => broadConstruction.Substitute(null!));

        CollectionAssert.AreEqual(
            new[]
            {
                MockReceiverFreeBehaviorKind.Substitute,
                MockReceiverFreeBehaviorKind.Passthrough,
                MockReceiverFreeBehaviorKind.Observe,
                MockReceiverFreeBehaviorKind.Replace
            },
            observed);
    }

    /// <summary>Typed field clauses retain exact observer and transform delegates.</summary>
    [TestMethod]
    public void FieldClauses_PublishTypedDelegates()
    {
        FieldInfo field = typeof(ReceiverFreeFieldOwner)
            .GetField(
                "instanceValue",
                BindingFlags.NonPublic |
                BindingFlags.Instance)!;
        var owner = new ReceiverFreeFieldOwner();
        MockReceiverFreeBehavior? readBehavior = null;
        MockReceiverFreeBehavior? writeBehavior = null;
        var read = new MockFieldReadSetupClause<int>(
            Publisher(
                field,
                MockInvocationOperationKind.FieldRead,
                owner,
                behavior => readBehavior = behavior));
        var write = new MockFieldWriteSetupClause<int>(
            Publisher(
                field,
                MockInvocationOperationKind.FieldWrite,
                owner,
                behavior => writeBehavior = behavior));
        MockValueObserver<int> observer =
            (scoped in _) => { };
        MockValueTransform<int> transform =
            (scoped in value) => value + 1;

        read.Observe(observer);
        write.Transform(transform);

        Assert.AreSame(observer, readBehavior!.Callback);
        Assert.AreSame(transform, writeBehavior!.Callback);
    }

    /// <summary>Field-write value lambdas bind exact values and matchers to logical parameter zero.</summary>
    [TestMethod]
    public void FieldWriteValueLambda_CapturesOnePattern()
    {
        FieldInfo field = typeof(ReceiverFreeFieldOwner)
            .GetField(
                "instanceValue",
                BindingFlags.NonPublic |
                BindingFlags.Instance)!;
        var owner = new ReceiverFreeFieldOwner();
        using var session = Mock.Session();

        MockReceiverFreeSetupPublisher exact =
            MockReceiverFreeApiBoundary.FieldSetup(
                field,
                MockInvocationOperationKind.FieldWrite,
                owner,
                () => 7);
        MockReceiverFreeSetupPublisher any =
            MockReceiverFreeApiBoundary.FieldSetup(
                field,
                MockInvocationOperationKind.FieldWrite,
                owner,
                () => Arg.Any<int>());

        Assert.AreEqual(7, exact.Descriptor.Patterns[0].Value);
        Assert.IsInstanceOfType<Matcher>(
            any.Descriptor.Patterns[0].Value);
        Assert.IsFalse(Capture.Context.IsActive);
    }

    /// <summary>Behavior setup fails before executing receiver-free code without a session.</summary>
    [TestMethod]
    public void ReceiverFreeSetup_RequiresCurrentSession()
    {
        MockException exception =
            Assert.ThrowsExactly<MockException>(
                () => Mock.WhenNew(
                    () => new ReceiverFreeConstructionTarget(1)));

        StringAssert.Contains(
            exception.Message,
            "requires this mock session to be current");
    }

    /// <summary>Verification site selection is forwarded unchanged to its executor.</summary>
    [TestMethod]
    public void Verification_AtSiteForwardsExactScope()
    {
        MethodInfo operation = StaticOperation();
        var site = new MockCallSite(
            SiteDescriptor(
                MockInvocationOperationKind.StaticMethod),
            operation);
        MockReceiverFreeSetupDescriptor? verified = null;
        var descriptor = new MockReceiverFreeSetupDescriptor(
            operation,
            MockInvocationOperationKind.StaticMethod,
            null,
            []);
        var contract = new MockReceiverFreeVerificationContract(
            descriptor,
            (scope, _, _, _, _, _) => verified = scope);

        new MockVerification(contract)
            .AtSite(site)
            .Once();

        Assert.AreSame(site, verified!.Site);
        Assert.IsNull(descriptor.Site);
    }

    private static MockReceiverFreeSetupPublisher Publisher(
        MemberInfo operation,
        MockInvocationOperationKind operationKind,
        List<MockReceiverFreeBehaviorKind> observed) =>
        Publisher(
            operation,
            operationKind,
            null,
            behavior => observed.Add(behavior.Kind));

    private static MockReceiverFreeSetupPublisher Publisher(
        MemberInfo operation,
        MockInvocationOperationKind operationKind,
        object? receiver,
        Action<MockReceiverFreeBehavior> publish) =>
        new(
            new(
                operation,
                operationKind,
                receiver,
                []),
            (_, behavior) => publish(behavior));

    private static MockInterceptionSiteDescriptor SiteDescriptor(
        MockInvocationOperationKind operationKind) =>
        new(
            Guid.Parse("7e594230-c6e8-4da4-baba-eb2e91278091"),
            0x06000001,
            12,
            operationKind);

    private static MethodInfo StaticOperation() =>
        typeof(MockingReceiverFreeContractTest).GetMethod(
            nameof(ReceiverFreeStaticOperation),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static int ReceiverFreeStaticOperation() => 1;

    private static int OtherStaticOperation() => 2;
}

internal sealed class ReceiverFreeFieldOwner
{
    internal const int Literal = 3;
    private readonly int instanceValue = 1;
    private static readonly int globalValue = 2;

    internal int Sum() => instanceValue + globalValue;
}

internal sealed class ReceiverFreeConstructionTarget(int value)
{
    internal int Value { get; } = value;
}
