namespace AlvorKit;

/// <summary>Runs isolated fixed-batch mocking measurements without timing assertions.</summary>
internal sealed class MockPerformanceFixture(MockPerformanceOptions options)
{
    private static long sink;
    private static int nextSiteOffset;

    /// <summary>Lists interpretation boundaries for measurements that cannot isolate one feature.</summary>
    internal static string[] MeasurementBoundaries { get; } =
    [
        "The same-type loose-mocked and unmocked-interception rows expose a combined dispatch-plus-history delta; " +
            "the public API has no history-disable mode that could isolate history alone.",
        "Verify and ClearInvocations expose public snapshot-related costs; raw snapshot-copy cost remains unavailable " +
            "because invocation snapshots are not publicly readable.",
        "The configured projector row includes typed dispatch, projector invocation, a four-int array copy, and retained " +
            "projected history; it does not isolate projector or copy cost.",
        "The configured callback row includes typed dispatch, direct callback invocation, and retained unavailable span " +
            "history; it does not isolate callback or history cost.",
        "The zero-argument row includes configured proxy dispatch and retained empty-argument history; it does not isolate " +
            "behavior selection, completion, or ledger cost.",
        "The partial ref/out row includes interception dispatch, original scalar work, caller-visible writeback, shallow entry " +
            "and exit snapshots, and retained history; it does not isolate parameter-carrier or cache cost."
    ];

    /// <summary>Runs cold cases first, then explicitly warmed single-thread and contention cases.</summary>
    internal MockPerformanceResult[] MeasureAll()
    {
        PrimeColdHarness();

        return
        [
            MeasureColdProxyGeneration(),
            MeasureColdInterceptionWrapperGeneration(),
            Measure(
                "configured-zero-argument-dispatch",
                "call",
                options.DispatchOperations,
                CreateConfiguredZeroArgumentDispatch,
                "Configured zero-argument interface dispatch with retained empty-argument invocation history; includes " +
                    "mock lookup, behavior selection, return completion, and ledger storage."),
            Measure(
                "warm-ordinary-boxed-dispatch",
                "call",
                options.DispatchOperations,
                CreateWarmOrdinaryDispatch,
                "Configured interface dispatch with retained invocation history."),
            Measure(
                "warm-typed-ref-struct-dispatch",
                "call",
                options.DispatchOperations,
                CreateWarmTypedDispatch,
                "Loose exact interception dispatch with a live Span<int>; span history is unavailable metadata."),
            Measure(
                "configured-span-entry-projector-dispatch",
                "call",
                options.DispatchOperations,
                CreateConfiguredProjectorDispatch,
                "Configured exact interception dispatch copies a four-int Span<int> into retained entry history; includes dispatch, " +
                    "projector invocation, array allocation/copy, and history storage."),
            Measure(
                "configured-span-typed-callback-dispatch",
                "call",
                options.DispatchOperations,
                CreateConfiguredTypedCallbackDispatch,
                "Configured exact interception dispatch calculates the return through a public live Span<int> callback and retains " +
                    "unavailable span history; includes dispatch, callback invocation, return, and history storage."),
            Measure(
                "direct-original-call",
                "call",
                options.OriginalOperations,
                CreateDirectOriginal,
                "Direct concrete call with no mocking instrumentation or history."),
            Measure(
                "unmocked-interception-wrapper-call",
                "call",
                options.OriginalOperations,
                CreateUnmockedInterception,
                "Exact interception runtime wrapper with an unmocked receiver; executes the preserved original and records no history."),
            Measure(
                "loose-mocked-interception-dispatch-history",
                "call",
                options.DispatchOperations,
                CreateLooseMockedInterception,
                "Same exact interception wrapper as the unmocked control; loose dispatch and retained history are both active, " +
                    "so their costs are not isolated from one another."),
            Measure(
                "partial-ref-out-passthrough",
                "call",
                options.DispatchOperations,
                CreatePartialRefOutPassthrough,
                "Partial exact interception dispatch executes original scalar ref/out work and retains shallow entry and exit " +
                    "history; includes lookup, argument transport, writeback, completion, and ledger storage."),
            Measure(
                "verification-copied-snapshot-256",
                "snapshot",
                1,
                CreateVerificationSnapshot,
                "Public Verify copies and scans 256 retained calls; includes capture and verified marking."),
            Measure(
                "clear-retired-snapshot-256",
                "clear",
                1,
                CreateClearSnapshot,
                "Public ClearInvocations retires and snapshots an epoch containing 256 calls."),
            Measure(
                "return-sequence-single-thread",
                "call",
                options.DispatchOperations,
                CreateSingleThreadSequence,
                "Unique configured values keep the atomic sequence counter unsaturated."),
            MeasureSequenceContention(),
            Measure(
                "session-number-single-thread",
                "call",
                options.DispatchOperations,
                CreateSingleThreadSession,
                "One ambient session assigns a shared timeline number to every call."),
            MeasureSessionContention(),
            Measure(
                "shared-code-one-mock",
                "call",
                options.DispatchOperations,
                CreateOneSharedCodeMock,
                "One sealed mock uses the already-cached exact interception wrapper and typed dispatch path."),
            Measure(
                "shared-code-thirty-two-mocks",
                "call",
                options.DispatchOperations,
                CreateManySharedCodeMocks,
                "Thirty-two sealed mocks share one cached exact interception wrapper while retaining separate state.")
        ];
    }

