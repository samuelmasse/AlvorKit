namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockGenericInstrumentationTest
{
    private static readonly TimeSpan CoordinationTimeout =
        TimeSpan.FromMilliseconds(750);

    /// <summary>
    /// Proves the manual public generic-preparation API has been removed.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_PublicPreparationApiIsAbsent()
    {
        Assert.IsFalse(
            typeof(Mock)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(static method => method.Name == "Generic"));
    }

    /// <summary>
    /// Proves value and shared-reference constructions retain independent setups.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_MultipleConstructionsDoNotCrossContaminate()
    {
        var mock = Mock.CreateLoose<GenericDispatchTarget>();
        var identifier = Guid.NewGuid();
        var uri = new Uri("https://example.test/");

        Mock.When(() => mock.Describe(11)).Return("integer");
        Mock.When(() => mock.Describe("eleven")).Return("string");
        Mock.When(() => mock.Describe(identifier)).Return("identifier");
        Mock.When(() => mock.Describe(uri)).Return("uri");

        Assert.AreEqual("integer", mock.Describe(11));
        Assert.AreEqual("string", mock.Describe("eleven"));
        Assert.AreEqual("identifier", mock.Describe(identifier));
        Assert.AreEqual("uri", mock.Describe(uri));
        Assert.AreEqual(string.Empty, mock.Describe(12));
        Assert.AreEqual(string.Empty, mock.Describe("twelve"));
    }

    /// <summary>
    /// Proves every proxy-owned interface, abstract, and virtual generic method dispatches automatically.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_AllProxyOwnedShapesWork()
    {
        AssertProxyOwned(Mock.Create<IMockTarget>());
        AssertProxyOwned(Mock.Create<AbstractMock>());
        AssertProxyOwned(Mock.Create<PartialMock>());
        AssertProxyOwned(Mock.Create<VirtualMock>());
        AssertProxyOwned(Mock.Create<DerivedMock>());
    }

    /// <summary>
    /// Proves generic value returns and managed-reference writeback remain exact.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_GenericReturnAndWritebackRemainExact()
    {
        var mock = Mock.Create<GenericDispatchTarget>();
        Mock.When(() => mock.Echo(7)).Return(11);
        Mock.When(() => mock.Echo("seven")).Return("eleven");

        int setupInteger = 3;
        Mock.When(
                () => mock.Exchange(
                    ref setupInteger,
                    out _))
            .Do(call =>
            {
                call.SetReference(0, 13);
                call.SetReference(1, 17);
            });
        string setupText = "before";
        Mock.When(
                () => mock.Exchange(
                    ref setupText,
                    out _))
            .Do(call =>
            {
                call.SetReference(0, "after");
                call.SetReference(1, "output");
            });

        Assert.AreEqual(11, mock.Echo(7));
        Assert.AreEqual("eleven", mock.Echo("seven"));

        int integer = 3;
        mock.Exchange(ref integer, out int integerOutput);
        Assert.AreEqual(13, integer);
        Assert.AreEqual(17, integerOutput);

        string text = "before";
        mock.Exchange(ref text, out string textOutput);
        Assert.AreEqual("after", text);
        Assert.AreEqual("output", textOutput);
    }

    /// <summary>
    /// Proves unowned concrete generic methods require interception call-site ownership.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_UnownedConcreteMethodsRejectDeterministically()
    {
        var sealedMock = Mock.Create<SealedGenericDispatchTarget>();
        var partial = Mock.Partial(new GenericDispatchTarget());
        var nonVirtual = Mock.Create<BasicMock>();

        AssertUnownedGenericRejection(
            () => Mock.When(() => sealedMock.Describe(13))
                .Return("sealed"));
        AssertUnownedGenericRejection(
            () => Mock.When(() => partial.Describe("configured"))
                .Return("partial"));
        AssertUnownedGenericRejection(
            () => Mock.When(
                    () => nonVirtual.ComputeSumOpen(1, 2))
                .Return(3));

        Assert.AreEqual(
            "original:neighbor",
            new GenericDispatchTarget().Describe("neighbor"));
    }

    /// <summary>
    /// Proves verification can cause the first specialization without setup.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_VerificationCanBeFirstUse()
    {
        var mock = Mock.CreateLoose<VerificationFirstGenericTarget>();

        Mock.Verify(() => mock.Describe(17)).Never();

        Assert.AreEqual(string.Empty, mock.Describe(17));
        Mock.Verify(() => mock.Describe(17)).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    /// <summary>
    /// Proves synchronized cold first use publishes one safe specialization shared across mocks.
    /// </summary>
    [TestMethod]
    public void AutomaticSpecialization_ConcurrentFirstUsePublishesOneArtifact()
    {
        const int callerCount = 16;
        var mocks = new ConcurrentFirstGenericTarget[callerCount];
        var tasks = new Task[callerCount];
        using var ready = new CountdownEvent(callerCount);
        using var start = new ManualResetEventSlim();

        for (int index = 0; index < callerCount; index++)
        {
            mocks[index] = Mock.Create<ConcurrentFirstGenericTarget>();
            int capture = index;
            tasks[index] = Task.Factory.StartNew(
                () =>
                {
                    ready.Signal();
                    if (!start.Wait(CoordinationTimeout))
                    {
                        throw new TimeoutException(
                            "Concurrent generic callers were not released.");
                    }

                    Mock.When(() => mocks[capture].Describe(capture))
                        .Return($"configured:{capture}");
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        var readyInTime = ready.Wait(CoordinationTimeout);
        if (!readyInTime)
            start.Set();
        Assert.IsTrue(
            readyInTime,
            "Concurrent generic callers did not reach their deterministic gate.");
        start.Set();
        Assert.IsTrue(
            Task.WaitAll(tasks, CoordinationTimeout),
            "Concurrent generic callers did not finish within the test bound.");

        for (int index = 0; index < callerCount; index++)
        {
            Assert.AreEqual(
                $"configured:{index}",
                mocks[index].Describe(index));
        }

        MethodInfo definition = mocks[0].GetType().GetMethod(
            nameof(ConcurrentFirstGenericTarget.Describe))!;
        MethodInfo construction = definition.MakeGenericMethod(
            typeof(int));
        Type cache = FindConstructedCache(
            definition.DeclaringType!.Assembly,
            construction);
        FieldInfo methodField = cache.GetField(
            "Method",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.AreSame(
            methodField.GetValue(null),
            methodField.GetValue(null));
    }

    private static void AssertProxyOwned<T>(T mock)
        where T : class, IMockTarget
    {
        static void Noop()
        {
        }

        Action action = Noop;
        Mock.When(() => mock.ComputeSumOpen(2, 3))
            .Return(101);
        Mock.When(
                () => mock.ComputeSumOpen<Action?, Action?>(
                    null,
                    action))
            .Return(103);

        Assert.AreEqual(101, mock.ComputeSumOpen(2, 3));
        Assert.AreEqual(
            103,
            mock.ComputeSumOpen<Action?, Action?>(
                null,
                action));
    }

    private static void AssertUnownedGenericRejection(Action setup)
    {
        MockException error = Assert.Throws<MockException>(setup);
        StringAssert.Contains(error.Message, "owned interception call site");
    }

    private static Type FindConstructedCache(
        Assembly assembly,
        MethodInfo construction)
    {
        foreach (Type definition in assembly.GetTypes())
        {
            if (!definition.Name.StartsWith(
                    "ProxyGenericCache_",
                    StringComparison.Ordinal)
                || !definition.IsGenericTypeDefinition
                || definition.GetGenericArguments().Length != 1)
            {
                continue;
            }

            Type cache;
            try
            {
                cache = definition.MakeGenericType(typeof(int));
            }
            catch (ArgumentException)
            {
                continue;
            }
            FieldInfo? field = cache.GetField(
                "Method",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field?.GetValue(null) is MethodInfo method
                && method == construction)
            {
                return cache;
            }
        }

        Assert.Fail("No constructed proxy generic cache was found.");
        return null!;
    }

    public class GenericDispatchTarget
    {
        public virtual string Describe<T>(T value) =>
            $"original:{value}";

        public virtual T Echo<T>(T value) => value;

        public virtual void Exchange<T>(
            ref T value,
            out T output) =>
            output = value;
    }

    public sealed class SealedGenericDispatchTarget
    {
        public string Describe<T>(T value) =>
            $"original:{value}";
    }

    public class VerificationFirstGenericTarget
    {
        public virtual string Describe<T>(T value) =>
            $"original:{value}";
    }

    public class ConcurrentFirstGenericTarget
    {
        public virtual string Describe<T>(T value) =>
            $"original:{value}";
    }
}
