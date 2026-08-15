namespace AlvorKit;

[TestClass]
public sealed class MockRefReturnTest
{
    /// <summary>Configured span factories return the owner's current mutable storage on every matching interface call.</summary>
    [TestMethod]
    public void InterfaceFactory_SpanReturnsCurrentBackingOnEveryCall()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var owner = new RefReturnOwner([1, 2, 3]);
        Mock.When(target.Mutable).ReturnFactory(owner.Mutable);

        Span<int> first = target.Mutable();
        first[1] = 20;
        owner.Replace([4, 5]);
        Span<int> second = target.Mutable();

        CollectionAssert.AreEqual(new[] { 1, 20, 3 }, first.ToArray());
        CollectionAssert.AreEqual(new[] { 4, 5 }, second.ToArray());
        Assert.AreEqual(2, owner.FactoryCalls);
        Assert.AreEqual(
            2,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>Read-only spans and arbitrary ref structs stay live while the ledger retains only borrowed-return metadata.</summary>
    [TestMethod]
    public void InterfaceFactory_BorrowedReturnsNeverEnterControlPlane()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var owner = new RefReturnOwner([8, 13, 21]);
        Mock.When(target.ReadOnly).ReturnFactory(owner.ReadOnly);
        Mock.When(target.View).ReturnFactory(owner.View);

        ReadOnlySpan<int> span = target.ReadOnly();
        BorrowedView view = target.View();

        Assert.IsTrue(span.SequenceEqual([8, 13, 21]));
        Assert.AreEqual(42, view.Sum);
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(2, invocations.Length);
        foreach (MockInvocation invocation in invocations)
        {
            Assert.AreEqual(
                MockInvocationReturnKind.Unavailable,
                invocation.Completion.Return!.Kind);
            Assert.AreEqual(
                MockUnavailableReason.BorrowedReturnNotRetained,
                invocation.Completion.Return.UnavailableReason);
            Assert.IsNull(invocation.Completion.Return.Value);
        }
    }

    /// <summary>A typed return factory propagates its exact exception instance and records the return-factory failure stage once.</summary>
    [TestMethod]
    public void FactoryThrow_PropagatesExactIdentityAndCompletesInvocationOnce()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var expected = new InvalidOperationException("factory");
        var calls = 0;
        Mock.When(target.ReadOnly).ReturnFactory(() =>
        {
            calls++;
            throw expected;
        });

        Exception actual = Assert.Throws<InvalidOperationException>(
            () => target.ReadOnly());

        Assert.AreSame(expected, actual);
        Assert.AreEqual(1, calls);
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(MockInvocationCompletionKind.Threw, invocation.Completion.Kind);
        Assert.AreEqual(MockInvocationFailureStage.ReturnFactory, invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
    }

    /// <summary>Strict and loose interface fallbacks respectively throw or return the typed default without setup capture invoking a factory.</summary>
    [TestMethod]
    public void InterfaceFallback_StrictThrowsAndLooseReturnsTypedDefault()
    {
        var strict = Mock.Create<IRefReturnTarget>();
        var loose = Mock.CreateLoose<IRefReturnTarget>();
        var owner = new RefReturnOwner([3]);

        Mock.When(strict.ReadOnly).ReturnFactory(owner.ReadOnly);
        Assert.AreEqual(0, owner.FactoryCalls);
        Assert.IsTrue(strict.ReadOnly().SequenceEqual([3]));
        Assert.Throws<MockException>(() => strict.View());
        Assert.IsTrue(loose.ReadOnly().IsEmpty);

        Assert.AreEqual(
            2,
            Mock.GetMocked(strict)!.Invocations.Snapshot().Invocations.Length);
        Assert.AreEqual(
            1,
            Mock.GetMocked(loose)!.Invocations.Snapshot().Invocations.Length);
        MockInvocation[] strictInvocations = [..
            Mock.GetMocked(strict)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(
            MockInvocationReturnKind.Unavailable,
            strictInvocations[0].Completion.Return!.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.StrictFailure,
            strictInvocations[1].Completion.Source);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            strictInvocations[1].Completion.Kind);
        MockInvocation looseInvocation =
            Mock.GetMocked(loose)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(
            MockInvocationReturnKind.Unavailable,
            looseInvocation.Completion.Return!.Kind);
        Assert.AreEqual(
            MockInvocationExecutionSource.LooseFallback,
            looseInvocation.Completion.Source);
    }