    private MockPerformanceResult MeasureColdProxyGeneration()
    {
        Func<MockPerformanceSample>[] samples =
        [
            MeasureColdProxySample<int>,
            MeasureColdProxySample<long>,
            MeasureColdProxySample<string>,
            MeasureColdProxySample<Guid>,
            MeasureColdProxySample<DateTime>,
            MeasureColdProxySample<Uri>,
            MeasureColdProxySample<Exception>,
            MeasureColdProxySample<object>,
            MeasureColdProxySample<TimeSpan>
        ];

        return Summarize(
            "cold-interface-proxy-generation",
            "create+call",
            1,
            RunCold(samples),
            "Each sample uses a new closed interface after common harness JIT priming.");
    }

    private MockPerformanceResult MeasureColdInterceptionWrapperGeneration()
    {
        Func<MockPerformanceSample>[] samples =
        [
            MeasureColdTypedSample<int>,
            MeasureColdTypedSample<long>,
            MeasureColdTypedSample<string>,
            MeasureColdTypedSample<Guid>,
            MeasureColdTypedSample<DateTime>,
            MeasureColdTypedSample<Uri>,
            MeasureColdTypedSample<Exception>,
            MeasureColdTypedSample<object>,
            MeasureColdTypedSample<TimeSpan>
        ];

        return Summarize(
            "cold-ref-struct-interception-wrapper-generation",
            "create+call",
            1,
            RunCold(samples),
            "Each sample binds a new closed exact interception span wrapper and typed dispatch path, creates a loose sealed mock, " +
                "and executes its first call after common harness JIT priming.");
    }

    private MockPerformanceResult Measure(
        string name,
        string unit,
        int operations,
        Func<MockPerformanceOperation> createOperation,
        string notes)
    {
        var warmupOperations = Math.Max(1, operations / 10);
        for (var warmup = 0; warmup < options.Warmups; warmup++)
        {
            using var operation = createOperation();
            operation.Run(warmupOperations);
        }

        var samples = new MockPerformanceSample[options.Runs];
        for (var run = 0; run < samples.Length; run++)
        {
            using var operation = createOperation();
            Collect();
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var started = Stopwatch.GetTimestamp();

            operation.Run(operations);

            var elapsed = Stopwatch.GetTimestamp() - started;
            var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            samples[run] = new(elapsed, allocated);
        }

        return Summarize(name, unit, operations, samples, notes);
    }

    private MockPerformanceResult MeasureSequenceContention() =>
        MeasureContention(
            "return-sequence-eight-worker-contention",
            operations =>
            {
                var mock = Mock.Create<IOrdinaryDispatchTarget>();
                var values = new int[operations];
                for (var index = 0; index < values.Length; index++)
                    values[index] = index;
                Mock.When(() => mock.Invoke(Arg.Any<int>()))
                    .ReturnSequence(values);
                return workerIndex =>
                {
                    var sum = 0L;
                    for (var index = workerIndex;
                         index < operations;
                         index += options.Workers)
                    {
                        sum += mock.Invoke(index);
                    }

                    Interlocked.Add(ref sink, sum);
                };
            },
            "Eight workers contend on one unsaturated atomic return-sequence cursor.");

