namespace AlvorKit.Interception.Test;

[TestClass]
public sealed class InterceptionCollisionRegistryConcurrencyTest
{
    private static readonly InterceptionClaimConsumer Consumer =
        new("MIR-80");
    private static readonly Guid ModuleId =
        new("7C27FB5A-59D5-494E-B7D2-997522999257");
    private static readonly TimeSpan CoordinationTimeout =
        TimeSpan.FromSeconds(10);

    /// <summary>Disjoint physical claims can be acquired, updated, and retired concurrently.</summary>
    [TestMethod]
    public void DisjointClaims_ConcurrentAcquireAndRelease_LeavesRegistryEmpty()
    {
        const int workerCount = 8;
        const int stressIterations = 128;
        var registry = new InterceptionCollisionRegistry();
        var method = Target(1, "Caller.Run");
        using var phase = new Barrier(workerCount);
        var observedSimultaneousCount = 0;
        var workers = new Task[workerCount];

        for (var worker = 0; worker < workerCount; worker++)
        {
            var workerIndex = worker;
            workers[worker] = LongRunningTask(() =>
            {
                SignalAndWait(phase, "Disjoint workers did not start together.");
                using (var initial = registry.Acquire(
                    Claim(
                        method,
                        InterceptionPhysicalRegion.IlRange(workerIndex * 4, 2),
                        $"worker:{workerIndex}:initial")))
                {
                    SignalAndWait(
                        phase,
                        "Disjoint workers did not acquire their initial claims.");
                    if (workerIndex == 0)
                        observedSimultaneousCount = registry.Count;
                    SignalAndWait(
                        phase,
                        "Disjoint workers did not observe the initial registry state.");
                }

                for (var iteration = 0;
                     iteration < stressIterations;
                     iteration++)
                {
                    using var lease = registry.Acquire(
                        Claim(
                            method,
                            InterceptionPhysicalRegion.IlRange(
                                workerIndex * 4,
                                2),
                            $"worker:{workerIndex}:{iteration}"));
                    lease.UpdateSelector(
                        $"worker:{workerIndex}:{iteration}:updated");
                }
            });
        }

        Assert.IsTrue(
            Task.WaitAll(workers, TimeSpan.FromSeconds(30)),
            "Disjoint claim workers did not complete within the stress bound.");
        Assert.AreEqual(workerCount, observedSimultaneousCount);
        Assert.AreEqual(0, registry.Count);
        Assert.AreEqual(0, registry.Snapshot().Length);
    }

    /// <summary>Two racing claims for one physical region always produce one canonical collision.</summary>
    [TestMethod]
    public void CollidingClaims_ConcurrentAcquire_IsSymmetricAndSingleWinner()
    {
        const int stressIterations = 128;
        var method = Target(1, "Caller.Run");
        var firstClaim = Claim(
            method,
            InterceptionPhysicalRegion.IlRange(10, 4),
            "alpha");
        var secondClaim = Claim(
            method,
            InterceptionPhysicalRegion.IlRange(12, 4),
            "omega");
        var forwardMessage = CollisionMessage(firstClaim, secondClaim);
        var reverseMessage = CollisionMessage(secondClaim, firstClaim);

        Assert.AreEqual(forwardMessage, reverseMessage);

        for (var iteration = 0;
             iteration < stressIterations;
             iteration++)
        {
            var registry = new InterceptionCollisionRegistry();
            using var start = new Barrier(2);
            var leftClaim = iteration % 2 == 0
                ? firstClaim
                : secondClaim;
            var rightClaim = iteration % 2 == 0
                ? secondClaim
                : firstClaim;
            var left = Task.Run(
                () => AcquireAtBarrier(registry, leftClaim, start));
            var right = Task.Run(
                () => AcquireAtBarrier(registry, rightClaim, start));

            Assert.IsTrue(
                Task.WaitAll([left, right], CoordinationTimeout),
                $"Collision race {iteration} did not complete.");
            var leftResult = left.Result;
            var rightResult = right.Result;
            try
            {
                Assert.AreNotEqual(
                    leftResult.Lease is null,
                    rightResult.Lease is null,
                    $"Collision race {iteration} did not have exactly one winner.");
                Assert.AreNotEqual(
                    leftResult.Collision is null,
                    rightResult.Collision is null,
                    $"Collision race {iteration} did not have exactly one rejection.");
                var collision =
                    leftResult.Collision ??
                    rightResult.Collision;
                Assert.IsNotNull(collision);
                Assert.AreEqual(
                    InterceptionCollisionReason.PhysicalRegion,
                    collision.Collision.Reason);
                Assert.AreEqual(forwardMessage, collision.Message);
                Assert.AreEqual(1, registry.Count);
            }
            finally
            {
                leftResult.Lease?.Dispose();
                rightResult.Lease?.Dispose();
            }

            Assert.AreEqual(0, registry.Count);
        }
    }

