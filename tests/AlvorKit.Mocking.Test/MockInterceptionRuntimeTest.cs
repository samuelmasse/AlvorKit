namespace AlvorKit.Mocking.Test;

/// <summary>Exercises concrete operation wrappers through the Interception runtime.</summary>
[TestClass]
public sealed class MockInterceptionRuntimeTest
{
    private static int nextOffset;

    static MockInterceptionRuntimeTest()
    {
        foreach (MethodInfo method in typeof(InterceptionRuntimeTarget).GetMethods(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly))
        {
            MockInterceptionOperationRuntime.Register(method);
        }
    }

    /// <summary>Repeated eager registration preserves one operation ownership record.</summary>
    [TestMethod]
    public void Register_RepeatedMethod_IsIdempotent()
    {
        MethodInfo method = typeof(InterceptionRuntimeTarget).GetMethod(
            nameof(InterceptionRuntimeTarget.Add))!;

        MockInterceptionOperationRuntime.Register(method);
        MockInterceptionOperationRuntime.Register(method);

        Assert.IsTrue(MockInterceptionMethodRegistry.Contains(method));
    }

    /// <summary>Unmocked, null, configured, strict, and loose receivers preserve their distinct paths.</summary>
    [TestMethod]
    public void Bind_OrdinaryReceivers_PreserveFallbackContracts()
    {
        InterceptionIntCall call = Bind(
            nameof(InterceptionRuntimeTarget.Add),
            new InterceptionIntCall(InterceptionRuntimeOriginal.Add));
        var unmocked = new InterceptionRuntimeTarget();

        Assert.AreEqual(12, call(unmocked, 2));
        Assert.AreEqual(1, unmocked.Calls);
        Assert.ThrowsExactly<NullReferenceException>(
            () => call(null!, 2));

        var strict = Mock.Create<InterceptionRuntimeTarget>();
        Mock.When(() => call(strict, 2)).Return(71);
        Assert.AreEqual(71, call(strict, 2));
        Assert.ThrowsExactly<MockException>(
            () => call(strict, 3));
        Assert.AreEqual(0, strict.Calls);
        Assert.IsTrue(Snapshot(strict).All(invocation =>
            invocation.Identity.Backend == MockBackendLabel.InterceptionInstance));

        var loose = Mock.CreateLoose<InterceptionRuntimeTarget>();
        Assert.AreEqual(0, call(loose, 5));
        Assert.AreEqual(0, loose.Calls);
    }

