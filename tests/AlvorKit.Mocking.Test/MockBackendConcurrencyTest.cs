namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockBackendConcurrencyTest
{
    private static readonly TimeSpan CoordinationTimeout =
        TimeSpan.FromMilliseconds(750);
    private static readonly MockBackendIdentity ProxyBackend =
        new(MockBackendKind.Proxy, 2);

    /// <summary>Cold concurrent callers publish one proxy type and one exact typed trampoline artifact.</summary>
    [TestMethod]
    public void FirstUse_ConcurrentCallersPublishOneProxyAndTrampolineArtifact()
    {
        const int callerCount = 8;
        (Type sourceType, MethodInfo method) =
            CreateCollectibleInterface(variableArguments: false);
        var proxies = new Type[callerCount];
        var artifacts = new MockTypedTrampolineArtifact[callerCount];
        var typeCaches = new TypeCache[callerCount];
        var callers = new Task[callerCount];
        using var start = new Barrier(callerCount + 1);

        for (var index = 0; index < callerCount; index++)
        {
            var capture = index;
            callers[capture] = LongRunningTask(() =>
            {
                SignalAndWait(
                    start,
                    $"Caller {capture} did not reach the cold first-use gate.");
                proxies[capture] = Proxies.Get(sourceType);
                typeCaches[capture] = Types.Get(sourceType);
                artifacts[capture] =
                    MockTypedTrampolineCache.GetOrCreate(
                        method,
                        ProxyBackend);
            });
        }

        SignalAndWait(start, "Cold first-use callers did not become ready.");
        Assert.IsTrue(
            Task.WaitAll(callers, CoordinationTimeout),
            "Cold first-use callers did not finish within the test bound.");

        MockCanonicalSignature expectedSignature =
            MockCanonicalSignature.Create(method);
        for (var index = 1; index < callerCount; index++)
        {
            Assert.AreSame(proxies[0], proxies[index]);
            Assert.AreSame(artifacts[0], artifacts[index]);
            Assert.AreSame(typeCaches[0], typeCaches[index]);
        }

        Assert.IsTrue(sourceType.IsAssignableFrom(proxies[0]));
        Assert.AreSame(sourceType, typeCaches[0].Type);
        Assert.AreEqual(expectedSignature, artifacts[0].Key.Signature);
        Assert.AreEqual(MockBackendKind.Proxy, artifacts[0].Key.Backend.Kind);
        Assert.AreEqual(2, artifacts[0].Key.Backend.AbiVersion);
    }

    /// <summary>Interface-only proxy, reflection, and trampoline caches release their source module and generated artifacts.</summary>
    [TestMethod]
    public void CollectibleInterface_ProxyAndTypeCachesReleaseSourceModule()
    {
        (string Name, WeakReference Reference)[] references =
            CreateCollectibleCacheReferences();

        for (var attempt = 0;
             attempt < 8 &&
             references.Any(static item => item.Reference.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        string[] retained =
        [
            .. references
                .Where(static item => item.Reference.IsAlive)
                .Select(static item => item.Name)
        ];
        Assert.HasCount(
            0,
            retained,
            $"Collectible backend cache retained: {string.Join(", ", retained)}.");
    }

    /// <summary>Weak mock ownership releases a configured projector owner and its target.</summary>
    [TestMethod]
    public void ConfiguredProjector_WeakOwnershipReleasesTargetAndOwner()
    {
        (WeakReference target, WeakReference owner) =
            ConfigureTransientProjector();

        for (var attempt = 0;
             attempt < 8 && (target.IsAlive || owner.IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(target.IsAlive, "The projector target remained rooted.");
        Assert.IsFalse(owner.IsAlive, "The projector owner remained rooted.");
    }

    /// <summary>Pre-installation rejection identifies the backend and normalized canonical signature exactly.</summary>
    [TestMethod]
    public void ValidationFailure_IdentifiesBackendAndNormalizedSignature()
    {
        (Type sourceType, MethodInfo method) =
            CreateCollectibleInterface(variableArguments: true);
        MockSignatureValidation validation =
            MockSignatureValidator.Validate(
                method,
                ProxyBackend,
                MockOperationKind.InstanceMethod);
        MockSignatureRejection rejection = validation.Rejection!;
        var normalizedSignature = new StringBuilder();
        MockDiagnosticSignatureFormatter.AppendCanonicalSignature(
            normalizedSignature,
            validation.Signature);

        MockException exception = Assert.Throws<MockException>(
            () => Proxies.Get(sourceType));

        Assert.AreEqual(ProxyBackend, rejection.Backend);
        Assert.AreEqual(
            MockUnsupportedSignatureReason.VariableArguments,
            rejection.Reason);
        Assert.AreSame(validation.Signature, rejection.Signature);
        Assert.AreEqual(rejection.Message, exception.Message);
        StringAssert.Contains(exception.Message, ProxyBackend.ToString());
        StringAssert.Contains(
            exception.Message,
            $"signature '{normalizedSignature}'");
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (string Name, WeakReference Reference)[]
        CreateCollectibleCacheReferences()
    {
        (Type sourceType, MethodInfo method) =
            CreateCollectibleInterface(variableArguments: false);
        Type proxyType = Proxies.Get(sourceType);
        TypeCache typeCache = Types.Get(sourceType);
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(
                method,
                ProxyBackend);
        Type trampolineType = artifact.Prefix.DeclaringType!;
        Type callbackType =
            MockTypedCallbackDelegateCache.GetOrCreate(method);

        return
        [
            ("source assembly", new(sourceType.Assembly)),
            ("source module", new(sourceType.Module)),
            ("source type", new(sourceType)),
            ("proxy assembly", new(proxyType.Assembly)),
            ("proxy module", new(proxyType.Module)),
            ("proxy type", new(proxyType)),
            ("type cache", new(typeCache)),
            ("trampoline artifact", new(artifact)),
            ("trampoline assembly", new(trampolineType.Assembly)),
            ("trampoline module", new(trampolineType.Module)),
            ("trampoline type", new(trampolineType)),
            ("callback assembly", new(callbackType.Assembly)),
            ("callback module", new(callbackType.Module)),
            ("callback type", new(callbackType)),
        ];
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Target, WeakReference Owner)
        ConfigureTransientProjector()
    {
        var target = Mock.Create<IBackendReachabilityTarget>();
        var owner = new BackendProjectorOwner();
        Mock.When(
                () => target.Invoke(
                    Arg.Any<Span<int>>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in Span<int> values) =>
                    owner.Project(in values))
            .Return(2);

        Assert.AreEqual(2, target.Invoke([13, 21]));
        return (new(target), new(owner));
    }

    private static (Type Type, MethodInfo Method)
        CreateCollectibleInterface(bool variableArguments)
    {
        string name =
            $"AlvorKit.Mocking.BackendConcurrency.{Guid.NewGuid():N}";
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(name),
            AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module = assembly.DefineDynamicModule(name);
        TypeBuilder type = module.DefineType(
            $"{name}.ITarget",
            TypeAttributes.Public
            | TypeAttributes.Interface
            | TypeAttributes.Abstract);
        MethodBuilder method = type.DefineMethod(
            "Invoke",
            MethodAttributes.Public
            | MethodAttributes.Abstract
            | MethodAttributes.Virtual
            | MethodAttributes.NewSlot,
            variableArguments
                ? CallingConventions.HasThis | CallingConventions.VarArgs
                : CallingConventions.HasThis,
            typeof(int),
            [typeof(int)]);
        Type sourceType = type.CreateType()!;
        return (sourceType, sourceType.GetMethod(method.Name)!);
    }

    private static Task LongRunningTask(Action action) =>
        Task.Factory.StartNew(
            action,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

    private static void SignalAndWait(Barrier barrier, string failureMessage)
    {
        if (!barrier.SignalAndWait(CoordinationTimeout))
            throw new TimeoutException(failureMessage);
    }
}

internal interface IBackendReachabilityTarget
{
    int Invoke(Span<int> values);
}

internal sealed class BackendProjectorOwner
{
    internal int[] Project(scoped in Span<int> values) =>
        values.ToArray();
}