    /// <summary>Repeated concurrent disposal retires a slot once and keeps the region reusable.</summary>
    [TestMethod]
    public void ClaimLease_ConcurrentRepeatedDispose_IsIdempotent()
    {
        var registry = new InterceptionCollisionRegistry();
        var claim = Claim(
            Target(1, "Caller.Run"),
            InterceptionPhysicalRegion.MethodWide,
            "scope:root");
        var first = registry.Acquire(claim);
        var firstSlotId = first.Slot.SlotId;

        Parallel.For(0, 1024, _ => first.Dispose());
        first.Dispose();

        Assert.IsFalse(first.IsActive);
        Assert.AreEqual(0, registry.Count);
        Assert.AreEqual(0, registry.Snapshot().Length);
        Assert.ThrowsExactly<ObjectDisposedException>(
            () => first.UpdateSelector("scope:retired"));

        using var replacement = registry.Acquire(claim);
        Assert.IsTrue(replacement.IsActive);
        Assert.IsTrue(replacement.Slot.SlotId > firstSlotId);
        Assert.AreEqual(1, registry.Count);
    }

    /// <summary>The live registry does not retain owner or selector metadata from retired leases.</summary>
    [TestMethod]
    public void RetiredLeases_DoNotRemainRootedByRegistry()
    {
        const int claimCount = 32;
        var registry = new InterceptionCollisionRegistry();
        var retired = new RetiredMetadata[claimCount];

        for (var index = 0; index < claimCount; index++)
            retired[index] = AcquireUpdateAndRetire(registry, index);

        Assert.AreEqual(0, registry.Count);
        Assert.AreEqual(0, registry.Snapshot().Length);

        for (var attempt = 0;
             attempt < 10 && retired.Any(IsAlive);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(
            retired.Any(IsAlive),
            "The registry retained owner or selector metadata after every lease retired.");
        GC.KeepAlive(registry);
    }

    private static AcquisitionAttempt AcquireAtBarrier(
        InterceptionCollisionRegistry registry,
        InterceptionClaim claim,
        Barrier start)
    {
        SignalAndWait(start, "Colliding claim workers did not start together.");
        try
        {
            return new(registry.Acquire(claim), null);
        }
        catch (InterceptionCollisionException exception)
        {
            return new(null, exception);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RetiredMetadata AcquireUpdateAndRetire(
        InterceptionCollisionRegistry registry,
        int index)
    {
        var consumer = new InterceptionClaimConsumer(
            string.Concat(
                "consumer:",
                index,
                ":",
                Guid.NewGuid().ToString("N")));
        var initialSelector = string.Concat(
            "selector:initial:",
            Guid.NewGuid().ToString("N"));
        var claim = new InterceptionClaim(
            Target(index + 1, $"Caller.{index}"),
            InterceptionPhysicalRegion.MethodWide,
            new(consumer, initialSelector));
        var lease = registry.Acquire(claim);
        var updatedSelector = string.Concat(
            "selector:updated:",
            Guid.NewGuid().ToString("N"));
        lease.UpdateSelector(updatedSelector);
        var updatedOwner = lease.Slot.Claim.Owner;
        var retired = new RetiredMetadata(
            new(updatedOwner),
            new(updatedSelector));
        lease.Dispose();
        return retired;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAlive(RetiredMetadata retired) =>
        retired.Owner.TryGetTarget(out _) ||
        retired.Selector.TryGetTarget(out _);

    private static string CollisionMessage(
        InterceptionClaim first,
        InterceptionClaim second)
    {
        var registry = new InterceptionCollisionRegistry();
        using var lease = registry.Acquire(first);
        return Assert.ThrowsExactly<InterceptionCollisionException>(
            () => registry.Acquire(second)).Message;
    }

    private static Task LongRunningTask(Action action) =>
        Task.Factory.StartNew(
            action,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

    private static void SignalAndWait(
        Barrier barrier,
        string failureMessage)
    {
        if (!barrier.SignalAndWait(CoordinationTimeout))
            throw new TimeoutException(failureMessage);
    }

    private static InterceptionClaim Claim(
        InterceptionTarget method,
        InterceptionPhysicalRegion region,
        string selector) =>
        new(
            method,
            region,
            new(Consumer, selector));

    private static InterceptionTarget Target(
        int row,
        string display) =>
        InterceptionTarget.FromIdentity(
            ModuleId,
            0x06000000 | row,
            checked((ulong)row),
            display);

    private sealed record AcquisitionAttempt(
        InterceptionClaimLease? Lease,
        InterceptionCollisionException? Collision);

    private sealed record RetiredMetadata(
        WeakReference<InterceptionClaimOwner> Owner,
        WeakReference<string> Selector);
}