    /// <summary>A partial wrapper completes configured, original return, original throw, and ref/out calls once.</summary>
    [TestMethod]
    public void Bind_PartialReceiver_CompletesOriginalPathsExactlyOnce()
    {
        InterceptionIntCall add = Bind(
            nameof(InterceptionRuntimeTarget.Add),
            new InterceptionIntCall(InterceptionRuntimeOriginal.Add));
        InterceptionThrowCall throwing = Bind(
            nameof(InterceptionRuntimeTarget.Throw),
            new InterceptionThrowCall(InterceptionRuntimeOriginal.Throw));
        InterceptionRefOutCall mutate = Bind(
            nameof(InterceptionRuntimeTarget.Mutate),
            new InterceptionRefOutCall(InterceptionRuntimeOriginal.Mutate));
        var expected = new IOException("interception original");
        var target = Mock.Partial(new InterceptionRuntimeTarget(expected));
        Mock.When(() => add(target, 2)).Return(83);

        Assert.AreEqual(83, add(target, 2));
        Assert.AreEqual(13, add(target, 3));
        Exception actual = Assert.ThrowsExactly<IOException>(
            () => throwing(target));
        int value = 4;
        Assert.AreEqual(7, mutate(target, ref value, out int doubled));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(7, value);
        Assert.AreEqual(14, doubled);
        Assert.AreEqual(3, target.Calls);
        MockInvocation[] invocations = Snapshot(target);
        Assert.AreEqual(4, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Identity.Backend == MockBackendLabel.InterceptionInstance));
        Assert.AreEqual(
            2,
            invocations.Count(invocation =>
                invocation.Completion.Source ==
                MockInvocationExecutionSource.PartialPassthrough &&
                invocation.Completion.Kind ==
                MockInvocationCompletionKind.Returned));
        MockInvocation thrown = invocations.Single(invocation =>
            invocation.Identity.Operation.Name ==
            nameof(InterceptionRuntimeTarget.Throw));
        Assert.AreSame(expected, thrown.Completion.Exception);
        Assert.AreEqual(
            MockInvocationFailureStage.OriginalImplementation,
            thrown.Completion.FailureStage);
        MockInvocation changed = invocations.Single(invocation =>
            invocation.Identity.Operation.Name ==
            nameof(InterceptionRuntimeTarget.Mutate));
        Assert.AreEqual(7, changed.Arguments[0].Exit.Value);
        Assert.AreEqual(14, changed.Arguments[1].Exit.Value);
    }

    /// <summary>Typed span callbacks and borrowed return factories remain entirely in the exact frame.</summary>
    [TestMethod]
    public void Bind_RefStructBehavior_ExecutesExactCallbackAndFactory()
    {
        InterceptionSpanCall sum = Bind(
            nameof(InterceptionRuntimeTarget.Sum),
            new InterceptionSpanCall(InterceptionRuntimeOriginal.Sum));
        InterceptionSpanReturnCall view = Bind(
            nameof(InterceptionRuntimeTarget.View),
            new InterceptionSpanReturnCall(InterceptionRuntimeOriginal.View));
        var target = Mock.Create<InterceptionRuntimeTarget>();
        int[] owned = [13, 21, 34];
        Mock.When(() => sum(
                target,
                Arg.Any<ReadOnlySpan<int>>(0)))
            .Answer((ReadOnlySpan<int> values) =>
                values.ToArray().Sum());
        Mock.When(() => view(target))
            .ReturnFactory(() => owned.AsSpan());

        Assert.AreEqual(6, sum(target, [1, 2, 3]));
        ReadOnlySpan<int> returned = view(target);

        Assert.IsTrue(returned.SequenceEqual(owned));
        Assert.AreEqual(0, target.Calls);
        MockInvocation[] invocations = Snapshot(target);
        Assert.AreEqual(2, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Source ==
            MockInvocationExecutionSource.Configured));
    }

    /// <summary>Configured and partial mutable/read-only aliases preserve exact storage identity.</summary>
    [TestMethod]
    public void Bind_ManagedReferenceReturn_PreservesAliasIdentity()
    {
        InterceptionRefReturnCall mutable = Bind(
            nameof(InterceptionRuntimeTarget.Mutable),
            new InterceptionRefReturnCall(InterceptionRuntimeOriginal.Mutable));
        InterceptionRefReadonlyReturnCall readOnly = Bind(
            nameof(InterceptionRuntimeTarget.ReadOnly),
            new InterceptionRefReadonlyReturnCall(InterceptionRuntimeOriginal.ReadOnly));
        var owner = new InterceptionAliasOwner([55, 89]);
        var configured = Mock.Create<InterceptionRuntimeTarget>();
        Mock.WhenRef(() => ref mutable(configured))
            .ReturnRef(owner.Mutable);
        Mock.WhenRefReadonly(() => ref readOnly(configured))
            .ReturnRef(owner.ReadOnly);

        ref int configuredMutable = ref mutable(configured);
        ref readonly int configuredReadOnly = ref readOnly(configured);
        ref int expectedMutable = ref owner.Mutable();
        ref readonly int expectedReadOnly = ref owner.ReadOnly();

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref configuredMutable,
                ref expectedMutable));
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref System.Runtime.CompilerServices.Unsafe.AsRef(
                    in configuredReadOnly),
                ref System.Runtime.CompilerServices.Unsafe.AsRef(
                    in expectedReadOnly)));

        var partial = Mock.Partial(
            new InterceptionRuntimeTarget(values: [144, 233]));
        ref int partialMutable = ref mutable(partial);
        ref int originalMutable = ref partial.Mutable();
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref partialMutable,
                ref originalMutable));
        Assert.AreEqual(
            MockInvocationExecutionSource.PartialPassthrough,
            Snapshot(partial).Single().Completion.Source);
    }

    /// <summary>Distinct constructed generic operations bind and select without cross-contamination.</summary>
    [TestMethod]
    public void Bind_ConstructedGenericMethods_RemainIndependent()
    {
        MethodInfo definition = Method(
            nameof(InterceptionRuntimeTarget.Echo));
        MethodInfo stringMethod =
            definition.MakeGenericMethod(typeof(string));
        MethodInfo intMethod =
            definition.MakeGenericMethod(typeof(int));
        InterceptionGenericCall<string> strings = Bind(
            stringMethod,
            new InterceptionGenericCall<string>(
                InterceptionRuntimeOriginal.Echo<string>));
        InterceptionGenericCall<int> integers = Bind(
            intMethod,
            new InterceptionGenericCall<int>(
                InterceptionRuntimeOriginal.Echo<int>));
        var target = Mock.Create<InterceptionRuntimeTarget>();
        Mock.When(() => strings(target, "a"))
            .Answer((string value) => value + "!");
        Mock.When(() => integers(target, 5)).Return(1597);

        Assert.AreEqual("a!", strings(target, "a"));
        Assert.AreEqual(1597, integers(target, 5));
        Assert.AreEqual(0, target.Calls);
        Assert.AreEqual(2, Snapshot(target).Length);
    }

    /// <summary>Static interception sites use session-owned setup, history, passthrough, and verification state.</summary>
    [TestMethod]
    public void Bind_StaticMethod_IsSessionScopedAndMemberWide()
    {
        MethodInfo method = Method(
            nameof(InterceptionRuntimeTarget.StaticDouble));
        Func<int, int> first = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.StaticMethod),
            method,
            new Func<int, int>(
                InterceptionRuntimeTarget.StaticDouble));
        Func<int, int> second = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.StaticMethod),
            method,
            new Func<int, int>(
                InterceptionRuntimeTarget.StaticDouble));

        Assert.AreEqual(8, first(4));

        using (MockSession session = Mock.Session())
        {
            Mock.When(() => first(4)).Return(71);

            Assert.AreEqual(71, first(4));
            Assert.AreEqual(10, second(5));
            Mock.Verify(() => first(4)).Once();
            Mock.Verify(() => first(5)).Once();

            MockInvocation[] invocations =
                session.SnapshotThrough(session.Checkpoint());
            Assert.AreEqual(2, invocations.Length);
            Assert.IsTrue(invocations.All(invocation =>
                invocation.Identity.Target.Kind ==
                    MockInvocationTargetKind.CallSite &&
                invocation.Identity.Target.OwnerId == session.Id &&
                invocation.Identity.Backend ==
                    MockBackendLabel.InterceptionReceiverFree));
            Assert.AreNotEqual(
                invocations[0].Identity.Target.IlOffset,
                invocations[1].Identity.Target.IlOffset);
            Assert.AreEqual(
                MockInvocationExecutionSource.Configured,
                invocations[0].Completion.Source);
            Assert.AreEqual(
                MockInvocationExecutionSource.ReceiverFreeOriginal,
                invocations[1].Completion.Source);
        }

        Assert.AreEqual(8, second(4));
    }

    /// <summary>Construction and field wrappers preserve originals outside a session and accept session setup.</summary>
    [TestMethod]
    public void Bind_ConstructionAndFields_UseExactReceiverFreeDispatch()
    {
        ConstructorInfo constructor = typeof(InterceptionRuntimeTarget)
            .GetConstructors(
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .Single();
        Func<Exception?, int[]?, InterceptionRuntimeTarget> construction =
            MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.Construction),
            constructor,
            new Func<Exception?, int[]?, InterceptionRuntimeTarget>(
                static (failure, values) =>
                    new InterceptionRuntimeTarget(failure, values)));
        FieldInfo field = typeof(InterceptionRuntimeTarget).GetField(
            nameof(InterceptionRuntimeTarget.LastValue))!;
        Func<int> read = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.FieldRead),
            field,
            new Func<int>(
                static () => InterceptionRuntimeTarget.LastValue));
        Action<int> write = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.FieldWrite),
            field,
            new Action<int>(
                static value =>
                    InterceptionRuntimeTarget.LastValue = value));

        InterceptionRuntimeTarget.LastValue = 3;
        Assert.IsNotNull(construction(null, null));
        Assert.AreEqual(3, read());
        write(5);
        Assert.AreEqual(5, InterceptionRuntimeTarget.LastValue);

        using (Mock.Session())
        {
            var substitute = new InterceptionRuntimeTarget();
            MockField<int> fieldHandle =
                Mock.Field<int>(field);
            Mock.WhenNew(
                    () => construction(null, null))
                .Substitute(substitute);
            Mock.WhenFieldRead(fieldHandle)
                .Return(89);
            Mock.WhenFieldWrite(
                    fieldHandle,
                    () => 13)
                .Throw(
                new IOException("field write"));

            Assert.AreSame(
                substitute,
                construction(null, null));
            Assert.AreEqual(89, read());
            Assert.ThrowsExactly<IOException>(
                () => write(13));
            write(21);
            Assert.AreEqual(21, InterceptionRuntimeTarget.LastValue);
            Mock.VerifyNew(
                () => construction(null, null)).Once();
            Mock.VerifyFieldRead(fieldHandle).Once();
            Mock.VerifyFieldWrite(
                fieldHandle,
                () => 13).Once();
            Mock.VerifyFieldWrite(
                fieldHandle,
                () => 21).Once();
        }
    }

    /// <summary>Constructor-body wrappers observe or replace only the post-initializer remainder.</summary>
    [TestMethod]
    public void Bind_ConstructorBody_PreservesIdentityAndControlsRemainder()
    {
        ConstructorInfo constructor = typeof(InterceptionConstructorBodyTarget)
            .GetConstructor([typeof(int)])!;
        InterceptionConstructorBodyCall body = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.ConstructorBody),
            constructor,
            new InterceptionConstructorBodyCall(
                static (target, value) =>
                    target.ApplyRemainder(value)));

        using (Mock.Session())
        {
            InterceptionConstructorBodyTarget? observed = null;
            Mock.WhenConstructorBody(
                    () => Construct(body, 5))
                .Observe(
                    new InterceptionConstructorBodyCall(
                        (target, value) =>
                        {
                            observed = target;
                            target.ObservedArgument = value;
                        }));
            Mock.WhenConstructorBody(
                    () => Construct(body, 7))
                .Replace(
                    target =>
                        target.ReplacementRan = true);

            InterceptionConstructorBodyTarget original =
                Construct(body, 5);
            InterceptionConstructorBodyTarget replacement =
                Construct(body, 7);
            InterceptionConstructorBodyTarget passthrough =
                Construct(body, 11);

            Assert.AreSame(original, observed);
            Assert.AreEqual(5, original.ObservedArgument);
            Assert.AreEqual(5, original.Value);
            Assert.AreEqual(1, original.Remainders);
            Assert.IsTrue(replacement.ReplacementRan);
            Assert.AreEqual(0, replacement.Value);
            Assert.AreEqual(0, replacement.Remainders);
            Assert.AreEqual(11, passthrough.Value);
            Assert.AreEqual(1, passthrough.Remainders);
            Mock.VerifyConstructorBody(
                () => Construct(body, 5)).Once();
            Mock.VerifyConstructorBody(
                () => Construct(body, 7)).Once();
            Mock.VerifyConstructorBody(
                () => Construct(body, 11)).Once();
        }
    }

    /// <summary>Constructor-body failures record the configured or original stage exactly once.</summary>
    [TestMethod]
    public void Bind_ConstructorBody_RecordsConfiguredAndOriginalFailures()
    {
        ConstructorInfo constructor = typeof(InterceptionConstructorBodyTarget)
            .GetConstructor([typeof(int)])!;
        InterceptionConstructorBodyCall body = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.ConstructorBody),
            constructor,
            new InterceptionConstructorBodyCall(
                static (target, value) =>
                    target.ApplyRemainder(value)));
        var configured = new IOException("configured constructor body");

        using MockSession session = Mock.Session();
        Mock.WhenConstructorBody(
                () => Construct(body, -1))
            .Throw(configured);

        Assert.AreSame(
            configured,
            Assert.ThrowsExactly<IOException>(
                () => Construct(body, -1)));
        InvalidOperationException original =
            Assert.ThrowsExactly<InvalidOperationException>(
                () => Construct(body, -2));

        StringAssert.Contains(original.Message, "-2");
        MockInvocation[] invocations =
            session.SnapshotThrough(session.Checkpoint());
        Assert.AreEqual(2, invocations.Length);
        Assert.AreEqual(
            MockInvocationExecutionSource.Configured,
            invocations[0].Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.Behavior,
            invocations[0].Completion.FailureStage);
        Assert.AreSame(
            configured,
            invocations[0].Completion.Exception);
        Assert.AreEqual(
            MockInvocationExecutionSource.ReceiverFreeOriginal,
            invocations[1].Completion.Source);
        Assert.AreEqual(
            MockInvocationFailureStage.OriginalImplementation,
            invocations[1].Completion.FailureStage);
        Assert.AreSame(
            original,
            invocations[1].Completion.Exception);
    }

    /// <summary>Constructor callbacks keep ref-struct arguments in the exact typed frame.</summary>
    [TestMethod]
    public void Bind_ConstructorBody_PreservesRefStructArguments()
    {
        ConstructorInfo constructor = typeof(InterceptionConstructorBodyTarget)
            .GetConstructor([typeof(ReadOnlySpan<int>)])!;
        InterceptionConstructorSpanBodyCall body = MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.ConstructorBody),
            constructor,
            new InterceptionConstructorSpanBodyCall(
                static (target, values) =>
                    target.ApplyRemainder(values)));

        using (Mock.Session())
        {
            Mock.WhenConstructorBody(
                    () => Construct(
                        body,
                        Arg.Any<ReadOnlySpan<int>>(0)))
                .Observe(
                    new InterceptionConstructorSpanBodyCall(
                        static (target, values) =>
                            target.ObservedArgument = values.Length));

            InterceptionConstructorBodyTarget target =
                Construct(body, [2, 3, 5]);

            Assert.AreEqual(3, target.ObservedArgument);
            Assert.AreEqual(10, target.Value);
            Assert.AreEqual(1, target.Remainders);
            Mock.VerifyConstructorBody(
                    () => Construct(
                        body,
                        Arg.Any<ReadOnlySpan<int>>(0)))
                .Once();
        }
    }

    /// <summary>Descriptor and member mismatches fail before a wrapper is installed.</summary>
    [TestMethod]
    public void Bind_ReceiverFreeMismatch_FailsActionably()
    {
        ConstructorInfo constructor = typeof(InterceptionRuntimeTarget)
            .GetConstructors(
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .Single();
        static InterceptionRuntimeTarget construction() => new();

        MockException error = Assert.ThrowsExactly<MockException>(
            () => MockInterceptionOperationRuntime.Bind(
                Site(MockInvocationOperationKind.StaticMethod),
                constructor,
construction));
        StringAssert.Contains(error.Message, "Interception site");
        StringAssert.Contains(error.Message, "StaticMethod");
    }

    /// <summary>Runtime caches retain metadata and generated code but no mock, setup, session, or user behavior.</summary>
    [TestMethod]
    public void InterceptionCaches_ContainNoPerMockOrUserState()
    {
        Type[] forbidden =
        [
            typeof(Mocked),
            typeof(MockSetup),
            typeof(MockSession),
            typeof(MockInvocationLedger),
            typeof(MockConfiguredBehavior)
        ];

        foreach (FieldInfo field in typeof(MockInterceptionWrapperCache).GetFields(
            BindingFlags.Static |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic))
        {
            Assert.IsFalse(
                forbidden.Any(type =>
                    field.FieldType == type ||
                    field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericArguments().Contains(type)),
                $"{field.Name} retains {field.FieldType}.");
            Assert.IsFalse(
                typeof(Delegate).IsAssignableFrom(field.FieldType),
                $"{field.Name} is a static or cache-owned delegate.");
        }
    }

    private static TDelegate Bind<TDelegate>(
        string methodName,
        TDelegate original)
        where TDelegate : Delegate =>
        Bind(Method(methodName), original);

    private static TDelegate Bind<TDelegate>(
        MethodInfo method,
        TDelegate original)
        where TDelegate : Delegate =>
        MockInterceptionOperationRuntime.Bind(
            Site(MockInvocationOperationKind.InstanceMethod),
            method,
            original);

    private static MockInterceptionSiteDescriptor Site(
        MockInvocationOperationKind kind) =>
        new(
            typeof(MockInterceptionRuntimeTest).Module.ModuleVersionId,
            typeof(MockInterceptionRuntimeTest).MetadataToken,
            Interlocked.Increment(ref nextOffset),
            kind);

    private static MethodInfo Method(string name) =>
        typeof(InterceptionRuntimeTarget).GetMethod(
            name,
            BindingFlags.Static |
            BindingFlags.Instance |
            BindingFlags.Public)!;

    private static MockInvocation[] Snapshot(object target) =>
        [.. Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];

    private static InterceptionConstructorBodyTarget Construct(
        InterceptionConstructorBodyCall body,
        int value)
    {
        var target = new InterceptionConstructorBodyTarget();
        body(target, value);
        return target;
    }

    private static InterceptionConstructorBodyTarget Construct(
        InterceptionConstructorSpanBodyCall body,
        ReadOnlySpan<int> values)
    {
        var target = new InterceptionConstructorBodyTarget();
        body(target, values);
        return target;
    }
}
