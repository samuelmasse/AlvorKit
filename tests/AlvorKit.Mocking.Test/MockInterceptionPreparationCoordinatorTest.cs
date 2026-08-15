namespace AlvorKit;

/// <summary>Verifies neutral transactional interception preparation and diagnostics.</summary>
[TestClass]
public sealed class MockInterceptionPreparationCoordinatorTest
{
    /// <summary>Every required public failure category provides concrete recovery guidance.</summary>
    [TestMethod]
    [DataRow(MockInterceptionPreparationFailureReason.ProfilerUnavailable)]
    [DataRow(MockInterceptionPreparationFailureReason.AbiMismatch)]
    [DataRow(MockInterceptionPreparationFailureReason.ModuleAllowlistRejected)]
    [DataRow(MockInterceptionPreparationFailureReason.StaleBody)]
    [DataRow(MockInterceptionPreparationFailureReason.UnsupportedSignature)]
    [DataRow(MockInterceptionPreparationFailureReason.PreparationFailed)]
    [DataRow(MockInterceptionPreparationFailureReason.Collision)]
    [DataRow(MockInterceptionPreparationFailureReason.RejitFailed)]
    [DataRow(MockInterceptionPreparationFailureReason.RollbackFailed)]
    public void DiagnosticCategoriesProvideActionableRecovery(
        MockInterceptionPreparationFailureReason reason)
    {
        var diagnostic = new MockInterceptionPreparationDiagnostic(
            reason,
            "caller::member/site",
            "controlled failure");

        Assert.AreEqual(reason, diagnostic.Reason);
        Assert.AreEqual("caller::member/site", diagnostic.RouteId);
        Assert.AreEqual("controlled failure", diagnostic.Detail);
        Assert.IsFalse(string.IsNullOrWhiteSpace(
            diagnostic.SuggestedAction));
        StringAssert.Contains(diagnostic.Message, reason.ToString());
        StringAssert.Contains(diagnostic.Message, "Action:");
        Assert.AreEqual(diagnostic.Message, diagnostic.ToString());
        if (reason ==
            MockInterceptionPreparationFailureReason
                .ModuleAllowlistRejected)
        {
            StringAssert.Contains(
                diagnostic.SuggestedAction,
                "ALVORKIT_INTERCEPTION_MODULES");
            StringAssert.Contains(
                diagnostic.SuggestedAction,
                "module name");
            StringAssert.Contains(
                diagnostic.SuggestedAction,
                "MVID separately");
        }
    }