    private MockPerformanceResult MeasureSessionContention() =>
        MeasureContention(
            "session-number-eight-worker-contention",
            operations =>
            {
                var mock = Mock.CreateLoose<IOrdinaryDispatchTarget>();
                return workerIndex =>
                {
                    var sum = 0L;
                    for (var index = workerIndex;
                         index < operations;
                         index += options.Workers)
                    {
                        sum += mock.Invoke(index);
                    }

                    Interlocked.Add(ref sink, sum);
                };
            },
            "Eight workers share one ambient session timeline; cross-thread allocation is not reported.",
            useSession: true);

    private MockPerformanceResult MeasureContention(
        string name,
        Func<int, Action<int>> createWorker,
        string notes,
        bool useSession = false)
    {
        for (var warmup = 0; warmup < options.Warmups; warmup++)
        {
            _ = RunContention(
                Math.Max(options.Workers, options.ContentionOperations / 10),
                createWorker,
                useSession);
        }

        var samples = new MockPerformanceSample[options.Runs];
        for (var run = 0; run < samples.Length; run++)
        {
            Collect();
            samples[run] = RunContention(
                options.ContentionOperations,
                createWorker,
                useSession);
        }

        return Summarize(
            name,
            "call",
            options.ContentionOperations,
            samples,
            notes);
    }

    private MockPerformanceSample RunContention(
        int operations,
        Func<int, Action<int>> createWorker,
        bool useSession)
    {
        using var session = useSession ? Mock.Session() : null;
        var worker = createWorker(operations);
        using var ready = new CountdownEvent(options.Workers);
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = new Task[options.Workers];
        for (var workerIndex = 0; workerIndex < tasks.Length; workerIndex++)
        {
            var index = workerIndex;
            tasks[index] = Task.Run(() =>
            {
                ready.Signal();
                start.Task.GetAwaiter().GetResult();
                worker(index);
            });
        }

        ready.Wait();
        var started = Stopwatch.GetTimestamp();
        start.SetResult();
        Task.WaitAll(tasks);
        var elapsed = Stopwatch.GetTimestamp() - started;
        return new(elapsed, null);
    }