    /// <summary>Virtual typed-return proxies enforce strict and loose fallback without running original bodies.</summary>
    [TestMethod]
    public void ClassFallbacks_StrictAndLooseRecordExactBorrowedOutcomes()
    {
        var strictVirtual = Mock.Create<VirtualRefReturnTarget>();
        var looseVirtual = Mock.CreateLoose<VirtualRefReturnTarget>();

        Assert.Throws<MockException>(() => strictVirtual.Read());
        Assert.IsTrue(looseVirtual.Read().IsEmpty);

        Assert.AreEqual(0, strictVirtual.Calls);
        Assert.AreEqual(0, looseVirtual.Calls);
        AssertStrictBorrowedFallback(strictVirtual);
        AssertLooseBorrowedFallback(looseVirtual);
    }

    /// <summary>Virtual class proxies execute an exact typed factory and leave the original virtual body untouched.</summary>
    [TestMethod]
    public void VirtualClassFactory_SkipsOriginalImplementation()
    {
        var target = Mock.Create<VirtualRefReturnTarget>();
        var owner = new RefReturnOwner([5, 8]);
        Mock.When(target.Read).ReturnFactory(owner.ReadOnly);

        ReadOnlySpan<int> result = target.Read();

        Assert.IsTrue(result.SequenceEqual([5, 8]));
        Assert.AreEqual(0, target.Calls);
        Assert.AreEqual(1, owner.FactoryCalls);
    }

    /// <summary>Closed generic proxy types preserve the exact ref-struct factory return identity.</summary>
    [TestMethod]
    public void ClosedGenericClassFactory_ReturnsExactBorrowedValue()
    {
        var target = Mock.Create<GenericRefReturnTarget<string>>();
        var owner = new RefReturnOwner([233, 377]);
        Mock.When(target.Read).ReturnFactory(owner.View);

        BorrowedView result = target.Read();

        Assert.AreEqual(610, result.Sum);
        Assert.AreEqual(1, owner.FactoryCalls);
    }

    /// <summary>Proxy-owned generic methods isolate exact constructed return factories by construction.</summary>
    [TestMethod]
    public void ProxyGenericMethodFactories_DoNotCrossContaminateConstructions()
    {
        var target = Mock.Create<IGenericRefReturnTarget>();
        var owner = new RefReturnOwner([1, 1, 2, 3, 5]);
        Mock.When(() => target.Read<ReadOnlySpan<int>>())
            .ReturnFactory(owner.ReadOnly);
        Mock.When(() => target.Read<BorrowedView>())
            .ReturnFactory(owner.View);

        ReadOnlySpan<int> span = target.Read<ReadOnlySpan<int>>();
        BorrowedView view = target.Read<BorrowedView>();

        Assert.IsTrue(span.SequenceEqual([1, 1, 2, 3, 5]));
        Assert.AreEqual(12, view.Sum);
        Assert.AreEqual(2, owner.FactoryCalls);
    }

    /// <summary>Argument patterns select the newest exact factory before a matcher and catch-all without cross-invocation state.</summary>
    [TestMethod]
    public void ArgumentFactories_ExactMatcherAndNewestSelectionStayDistinct()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var any = new RefReturnOwner([1]);
        var negative = new RefReturnOwner([2]);
        var olderExact = new RefReturnOwner([3]);
        var newestExact = new RefReturnOwner([4]);
        Mock.When(() => target.Select(Arg.Any<int>()))
            .ReturnFactory(any.ReadOnly);
        Mock.When(() => target.Select(
                Arg.Match<int>(value => value < 0)))
            .ReturnFactory(negative.ReadOnly);
        Mock.When(() => target.Select(7))
            .ReturnFactory(olderExact.ReadOnly);
        Mock.When(() => target.Select(7))
            .ReturnFactory(newestExact.ReadOnly);