    /// <summary>All routes prepare before activation and successful ownership rolls back in LIFO order.</summary>
    [TestMethod]
    public void PrepareAndActivate_SucceedsTransactionally()
    {
        RecordingRouteLifecycle lifecycle = new();
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate([first, second]);

        Assert.IsTrue(result.IsSuccessful);
        Assert.IsTrue(result.Diagnostics.IsEmpty);
        CollectionAssert.AreEqual(
            new[] { "prepare:first", "prepare:second", "activate:first", "activate:second" },
            lifecycle.Events);
        first.RequireActivated();
        second.RequireActivated();
        Assert.IsTrue(first.IsActivated);
        Assert.IsTrue(second.IsActivated);

        result.Activation!.Dispose();
        result.Activation.Dispose();

        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "activate:first",
                "activate:second",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);
        Assert.IsFalse(result.Activation.IsActive);
        Assert.Throws<MockException>(first.RequireActivated);
    }

    /// <summary>A preparation rejection rolls every attempted route back in LIFO order and preserves Dynamic.</summary>
    [TestMethod]
    public void PreparationFailure_BlocksRoutesAndPreservesDynamicProxy()
    {
        MockDynamic.Enable();
        IMockProxyCallbackBackend proxy =
            MockRuntimeBackendRegistry.Proxy;
        IMockOperationBackend? operation =
            MockRuntimeBackendRegistry.ExplicitOperation;
        RecordingRouteLifecycle lifecycle = new();
        lifecycle.PreparationFailures.Add(
            "second",
            MockInterceptionPreparationFailureReason.UnsupportedSignature);
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        MockInterceptionRoute third = new("third");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate(
            [first, second, third]);

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNull(result.Activation);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.UnsupportedSignature,
            result.Diagnostics.Single().Reason);
        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "prepare:third",
                "rollback:third",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        Assert.IsTrue(lifecycle.ActiveRoutes.Count == 0);
        Assert.Throws<MockException>(second.RequireActivated);
        Assert.AreSame(proxy, MockRuntimeBackendRegistry.Proxy);
        Assert.AreSame(
            operation,
            MockRuntimeBackendRegistry.ExplicitOperation);
    }

    /// <summary>A failed ReJIT reverses every prepared route, including a later inactive route.</summary>
    [TestMethod]
    public void ActivationFailure_RollsBackPartialActivation()
    {
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim proceed = new();
        RecordingRouteLifecycle lifecycle = new()
        {
            BlockActivationRouteId = "second",
            ActivationEntered = entered,
            ContinueActivation = proceed
        };
        lifecycle.ActivationFailures.Add(
            "second",
            MockInterceptionPreparationFailureReason.RejitFailed);
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        MockInterceptionRoute third = new("third");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        Task<MockInterceptionPreparationResult> resultTask =
            Task.Run(() => coordinator.PrepareAndActivate(
                [first, second, third]));

        try
        {
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            CollectionAssert.AreEquivalent(
                new[] { "first", "second" },
                lifecycle.ActiveRoutes.ToArray());
            Assert.IsFalse(first.IsActivated);
            Assert.Throws<MockException>(first.RequireActivated);
        }
        finally
        {
            proceed.Set();
        }

        var result = resultTask.GetAwaiter().GetResult();

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNull(result.Activation);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RejitFailed,
            result.Diagnostics.Single().Reason);
        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "prepare:third",
                "activate:first",
                "activate:second",
                "rollback:third",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        Assert.IsTrue(lifecycle.ActiveRoutes.Count == 0);
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);
        Assert.IsFalse(third.IsActivated);
        Assert.Throws<MockException>(first.RequireActivated);
        Assert.Throws<MockException>(second.RequireActivated);
    }

    /// <summary>The shared publication gate blocks early routes until the final route is ready.</summary>
    [TestMethod]
    public void CompleteActivation_PublishesAllRoutesTogether()
    {
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim proceed = new();
        RecordingRouteLifecycle lifecycle = new()
        {
            BlockActivationRouteId = "second",
            ActivationEntered = entered,
            ContinueActivation = proceed
        };
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        Task<MockInterceptionPreparationResult> resultTask =
            Task.Run(() => coordinator.PrepareAndActivate([first, second]));

        try
        {
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            CollectionAssert.AreEquivalent(
                new[] { "first", "second" },
                lifecycle.ActiveRoutes.ToArray());
            Assert.IsFalse(first.IsActivated);
            Assert.IsFalse(second.IsActivated);
            Assert.Throws<MockException>(first.RequireActivated);
        }
        finally
        {
            proceed.Set();
        }

        var result = resultTask.GetAwaiter().GetResult();

        Assert.IsTrue(result.IsSuccessful);
        first.RequireActivated();
        second.RequireActivated();
        Assert.IsTrue(first.IsActivated);
        Assert.IsTrue(second.IsActivated);
        result.Activation!.Dispose();
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);
    }

    /// <summary>A throwing preparation becomes actionable and still rolls all attempted routes back.</summary>
    [TestMethod]
    public void PreparationException_ReturnsDiagnosticAndRollsBack()
    {
        RecordingRouteLifecycle lifecycle = new();
        lifecycle.PreparationExceptions.Add("first");
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate([first, second]);

        Assert.IsFalse(result.IsSuccessful);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.PreparationFailed,
            result.Diagnostics.Single().Reason);
        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);
    }

    /// <summary>A rollback exception is recorded while remaining routes still unwind.</summary>
    [TestMethod]
    public void RollbackException_RecordsFailureAndContinues()
    {
        RecordingRouteLifecycle lifecycle = new();
        lifecycle.ActivationFailures.Add(
            "second",
            MockInterceptionPreparationFailureReason.RejitFailed);
        lifecycle.RollbackExceptions.Add("second");
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        MockInterceptionRoute third = new("third");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate(
            [first, second, third]);

        CollectionAssert.AreEqual(
            new[]
            {
                MockInterceptionPreparationFailureReason.RejitFailed,
                MockInterceptionPreparationFailureReason.RollbackFailed
            },
            result.Diagnostics
                .Select(diagnostic => diagnostic.Reason)
                .ToArray());
        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "prepare:third",
                "activate:first",
                "activate:second",
                "rollback:third",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        CollectionAssert.AreEquivalent(
            new[] { "second" },
            lifecycle.ActiveRoutes.ToArray());
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);
        Assert.IsFalse(third.IsActivated);

        string[] eventsBeforeRetry = [.. lifecycle.Events];
        var sameRouteRetry =
            coordinator.PrepareAndActivate([second]);
        var sameIdentityRetry =
            coordinator.PrepareAndActivate([new("second")]);

        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            sameRouteRetry.Diagnostics.Single().Reason);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            sameIdentityRetry.Diagnostics.Single().Reason);
        CollectionAssert.AreEqual(
            eventsBeforeRetry,
            lifecycle.Events);
        MockException unavailable =
            Assert.ThrowsExactly<MockException>(
                second.RequireActivated);
        StringAssert.Contains(unavailable.Message, "restart");
    }

    /// <summary>A failed owned-activation rollback poisons its identity while other routes release.</summary>
    [TestMethod]
    public void ActivationDisposeFailure_PoisonsIdentityAndContinues()
    {
        RecordingRouteLifecycle lifecycle = new();
        MockInterceptionRoute first = new("first");
        MockInterceptionRoute second = new("second");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var result =
            coordinator.PrepareAndActivate([first, second]);
        lifecycle.RollbackExceptions.Add("second");

        AggregateException exception =
            Assert.ThrowsExactly<AggregateException>(
                result.Activation!.Dispose);

        Assert.AreEqual(1, exception.InnerExceptions.Count);
        CollectionAssert.AreEqual(
            new[]
            {
                "prepare:first",
                "prepare:second",
                "activate:first",
                "activate:second",
                "rollback:second",
                "rollback:first"
            },
            lifecycle.Events);
        CollectionAssert.AreEquivalent(
            new[] { "second" },
            lifecycle.ActiveRoutes.ToArray());
        Assert.IsFalse(result.Activation.IsActive);
        Assert.IsFalse(first.IsActivated);
        Assert.IsFalse(second.IsActivated);

        string[] eventsBeforeRetry = [.. lifecycle.Events];
        var sameRouteRetry =
            coordinator.PrepareAndActivate([second]);
        var sameIdentityRetry =
            coordinator.PrepareAndActivate([new("second")]);

        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            sameRouteRetry.Diagnostics.Single().Reason);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            sameIdentityRetry.Diagnostics.Single().Reason);
        CollectionAssert.AreEqual(
            eventsBeforeRetry,
            lifecycle.Events);

        var healthyRetry =
            coordinator.PrepareAndActivate([first]);
        Assert.IsTrue(healthyRetry.IsSuccessful);
        healthyRetry.Activation!.Dispose();
    }

    /// <summary>Distinct objects sharing one identity reject before a concurrent lifecycle call.</summary>
    [TestMethod]
    public void ConcurrentTransaction_CannotPrepareReservedRoute()
    {
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim proceed = new();
        RecordingRouteLifecycle lifecycle = new()
        {
            PrepareEntered = entered,
            ContinuePreparation = proceed
        };
        MockInterceptionRoute firstRoute = new("shared");
        MockInterceptionRoute secondRoute = new("shared");
        var firstCoordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        var secondCoordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);
        Task<MockInterceptionPreparationResult> firstTask =
            Task.Run(() =>
                firstCoordinator.PrepareAndActivate([firstRoute]));

        MockInterceptionPreparationResult second;
        try
        {
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsFalse(firstRoute.IsActivated);
            Assert.Throws<MockException>(
                firstRoute.RequireActivated);

            second = secondCoordinator.PrepareAndActivate(
                [secondRoute]);
        }
        finally
        {
            proceed.Set();
        }

        var first = firstTask.GetAwaiter().GetResult();

        Assert.IsFalse(second.IsSuccessful);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.Collision,
            second.Diagnostics.Single().Reason);
        Assert.IsTrue(first.IsSuccessful);
        CollectionAssert.AreEqual(
            new[] { "prepare:shared", "activate:shared" },
            lifecycle.Events);
        first.Activation!.Dispose();
        Assert.IsFalse(firstRoute.IsActivated);
        Assert.IsFalse(secondRoute.IsActivated);

        var retry =
            secondCoordinator.PrepareAndActivate([secondRoute]);

        Assert.IsTrue(retry.IsSuccessful);
        retry.Activation!.Dispose();
    }

    /// <summary>Duplicate exact route identities reject before backend preparation.</summary>
    [TestMethod]
    public void DuplicateRouteIdentity_RejectsAsCollision()
    {
        RecordingRouteLifecycle lifecycle = new();
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate(
            [new("same"), new("same")]);

        Assert.IsFalse(result.IsSuccessful);
        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.Collision,
            result.Diagnostics.Single().Reason);
        Assert.IsTrue(lifecycle.Events.Count == 0);
        StringAssert.Contains(
            result.Diagnostics.Single().SuggestedAction,
            "overlapping");
    }

    /// <summary>A poisoned identity claim does not keep its lifecycle alive globally.</summary>
    [TestMethod]
    public void PoisonedIdentity_DoesNotRetainLifecycle()
    {
        WeakReference lifecycle = CreatePoisonedLifecycleReference();

        for (var attempt = 0;
            attempt < 10 && lifecycle.IsAlive;
            ++attempt)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(lifecycle.IsAlive);
    }

    /// <summary>Creates an otherwise-unrooted lifecycle with a poisoned claim.</summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference CreatePoisonedLifecycleReference()
    {
        RecordingRouteLifecycle lifecycle = new();
        lifecycle.ActivationFailures.Add(
            "poisoned",
            MockInterceptionPreparationFailureReason.RejitFailed);
        lifecycle.RollbackExceptions.Add("poisoned");
        var coordinator =
            new MockInterceptionPreparationCoordinator(lifecycle);

        var result = coordinator.PrepareAndActivate(
            [new("poisoned")]);

        Assert.AreEqual(
            MockInterceptionPreparationFailureReason.RollbackFailed,
            result.Diagnostics[^1].Reason);
        return new(lifecycle);
    }

    /// <summary>Records controlled route lifecycle calls and failures.</summary>
    private sealed class RecordingRouteLifecycle :
        IMockInterceptionRouteLifecycle
    {
        /// <summary>The ordered lifecycle calls.</summary>
        internal List<string> Events { get; } = [];

        /// <summary>Preparation failures keyed by route identity.</summary>
        internal Dictionary<
            string,
            MockInterceptionPreparationFailureReason> PreparationFailures
        { get; } = [];

        /// <summary>Activation failures keyed by route identity.</summary>
        internal Dictionary<
            string,
            MockInterceptionPreparationFailureReason> ActivationFailures
        { get; } = [];

        /// <summary>Route identities whose preparation throws unexpectedly.</summary>
        internal HashSet<string> PreparationExceptions { get; } =
            new(StringComparer.Ordinal);

        /// <summary>Route identities whose rollback throws unexpectedly.</summary>
        internal HashSet<string> RollbackExceptions { get; } =
            new(StringComparer.Ordinal);

        /// <summary>Routes whose simulated backend generation is active.</summary>
        internal HashSet<string> ActiveRoutes { get; } =
            new(StringComparer.Ordinal);

        /// <summary>The route identity whose activation waits at a controlled gate.</summary>
        internal string? BlockActivationRouteId { get; init; }

        /// <summary>An optional signal published after entering preparation.</summary>
        internal ManualResetEventSlim? PrepareEntered { get; init; }

        /// <summary>An optional gate that blocks controlled preparation.</summary>
        internal ManualResetEventSlim? ContinuePreparation { get; init; }

        /// <summary>An optional signal published after entering blocked activation.</summary>
        internal ManualResetEventSlim? ActivationEntered { get; init; }

        /// <summary>An optional gate that blocks one controlled activation.</summary>
        internal ManualResetEventSlim? ContinueActivation { get; init; }

        /// <summary>Previews one controlled route without activation.</summary>
        public MockInterceptionPreparationDiagnostic? Prepare(
            MockInterceptionRoute route)
        {
            Events.Add($"prepare:{route.Id}");
            PrepareEntered?.Set();
            ContinuePreparation?.Wait();
            if (PreparationExceptions.Contains(route.Id))
            {
                throw new InvalidOperationException(
                    "controlled preparation exception");
            }

            return PreparationFailures.TryGetValue(
                route.Id,
                out var reason)
                ? Failure(route, reason)
                : null;
        }

        /// <summary>Simulates activation before returning any controlled failure.</summary>
        public MockInterceptionPreparationDiagnostic? Activate(
            MockInterceptionRoute route)
        {
            Events.Add($"activate:{route.Id}");
            ActiveRoutes.Add(route.Id);
            if (StringComparer.Ordinal.Equals(
                    route.Id,
                    BlockActivationRouteId))
            {
                ActivationEntered?.Set();
                ContinueActivation?.Wait();
            }

            return ActivationFailures.TryGetValue(
                route.Id,
                out var reason)
                ? Failure(route, reason)
                : null;
        }

        /// <summary>Restores one simulated pristine route.</summary>
        public void Rollback(MockInterceptionRoute route)
        {
            Events.Add($"rollback:{route.Id}");
            if (RollbackExceptions.Contains(route.Id))
            {
                throw new InvalidOperationException(
                    "controlled rollback exception");
            }

            ActiveRoutes.Remove(route.Id);
        }

        /// <summary>Creates one controlled actionable failure.</summary>
        private static MockInterceptionPreparationDiagnostic Failure(
            MockInterceptionRoute route,
            MockInterceptionPreparationFailureReason reason) =>
            new(reason, route.Id, $"controlled {reason} failure");
    }
}
