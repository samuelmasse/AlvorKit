namespace AlvorKit.Interception.Test;

[TestClass]
public sealed class InterceptionCollisionRegistryTest
{
    private static readonly Guid ModuleId =
        new("8DA409B2-A7BD-48C8-9749-206368A48050");
    private static readonly InterceptionClaimConsumer RuntimeRewrite =
        new("RuntimeRewrite");
    private static readonly InterceptionClaimConsumer Mocking =
        new("Mocking");

    /// <summary>Verifies disjoint caller sites for one consumer compose in a shared method.</summary>
    [TestMethod]
    public void Acquire_DisjointSitesForSameConsumer_Coexist()
    {
        var registry = new InterceptionCollisionRegistry();
        var caller = Target(1, "Caller.Run");
        var operand = InterceptionLogicalOperand.ForMethod(
            Target(2, "Service.Calculate"));
        using var first = registry.Acquire(
            Claim(
                caller,
                InterceptionPhysicalRegion.IlRange(10, 2),
                "Mocking",
                "site-a",
                operand));
        using var second = registry.Acquire(
            Claim(
                caller,
                InterceptionPhysicalRegion.IlRange(20, 2),
                "Mocking",
                "site-b",
                operand));

        Assert.AreEqual(2, registry.Count);
        Assert.AreEqual(2, registry.Snapshot().Length);
    }

    /// <summary>Verifies physical identity includes the loaded method as well as the IL region.</summary>
    [TestMethod]
    public void Acquire_SameRegionInDifferentMethods_Coexists()
    {
        var registry = new InterceptionCollisionRegistry();
        using var first = registry.Acquire(
            Claim(
                Target(1, "First.Run"),
                InterceptionPhysicalRegion.IlRange(10, 2),
                "RuntimeRewrite",
                "all"));
        using var second = registry.Acquire(
            Claim(
                Target(2, "Second.Run"),
                InterceptionPhysicalRegion.IlRange(10, 2),
                "Mocking",
                "site:run"));

        Assert.AreEqual(2, registry.Count);
    }

    /// <summary>Verifies owner and selector metadata cannot create two keys for one physical region.</summary>
    [TestMethod]
    public void Acquire_SamePhysicalRegionWithDifferentMetadata_Collides()
    {
        var registry = new InterceptionCollisionRegistry();
        var method = Target(1, "Caller.Run");
        using var first = registry.Acquire(
            Claim(
                method,
                InterceptionPhysicalRegion.IlRange(10, 2),
                "Mocking",
                "site-a"));

        var exception = Assert.ThrowsExactly<InterceptionCollisionException>(
            () => registry.Acquire(
                Claim(
                    method,
                    InterceptionPhysicalRegion.IlRange(10, 2),
                    "Mocking",
                    "site-b")));

        Assert.AreEqual(
            InterceptionCollisionReason.PhysicalRegion,
            exception.Collision.Reason);
        StringAssert.Contains(exception.Message, "site-a");
        StringAssert.Contains(exception.Message, "site-b");
    }

    /// <summary>Verifies a method-wide claim collides with every site independently of registration order.</summary>
    [TestMethod]
    public void Acquire_MethodWideAndSiteCollision_IsOrderIndependent()
    {
        var method = Target(1, "Caller.Run");
        var operand = InterceptionLogicalOperand.ForMethod(method);
        var runtimeRewrite = Claim(
            method,
            InterceptionPhysicalRegion.MethodWide,
            "RuntimeRewrite",
            "scope:root",
            operand);
        var mocking = Claim(
            method,
            InterceptionPhysicalRegion.IlRange(14, 3),
            "Mocking",
            "site:calculate",
            operand);

        var forward = CollisionMessage(runtimeRewrite, mocking);
        var reverse = CollisionMessage(mocking, runtimeRewrite);

        Assert.AreEqual(forward, reverse);
        StringAssert.Contains(forward, "physical region");
        StringAssert.Contains(forward, "RuntimeRewrite");
        StringAssert.Contains(forward, "Mocking");
        StringAssert.Contains(forward, "scope:root");
        StringAssert.Contains(forward, "site:calculate");
        StringAssert.Contains(forward, "method-wide");
        StringAssert.Contains(forward, "logical=");
    }

