namespace AlvorKit.Mocking.Test;

/// <summary>Exercises the live-ref struct control plane through exact runtime bindings.</summary>
[TestClass]
public sealed class MockStructRuntimeTest
{
    private static int nextOffset;

    /// <summary>Type, value, and site scopes select copies without instance identity.</summary>
    [TestMethod]
    public void Bind_StructScopes_SelectAndVerifyLiveCopies()
    {
        (StructRuntimeCall first, MockCallSite firstSite) =
            BindAdd();
        (StructRuntimeCall second, MockCallSite secondSite) =
            BindAdd();
        StructRuntimeSites.Add = first;
        var matching = Mock.Struct<StructRuntimeCounter>()
            .Matching(
                (scoped in value) =>
                    value.Value == 5);

        using (Mock.Session())
        {
            Mock.Struct<StructRuntimeCounter>()
                .When<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            Arg.Any<int>()))
                .Return(100);
            matching
                .When<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            2))
                .Return(200);
            Mock.Struct<StructRuntimeCounter>()
                .AtSite(secondSite)
                .When<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            3))
                .Return(300);

            var five = new StructRuntimeCounter(5);
            var seven = new StructRuntimeCounter(7);
            Assert.AreEqual(200, first(ref five, 2));
            Assert.AreEqual(100, first(ref seven, 2));
            Assert.AreEqual(300, second(ref seven, 3));
            Assert.AreEqual(5, five.Value);
            Assert.AreEqual(7, seven.Value);

            matching.Verify<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            2))
                .Once();
            Mock.Struct<StructRuntimeCounter>()
                .AtSite(firstSite)
                .Verify<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            Arg.Any<int>()))
                .Exactly(2);
            Mock.Struct<StructRuntimeCounter>()
                .AtSite(secondSite)
                .Verify<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            3))
                .Once();
        }
    }

    /// <summary>Struct hooks run in the frozen phase order and readonly mutation rejects.</summary>
    [TestMethod]
    public void Bind_StructHooks_MutateLiveStorageAndRejectReadonly()
    {
        (StructRuntimeCall add, _) = BindAdd();
        StructRuntimeSites.Add = add;
        var entry = -1;
        var exit = -1;

        using (Mock.Session())
        {
            Mock.Struct<StructRuntimeCounter>()
                .When<int>(
                    static (scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            2))
                .SnapshotThisOnEntry(
                    (scoped in value) =>
                    {
                        entry = value.Value;
                        return value.Value;
                    })
                .MutateThisOnEntry(
                    (scoped ref value) =>
                        value.Value += 10)
                .MutateThisOnExit(
                    (scoped ref value) =>
                        value.Value += 100)
                .SnapshotThisOnExit(
                    (scoped in value) =>
                    {
                        exit = value.Value;
                        return value.Value;
                    })
                .Passthrough();

            var counter = new StructRuntimeCounter(3);
            Assert.AreEqual(15, add(ref counter, 2));
            Assert.AreEqual(115, counter.Value);
            Assert.AreEqual(3, entry);
            Assert.AreEqual(115, exit);

            MockSession current = MockSession.Current!;
            MockInvocation invocation =
                current.SnapshotThrough(current.Checkpoint()).Single();
            Assert.AreEqual(
                3,
                invocation.Arguments[0].Entry.Value);
            Assert.AreEqual(
                115,
                invocation.Arguments[0].Exit.Value);
            Mock.Struct<StructRuntimeCounter>()
                .Matching(
                    static (
                        scoped in value) =>
                        value.Value == 3)
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        StructRuntimeSites.InvokeAdd(
                            ref value,
                            2))
                .Once();

            (StructReadonlyRuntimeCall read, _) =
                BindRead();
            StructRuntimeSites.Read = read;
            MockException error =
                Assert.ThrowsExactly<MockException>(
                    () => Mock.Struct<StructRuntimeCounter>()
                        .When<int>(
                            static (
                                scoped ref value) =>
                                StructRuntimeSites.InvokeRead(
                                    ref value,
                                    1))
                        .MutateThisOnEntry(
                            static (
                                scoped ref value) =>
                                value.Value++)
                        .Passthrough());
            StringAssert.Contains(error.Message, "Readonly");
        }
    }

    /// <summary>A constrained interface operation binds to one closed live struct receiver.</summary>
    [TestMethod]
    public void Bind_ConstrainedInterfaceOperation_UsesClosedReceiver()
    {
        (StructRuntimeCall call, _) = BindInterfaceAdd();
        StructRuntimeSites.InterfaceAdd = call;

        using (Mock.Session())
        {
            Mock.Struct<StructRuntimeCounter>()
                .When<int>(
                    static (
                        scoped ref value) =>
                        StructRuntimeSites.InvokeInterfaceAdd(
                            ref value,
                            4))
                .Return(90);

            var counter = new StructRuntimeCounter(8);
            Assert.AreEqual(90, call(ref counter, 4));
            Assert.AreEqual(8, counter.Value);
            Mock.Struct<StructRuntimeCounter>()
                .Verify<int>(
                    static (
                        scoped ref value) =>
                        StructRuntimeSites.InvokeInterfaceAdd(
                            ref value,
                            4))
                .Once();
        }
    }

    private static (
        StructRuntimeCall Call,
        MockCallSite Site) BindAdd()
    {
        MethodInfo method = typeof(StructRuntimeCounter)
            .GetMethod(nameof(StructRuntimeCounter.Add))!;
        MockInterceptionSiteDescriptor descriptor = Site();
        return (
            MockInterceptionOperationRuntime.Bind(
                descriptor,
                method,
                new StructRuntimeCall(
                    static (
                        scoped ref value,
                        amount) =>
                        value.Add(amount))),
            new(descriptor, method));
    }

    private static (
        StructReadonlyRuntimeCall Call,
        MockCallSite Site) BindRead()
    {
        MethodInfo method = typeof(StructRuntimeCounter)
            .GetMethod(nameof(StructRuntimeCounter.Read))!;
        MockInterceptionSiteDescriptor descriptor = Site();
        return (
            MockInterceptionOperationRuntime.Bind(
                descriptor,
                method,
                new StructReadonlyRuntimeCall(
                    static (
                        scoped in value,
                        amount) =>
                        value.Read(amount))),
            new(descriptor, method));
    }

    private static (
        StructRuntimeCall Call,
        MockCallSite Site) BindInterfaceAdd()
    {
        MethodInfo method = typeof(IStructRuntimeCounter)
            .GetMethod(nameof(IStructRuntimeCounter.Add))!;
        MockInterceptionSiteDescriptor descriptor = Site();
        return (
            MockInterceptionOperationRuntime.Bind(
                descriptor,
                method,
                new StructRuntimeCall(
                    static (
                        scoped ref value,
                        amount) =>
                        value.Add(amount))),
            new(descriptor, method));
    }

    private static MockInterceptionSiteDescriptor Site() =>
        new(
            typeof(MockStructRuntimeTest).Module.ModuleVersionId,
            typeof(MockStructRuntimeTest).MetadataToken,
            Interlocked.Increment(ref nextOffset),
            MockInvocationOperationKind.StructMethod);
}

internal delegate int StructRuntimeCall(
    scoped ref StructRuntimeCounter value,
    int amount);

internal delegate int StructReadonlyRuntimeCall(
    scoped in StructRuntimeCounter value,
    int amount);

internal interface IStructRuntimeCounter
{
    int Add(int amount);
}

internal struct StructRuntimeCounter(int value) :
    IStructRuntimeCounter
{
    internal int Value = value;

    public int Add(int amount)
    {
        Value += amount;
        return Value;
    }

    public readonly int Read(int amount) =>
        Value + amount;
}

internal static class StructRuntimeSites
{
    internal static StructRuntimeCall Add = null!;
    internal static StructRuntimeCall InterfaceAdd = null!;
    internal static StructReadonlyRuntimeCall Read = null!;

    internal static int InvokeAdd(
        scoped ref StructRuntimeCounter value,
        int amount) =>
        Add(ref value, amount);

    internal static int InvokeRead(
        scoped ref StructRuntimeCounter value,
        int amount) =>
        Read(in value, amount);

    internal static int InvokeInterfaceAdd(
        scoped ref StructRuntimeCounter value,
        int amount) =>
        InterfaceAdd(ref value, amount);
}