        Assert.IsTrue(target.Select(7).SequenceEqual([4]));
        newestExact.Replace([40]);
        Assert.IsTrue(target.Select(7).SequenceEqual([40]));
        Assert.IsTrue(target.Select(-1).SequenceEqual([2]));
        Assert.IsTrue(target.Select(8).SequenceEqual([1]));

        Assert.AreEqual(1, any.FactoryCalls);
        Assert.AreEqual(1, negative.FactoryCalls);
        Assert.AreEqual(0, olderExact.FactoryCalls);
        Assert.AreEqual(2, newestExact.FactoryCalls);
        Assert.AreEqual(
            4,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>Unowned concrete generic interception rejects before a return factory can be retained.</summary>
    [TestMethod]
    public void UnownedConcreteGenericFactory_RejectsBeforePublication()
    {
        var target = Mock.Create<SealedGenericRefReturnTarget>();
        var owner = new RefReturnOwner([987]);

        MockException exception = Assert.Throws<MockException>(
            () => Mock.When(
                () => target.Read<ReadOnlySpan<int>>()));

        StringAssert.Contains(exception.Message, "owned interception call site");
        Assert.AreEqual(0, owner.FactoryCalls);
        Assert.AreEqual(
            0,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>A borrowed factory can reenter the same mock because user code executes outside setup and ledger locks.</summary>
    [TestMethod]
    public void Factory_ReentersSameMockWithoutDeadlockOrTokenLoss()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var owner = new RefReturnOwner([1597]);
        Mock.When(target.Value).Return(7);
        Mock.When(target.ReadOnly).ReturnFactory(() =>
        {
            Assert.AreEqual(7, target.Value());
            return owner.ReadOnly();
        });

        ReadOnlySpan<int> result = target.ReadOnly();

        Assert.IsTrue(result.SequenceEqual([1597]));
        Assert.AreEqual(1, owner.FactoryCalls);
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(2, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Kind ==
            MockInvocationCompletionKind.Returned));
    }

    /// <summary>Null and exact-return mismatches fail before a typed factory behavior can be published.</summary>
    [TestMethod]
    public void FactoryValidation_RejectsBeforePublication()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var owner = new RefReturnOwner([2584]);

        Assert.Throws<ArgumentNullException>(
            () => Mock.When(target.ReadOnly).ReturnFactory(null!));
        Assert.AreEqual(
            0,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);

        MethodInfo mutable = typeof(IRefReturnTarget).GetMethod(
            nameof(IRefReturnTarget.Mutable))!;
        Assert.Throws<MockException>(
            () => Mock.GetMocked(target)!.AddTypedReturnFactory(
                mutable,
                [],
                owner.ReadOnly));

        Assert.AreEqual(0, owner.FactoryCalls);
        Assert.Throws<MockException>(() => target.Mutable());
        Assert.Throws<MockException>(() => target.ReadOnly());
    }