    /// <summary>Verifies a caller-site claim conflicts with a different consumer's callee claim in either order.</summary>
    [TestMethod]
    public void Acquire_SharedLogicalOperandAcrossMethods_IsOrderIndependent()
    {
        var caller = Target(1, "Caller.Run");
        var callee = Target(2, "Service.Calculate");
        var operand = InterceptionLogicalOperand.ForMethod(callee);
        var runtimeRewrite = Claim(
            callee,
            InterceptionPhysicalRegion.MethodWide,
            "RuntimeRewrite",
            "all",
            operand);
        var mocking = Claim(
            caller,
            InterceptionPhysicalRegion.IlRange(22, 5),
            "Mocking",
            "site:calculate",
            operand);

        var forward = CollisionMessage(runtimeRewrite, mocking);
        var reverse = CollisionMessage(mocking, runtimeRewrite);

        Assert.AreEqual(forward, reverse);
        StringAssert.Contains(forward, "logical operand");
        StringAssert.Contains(forward, "Caller.Run");
        StringAssert.Contains(forward, "Service.Calculate");
    }

    /// <summary>Verifies equal diagnostic names do not grant cross-owner composition rights.</summary>
    [TestMethod]
    public void Acquire_DistinctConsumersWithSameName_CollideLogically()
    {
        var registry = new InterceptionCollisionRegistry();
        var operand = InterceptionLogicalOperand.ForMethod(
            Target(3, "Service.Calculate"));
        var firstConsumer = new InterceptionClaimConsumer("Plugin");
        var secondConsumer = new InterceptionClaimConsumer("Plugin");
        using var first = registry.Acquire(
            new(
                Target(1, "FirstCaller.Run"),
                InterceptionPhysicalRegion.IlRange(10, 2),
                new(firstConsumer, "first"),
                operand));

        var exception = Assert.ThrowsExactly<InterceptionCollisionException>(
            () => registry.Acquire(
                new(
                    Target(2, "SecondCaller.Run"),
                    InterceptionPhysicalRegion.IlRange(20, 2),
                    new(secondConsumer, "second"),
                    operand)));

        Assert.AreEqual(
            InterceptionCollisionReason.LogicalOperand,
            exception.Collision.Reason);
    }

    /// <summary>Verifies disposing a claim lease makes its physical slot immediately reusable.</summary>
    [TestMethod]
    public void ClaimLease_Dispose_ReleasesPhysicalSlot()
    {
        var registry = new InterceptionCollisionRegistry();
        var claim = Claim(
            Target(1, "Caller.Run"),
            InterceptionPhysicalRegion.MethodWide,
            "RuntimeRewrite",
            "all");
        var first = registry.Acquire(claim);
        var slotId = first.Slot.SlotId;
        first.UpdateSelector("scope:updated");

        Assert.AreEqual(slotId, first.Slot.SlotId);
        Assert.AreEqual(
            "scope:updated",
            first.Slot.Claim.Owner.Selector);

        first.Dispose();
        using var second = registry.Acquire(claim);

        Assert.IsFalse(first.IsActive);
        Assert.IsTrue(second.IsActive);
        Assert.AreEqual(1, registry.Count);
    }

    private static string CollisionMessage(
        InterceptionClaim first,
        InterceptionClaim second)
    {
        var registry = new InterceptionCollisionRegistry();
        using var lease = registry.Acquire(first);
        return Assert.ThrowsExactly<InterceptionCollisionException>(
            () => registry.Acquire(second)).Message;
    }

    private static InterceptionClaim Claim(
        InterceptionTarget method,
        InterceptionPhysicalRegion region,
        string consumer,
        string selector,
        InterceptionLogicalOperand? operand = null) =>
        new(
            method,
            region,
            new(
                consumer == RuntimeRewrite.Name
                    ? RuntimeRewrite
                    : Mocking,
                selector),
            operand);

    private static InterceptionTarget Target(
        int row,
        string display) =>
        InterceptionTarget.FromIdentity(
            ModuleId,
            0x06000000 | row,
            checked((ulong)row),
            display);
}