    private static MockPerformanceOperation CreateWarmOrdinaryDispatch()
    {
        var mock = Mock.Create<IOrdinaryDispatchTarget>();
        Mock.When(() => mock.Invoke(Arg.Any<int>())).Return(1);
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += mock.Invoke(index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation
        CreateConfiguredZeroArgumentDispatch()
    {
        var mock = Mock.Create<IZeroArgumentDispatchTarget>();
        Mock.When(mock.Invoke).Return(1);
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += mock.Invoke();
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateWarmTypedDispatch()
    {
        var call = Bind(
            InvokeMethod<TypedDispatchTarget>(),
            new InterceptionTypedDispatchCall(InvokeTypedOriginal));
        var mock = Mock.CreateLoose<TypedDispatchTarget>();
        var storage = new int[4];
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(mock, index, storage);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateConfiguredProjectorDispatch()
    {
        var call = Bind(
            InvokeMethod<ConfiguredTypedDispatchTarget>(),
            new InterceptionSpanDispatchCall(InvokeSpanOriginal));
        var mock = Mock.Create<ConfiguredTypedDispatchTarget>();
        Mock.When(
                () => call(
                    mock,
                    Arg.Any<Span<int>>(0)))
            .SnapshotArgument(
                0,
                (
                    scoped in Span<int> values) =>
                    values.ToArray())
            .Return(4);
        var storage = new int[4];
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(mock, storage);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateConfiguredTypedCallbackDispatch()
    {
        var call = Bind(
            InvokeMethod<ConfiguredTypedDispatchTarget>(),
            new InterceptionSpanDispatchCall(InvokeSpanOriginal));
        var mock = Mock.Create<ConfiguredTypedDispatchTarget>();
        Mock.When(
                () => call(
                    mock,
                    Arg.Any<Span<int>>(0)))
            .Answer((Span<int> values) => values.Length);
        var storage = new int[4];
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(mock, storage);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateDirectOriginal()
    {
        var target = new DirectDispatchTarget();
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += target.Invoke(index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateUnmockedInterception()
    {
        var call = Bind(
            InvokeMethod<InterceptionDispatchTarget>(),
            new InterceptionDispatchCall(InvokeInterceptionOriginal));
        var target = new InterceptionDispatchTarget();
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(target, index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateLooseMockedInterception()
    {
        var call = Bind(
            InvokeMethod<InterceptionDispatchTarget>(),
            new InterceptionDispatchCall(InvokeInterceptionOriginal));
        var mock = Mock.CreateLoose<InterceptionDispatchTarget>();
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(mock, index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreatePartialRefOutPassthrough()
    {
        var call = Bind(
            InvokeMethod<PartialRefOutDispatchTarget>(),
            new InterceptionRefOutDispatchCall(InvokeRefOutOriginal));
        var target = Mock.Partial(new PartialRefOutDispatchTarget());
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
            {
                var value = index;
                sum += call(
                    target,
                    ref value,
                    out var doubled);
                sum += value + doubled;
            }

            Volatile.Write(ref sink, sum);
        });
    }

    private MockPerformanceOperation CreateVerificationSnapshot()
    {
        var mock = Mock.CreateLoose<IOrdinaryDispatchTarget>();
        for (var index = 0; index < options.SnapshotHistory; index++)
            mock.Invoke(index);

        return new(_ =>
            Mock.Verify(() => mock.Invoke(Arg.Any<int>()))
                .AtLeast(options.SnapshotHistory));
    }

    private MockPerformanceOperation CreateClearSnapshot()
    {
        var mock = Mock.CreateLoose<IOrdinaryDispatchTarget>();
        for (var index = 0; index < options.SnapshotHistory; index++)
            mock.Invoke(index);

        return new(_ => Mock.ClearInvocations(mock));
    }

    private static MockPerformanceOperation CreateSingleThreadSequence()
    {
        const int maximumOperations = 20_000;
        var mock = Mock.Create<IOrdinaryDispatchTarget>();
        var values = new int[maximumOperations];
        for (var index = 0; index < values.Length; index++)
            values[index] = index;
        Mock.When(() => mock.Invoke(Arg.Any<int>())).ReturnSequence(values);

        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += mock.Invoke(index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateSingleThreadSession()
    {
        var mock = Mock.CreateLoose<IOrdinaryDispatchTarget>();
        var session = Mock.Session();
        return new(
            operations =>
            {
                var sum = 0L;
                for (var index = 0; index < operations; index++)
                    sum += mock.Invoke(index);
                Volatile.Write(ref sink, sum);
            },
            session.Dispose);
    }

    private static MockPerformanceOperation CreateOneSharedCodeMock()
    {
        var call = Bind(
            InvokeMethod<InterceptionDispatchTarget>(),
            new InterceptionDispatchCall(InvokeInterceptionOriginal));
        var mock = Mock.Create<InterceptionDispatchTarget>();
        Mock.When(() => call(mock, Arg.Any<int>())).Return(1);
        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(mock, index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceOperation CreateManySharedCodeMocks()
    {
        const int mockCount = 32;
        var call = Bind(
            InvokeMethod<InterceptionDispatchTarget>(),
            new InterceptionDispatchCall(InvokeInterceptionOriginal));
        var mocks = new InterceptionDispatchTarget[mockCount];
        for (var index = 0; index < mocks.Length; index++)
        {
            var mock = Mock.Create<InterceptionDispatchTarget>();
            Mock.When(() => call(mock, Arg.Any<int>())).Return(index);
            mocks[index] = mock;
        }

        return new(operations =>
        {
            var sum = 0L;
            for (var index = 0; index < operations; index++)
                sum += call(
                    mocks[index & (mockCount - 1)],
                    index);
            Volatile.Write(ref sink, sum);
        });
    }

    private static MockPerformanceSample[] RunCold(
        Func<MockPerformanceSample>[] measurements)
    {
        var samples = new MockPerformanceSample[measurements.Length];
        for (var index = 0; index < measurements.Length; index++)
            samples[index] = measurements[index]();
        return samples;
    }

    private static MockPerformanceSample MeasureColdProxySample<TTag>()
    {
        Collect();
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var started = Stopwatch.GetTimestamp();
        var mock = Mock.CreateLoose<IColdProxyTarget<TTag>>();
        var value = mock.Invoke(1);
        var elapsed = Stopwatch.GetTimestamp() - started;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Volatile.Write(ref sink, value);
        return new(elapsed, allocated);
    }

    private static MockPerformanceSample MeasureColdTypedSample<TTag>()
    {
        Collect();
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var started = Stopwatch.GetTimestamp();
        var call = Bind(
            InvokeMethod<ColdTypedDispatchTarget<TTag>>(),
            new InterceptionColdTypedDispatchCall<TTag>(
                InvokeColdTypedOriginal));
        var mock = Mock.CreateLoose<ColdTypedDispatchTarget<TTag>>();
        Span<int> values = stackalloc int[4];
        var value = call(mock, values);
        var elapsed = Stopwatch.GetTimestamp() - started;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Volatile.Write(ref sink, value);
        return new(elapsed, allocated);
    }

    private static MockPerformanceResult Summarize(
        string name,
        string unit,
        int operations,
        MockPerformanceSample[] samples,
        string notes)
    {
        var elapsed = new double[samples.Length];
        var allocated = new double[samples.Length];
        var hasAllocations = samples[0].AllocatedBytes.HasValue;
        for (var index = 0; index < samples.Length; index++)
        {
            elapsed[index] =
                samples[index].ElapsedTicks * 1_000_000_000.0 /
                Stopwatch.Frequency /
                operations;
            allocated[index] = hasAllocations
                ? samples[index].AllocatedBytes!.Value / (double)operations
                : 0;
        }

        Array.Sort(elapsed);
        if (hasAllocations)
            Array.Sort(allocated);

        var middle = samples.Length / 2;
        return new(
            name,
            unit,
            operations,
            elapsed[middle],
            elapsed[0],
            elapsed[^1],
            elapsed[^1] - elapsed[0],
            hasAllocations ? allocated[middle] : null,
            notes);
    }

    private static void PrimeColdHarness()
    {
        var proxy = Mock.CreateLoose<IColdProxyTarget<Version>>();
        _ = proxy.Invoke(0);

        var call = Bind(
            InvokeMethod<ColdTypedDispatchTarget<Version>>(),
            new InterceptionColdTypedDispatchCall<Version>(
                InvokeColdTypedOriginal));
        var typed = Mock.CreateLoose<ColdTypedDispatchTarget<Version>>();
        Span<int> values = stackalloc int[1];
        _ = call(typed, values);
    }

    /// <summary>Binds one exact receiver-first call to the interception runtime seam.</summary>
    private static TDelegate Bind<TDelegate>(
        MethodInfo operation,
        TDelegate original)
        where TDelegate : Delegate =>
        MockInterceptionOperationRuntime.Bind(
            new(
                typeof(MockPerformanceFixture).Module.ModuleVersionId,
                typeof(MockPerformanceFixture).MetadataToken,
                Interlocked.Increment(ref nextSiteOffset),
                MockInvocationOperationKind.InstanceMethod),
            operation,
            original);

    /// <summary>Finds the single public instance operation on a fixture target.</summary>
    private static MethodInfo InvokeMethod<TTarget>() =>
        typeof(TTarget).GetMethod(
            "Invoke",
            BindingFlags.Instance | BindingFlags.Public)!;

    /// <summary>Preserves the ordinary concrete operation behind its interception wrapper.</summary>
    private static int InvokeInterceptionOriginal(
        InterceptionDispatchTarget target,
        int value) =>
        target.Invoke(value);

    /// <summary>Preserves the value-and-span operation behind its interception wrapper.</summary>
    private static int InvokeTypedOriginal(
        TypedDispatchTarget target,
        int value,
        Span<int> values) =>
        target.Invoke(value, values);

    /// <summary>Preserves the span operation behind its interception wrapper.</summary>
    private static int InvokeSpanOriginal(
        ConfiguredTypedDispatchTarget target,
        Span<int> values) =>
        target.Invoke(values);

    /// <summary>Preserves the ref/out operation behind its interception wrapper.</summary>
    private static int InvokeRefOutOriginal(
        PartialRefOutDispatchTarget target,
        ref int value,
        out int doubled) =>
        target.Invoke(ref value, out doubled);

    /// <summary>Preserves one closed generic span operation behind its interception wrapper.</summary>
    private static int InvokeColdTypedOriginal<TTag>(
        ColdTypedDispatchTarget<TTag> target,
        Span<int> values) =>
        target.Invoke(values);

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