    /// <summary>Dropping a mock also releases the factory delegate and its captured owner despite process-wide emitted-code caches.</summary>
    [TestMethod]
    public void ReleasedMock_DoesNotLeaveFactoryOrOwnerRootedInCaches()
    {
        (WeakReference owner, WeakReference mock) =
            CreateReleasedFactoryRoots();

        for (var attempt = 0;
             attempt < 8 && (owner.IsAlive || mock.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(mock.IsAlive, "The released proxy mock remained rooted.");
        Assert.IsFalse(owner.IsAlive, "The typed factory owner remained rooted.");
    }

    /// <summary>Closed-generic proxy ownership releases exact factory delegates and owners.</summary>
    [TestMethod]
    public void ReleasedClosedGenericMock_DoesNotRetainFactoryOwner()
    {
        (WeakReference genericOwner, WeakReference genericMock) =
            CreateReleasedGenericFactoryRoots();

        for (var attempt = 0;
             attempt < 8 && (
                 genericOwner.IsAlive || genericMock.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(genericMock.IsAlive);
        Assert.IsFalse(genericOwner.IsAlive);
    }

    /// <summary>Mutable owned returns isolate setup input and preserve mutations through later fresh span views.</summary>
    [TestMethod]
    public void ReturnOwned_MutableSpanCopiesOnceAndKeepsCurrentStorage()
    {
        var target = Mock.Create<IRefReturnTarget>();
        int[] source = [1, 2, 3];
        Mock.When(target.Mutable).ReturnOwned(source);

        Assert.AreEqual(
            0,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);
        source[0] = 99;
        Span<int> first = target.Mutable();
        first[1] = 55;
        Span<int> second = target.Mutable();

        CollectionAssert.AreEqual(
            new[] { 1, 55, 3 },
            second.ToArray());
        Assert.AreEqual(99, source[0]);
        Assert.AreEqual(2, source[1]);
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(2, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Source ==
            MockInvocationExecutionSource.Configured
            && invocation.Completion.Return!.Kind ==
            MockInvocationReturnKind.Unavailable));
    }

    /// <summary>Read-only owned returns reuse the copied setup storage and never observe later source mutation.</summary>
    [TestMethod]
    public void ReturnOwned_ReadOnlySpanReturnsSameCopiedStorage()
    {
        var target = Mock.Create<IRefReturnTarget>();
        int[] source = [5, 8, 13];
        Mock.When(target.ReadOnly).ReturnOwned(source);

        source[2] = 999;
        ReadOnlySpan<int> first = target.ReadOnly();
        ReadOnlySpan<int> second = target.ReadOnly();

        Assert.IsTrue(first.SequenceEqual([5, 8, 13]));
        Assert.IsTrue(second.SequenceEqual([5, 8, 13]));
        ref int firstReference =
            ref System.Runtime.InteropServices.MemoryMarshal.GetReference(first);
        ref int secondReference =
            ref System.Runtime.InteropServices.MemoryMarshal.GetReference(second);
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref firstReference,
                ref secondReference));
        Assert.AreEqual(
            2,
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations.Length);
    }

    /// <summary>Mock-owned span storage and its element graph become collectible with the configured mock.</summary>
    [TestMethod]
    public void ReturnOwned_ReleasesCopiedStorageWithMock()
    {
        (WeakReference marker, WeakReference mock) =
            CreateReleasedOwnedSpanRoots();

        for (var attempt = 0;
             attempt < 8 && (marker.IsAlive || mock.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(mock.IsAlive, "The released owned-span mock remained rooted.");
        Assert.IsFalse(marker.IsAlive, "The copied owned-span element remained rooted.");
    }

    /// <summary>Owned-span implementation types declare no direct static storage fields.</summary>
    [TestMethod]
    public void ReturnOwned_DeclaresNoDirectStaticStorageFields()
    {
        Assert.AreEqual(
            0,
            typeof(MockOwnedSpanReturn<>).GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic).Length);
        Assert.AreEqual(
            0,
            typeof(MockSetupClauseExtensions).GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic).Length);
    }

    /// <summary>Dispatch cache types declare no direct static delegate or object field.</summary>
    [TestMethod]
    public void DispatchCaches_DeclareNoDirectFactoryOrOwnerFields()
    {
        Type[] cacheTypes =
        [
            typeof(MockTypedTrampolineCache),
            typeof(MockGenericCallsite),
            typeof(ProxyTypeBuilder)
        ];

        foreach (Type cacheType in cacheTypes)
        {
            FieldInfo[] unsafeFields = [..
                cacheType.GetFields(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Where(field =>
                    typeof(Delegate).IsAssignableFrom(field.FieldType)
                    || field.FieldType == typeof(object))];
            Assert.AreEqual(
                0,
                unsafeFields.Length,
                $"{cacheType.Name} exposes a process-wide delegate/object root.");
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Owner, WeakReference Mock)
        CreateReleasedFactoryRoots()
    {
        var target = Mock.Create<IRefReturnTarget>();
        var owner = new RefReturnOwner([4181]);
        Mock.When(target.ReadOnly).ReturnFactory(owner.ReadOnly);
        Assert.AreEqual(1, target.ReadOnly().Length);
        return (new(owner), new(target));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Owner, WeakReference Mock)
        CreateReleasedGenericFactoryRoots()
    {
        var target = Mock.Create<GenericRefReturnTarget<string>>();
        var owner = new RefReturnOwner([17711]);
        Mock.When(target.Read).ReturnFactory(owner.View);
        Assert.AreEqual(17711, target.Read().Sum);
        return (new(owner), new(target));
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Marker, WeakReference Mock)
        CreateReleasedOwnedSpanRoots()
    {
        var target = Mock.Create<IOwnedSpanReturnTarget>();
        var marker = new OwnedSpanMarker();
        OwnedSpanMarker[] source = [marker];
        Mock.When(target.Mutable).ReturnOwned(source);
        source[0] = null!;
        Assert.AreEqual(1, target.Mutable().Length);
        return (new(marker), new(target));
    }

    private static void AssertStrictBorrowedFallback(object target)
    {
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(
            MockInvocationExecutionSource.StrictFailure,
            invocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationCompletionKind.Threw,
            invocation.Completion.Kind);
        Assert.IsNull(invocation.Completion.Return);
    }

    private static void AssertLooseBorrowedFallback(object target)
    {
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];
        Assert.AreEqual(
            MockInvocationExecutionSource.LooseFallback,
            invocation.Completion.Source);
        Assert.AreEqual(
            MockInvocationCompletionKind.Returned,
            invocation.Completion.Kind);
        Assert.AreEqual(
            MockInvocationReturnKind.Unavailable,
            invocation.Completion.Return!.Kind);
    }
}

internal readonly ref struct BorrowedView
{
    private readonly ReadOnlySpan<int> values;

    internal BorrowedView(ReadOnlySpan<int> values)
    {
        this.values = values;
    }

    internal int Sum
    {
        get
        {
            var sum = 0;
            foreach (int value in values)
                sum += value;
            return sum;
        }
    }
}

internal sealed class RefReturnOwner(int[] values)
{
    private int[] values = values;

    internal int FactoryCalls { get; private set; }

    internal void Replace(int[] replacement) => values = replacement;

    internal Span<int> Mutable()
    {
        FactoryCalls++;
        return values;
    }

    internal ReadOnlySpan<int> ReadOnly()
    {
        FactoryCalls++;
        return values;
    }

    internal BorrowedView View()
    {
        FactoryCalls++;
        return new(values);
    }
}

internal interface IRefReturnTarget
{
    Span<int> Mutable();
    ReadOnlySpan<int> ReadOnly();
    BorrowedView View();
    int Value();
    ReadOnlySpan<int> Select(int key);
}

internal class VirtualRefReturnTarget
{
    private static readonly int[] Values = [13, 21];

    internal int Calls;

    public virtual ReadOnlySpan<int> Read()
    {
        Calls++;
        return Values;
    }
}

internal sealed class SealedRefReturnTarget
{
    private static readonly int[] Values = [13, 21];

    internal int Calls;

    public ReadOnlySpan<int> Read()
    {
        Calls++;
        return Values;
    }
}

internal sealed class PartialRefReturnTarget
{
    private static readonly int[] ConfiguredValues = [1];
    private static readonly int[] NeighborValues = [2, 3];

    internal int ConfiguredCalls;
    internal int NeighborCalls;

    public ReadOnlySpan<int> Configured()
    {
        ConfiguredCalls++;
        return ConfiguredValues;
    }

    public ReadOnlySpan<int> Neighbor()
    {
        NeighborCalls++;
        return NeighborValues;
    }
}

internal class GenericRefReturnTarget<TMarker>
{
    public virtual BorrowedView Read() => default;
}

internal interface IGenericRefReturnTarget
{
    T Read<T>()
        where T : allows ref struct;
}

internal sealed class OwnedSpanMarker;

internal interface IOwnedSpanReturnTarget
{
    Span<OwnedSpanMarker> Mutable();
}

internal sealed class SealedGenericRefReturnTarget
{
    public T Read<T>()
        where T : allows ref struct =>
        default!;
}
