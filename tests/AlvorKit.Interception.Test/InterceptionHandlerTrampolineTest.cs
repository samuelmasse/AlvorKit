namespace AlvorKit.Interception.Test;

/// <summary>Verifies stable identities and the generated exact managed handler ABI.</summary>
[TestClass]
public class InterceptionHandlerTrampolineTest
{
    /// <summary>Display text does not participate in exact runtime method identity.</summary>
    [TestMethod]
    public void TargetIdentityIgnoresDisplayName()
    {
        var first = InterceptionTarget.FromIdentity(
            Guid.NewGuid(),
            0x06000001,
            42,
            "first");
        var second = InterceptionTarget.FromIdentity(
            first.ModuleMvid,
            first.MethodToken,
            first.SignatureHash,
            "second");

        Assert.AreEqual(first, second);
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
    }

    /// <summary>Generic constructor MethodDefs require construction-specific routing.</summary>
    [TestMethod]
    public void GenericDeclaringTypeConstructorIsRejected()
    {
        ConstructorInfo constructor =
            typeof(GenericConstructorTarget<int>).GetConstructor(
                [typeof(int)])!;

        var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionTarget.FromConstructor(constructor));

        StringAssert.Contains(exception.Message, "construction-specific");
    }

    /// <summary>The generated entry point preserves receiver, value, ref write-back, and return exactly.</summary>
    [TestMethod]
    public unsafe void TrampolinePreservesExactSignature()
    {
        var targetMethod = Method<ExactTarget>(nameof(ExactTarget.Calculate));
        var handler = new ExactHandler();
        using var trampoline = InterceptionHandlerTrampolineFactory.Create(
            targetMethod,
            handler,
            Method<ExactHandler>(nameof(ExactHandler.Run)));
        var target = new ExactTarget { Bias = 7 };

        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        var observed = 0;
        var result =
            ((delegate* managed<ExactTarget, int, ref int, int>)entryPoint)(
                target,
                5,
                ref observed);

        Assert.AreEqual(112, observed);
        Assert.AreEqual(336, result);
        trampoline.Dispose();
        Assert.IsFalse(trampoline.TryAcquire(out _));
    }

    /// <summary>A warmed exact handler call and lease pair allocates no managed memory.</summary>
    [TestMethod]
    public unsafe void WarmInvocationAllocatesNothing()
    {
        using var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactTarget>(nameof(ExactTarget.Calculate)),
            new ExactHandler(),
            Method<ExactHandler>(nameof(ExactHandler.Run)));
        var target = new ExactTarget();
        var observed = 0;
        for (var index = 0; index < 256; index++)
            Invoke(trampoline, target, ref observed);

        // Let the CLR perform its one-time tiered-JIT transition before
        // asserting the steady-state dispatch path.
        _ = MeasureAllocations(trampoline, target, ref observed);
        var allocated = MeasureAllocations(
            trampoline,
            target,
            ref observed);

        Assert.AreEqual(0L, allocated);
    }

    private static long MeasureAllocations(
        InterceptionHandlerTrampoline trampoline,
        ExactTarget target,
        ref int observed)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10_000; index++)
            Invoke(trampoline, target, ref observed);
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    /// <summary>Handler mismatch is rejected before any stable entry point is published.</summary>
    [TestMethod]
    public void RejectsNonExactHandler()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                Method<ExactTarget>(nameof(ExactTarget.Calculate)),
                new WrongHandler(),
                Method<WrongHandler>(nameof(WrongHandler.Run))));

        StringAssert.Contains(exception.Message, "parameter count");
    }

    /// <summary>The neutral default propagates the original handler exception and stays active.</summary>
    [TestMethod]
    public unsafe void HandlerExceptionPropagatesAndRemainsActive()
    {
        using var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactTarget>(nameof(ExactTarget.Calculate)),
            new ThrowingHandler(),
            Method<ThrowingHandler>(nameof(ThrowingHandler.Run)));
        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        var observed = 19;

        Assert.ThrowsExactly<InvalidOperationException>(
            () => _ =
                ((delegate* managed<ExactTarget, int, ref int, int>)entryPoint)(
                    new(),
                    2,
                    ref observed));

        Assert.IsNull(trampoline.Failure);
        Assert.IsTrue(trampoline.TryAcquire(out entryPoint));
        Assert.ThrowsExactly<InvalidOperationException>(
            () => _ =
                ((delegate* managed<ExactTarget, int, ref int, int>)entryPoint)(
                    new(),
                    2,
                    ref observed));
    }

    /// <summary>The explicit containment policy returns a default result and deactivates.</summary>
    [TestMethod]
    public unsafe void ContainmentPolicyReturnsDefaultAndDeactivates()
    {
        using var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactTarget>(nameof(ExactTarget.Calculate)),
            new ThrowingHandler(),
            Method<ThrowingHandler>(nameof(ThrowingHandler.Run)),
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate);
        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        var observed = 19;

        var result =
            ((delegate* managed<ExactTarget, int, ref int, int>)entryPoint)(
                new(),
                2,
                ref observed);

        Assert.AreEqual(0, result);
        Assert.AreEqual(19, observed);
        Assert.IsInstanceOfType<InvalidOperationException>(
            trampoline.Failure);
        Assert.IsFalse(trampoline.TryAcquire(out _));
    }

    /// <summary>Disposed trampolines release the submitted handler object.</summary>
    [TestMethod]
    public void DisposedHandlerIsCollectible()
    {
        WeakReference<ExactHandler> handler = CreateAndDispose();

        for (var attempt = 0;
             attempt < 10 && IsAlive(handler);
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.IsFalse(
            IsAlive(handler),
            "The submitted handler remained rooted after trampoline disposal.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAlive<T>(WeakReference<T> reference)
        where T : class =>
        reference.TryGetTarget(out _);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<ExactHandler> CreateAndDispose()
    {
        var handler = new ExactHandler();
        var reference = new WeakReference<ExactHandler>(handler);
        var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactTarget>(nameof(ExactTarget.Calculate)),
            handler,
            Method<ExactHandler>(nameof(ExactHandler.Run)));
        trampoline.Dispose();
        return reference;
    }

    private static unsafe void Invoke(
        InterceptionHandlerTrampoline trampoline,
        ExactTarget target,
        ref int observed)
    {
        if (!trampoline.TryAcquire(out var entryPoint))
            Assert.Fail("The warmed trampoline unexpectedly deactivated.");
        _ = ((delegate* managed<ExactTarget, int, ref int, int>)entryPoint)(
            target,
            3,
            ref observed);
    }

    private static MethodInfo Method<T>(string name) =>
        typeof(T).GetMethod(name)
        ?? throw new InvalidOperationException($"Method '{typeof(T).FullName}.{name}' was not found.");
}

internal sealed class GenericConstructorTarget<T>(T value)
{
    internal T Value { get; } = value;
}

/// <summary>Ordinary target used to define an exact receiver/ref/return signature.</summary>
public sealed class ExactTarget
{
    public int Bias { get; init; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Calculate(int value, ref int observed)
    {
        observed = value + Bias;
        return observed * 2;
    }
}

/// <summary>Exact replacement handler.</summary>
public sealed class ExactHandler
{
    public int Run(ExactTarget receiver, int value, ref int observed)
    {
        observed = receiver.Bias + value + 100;
        return observed * 3;
    }
}

/// <summary>Deliberately invalid replacement handler.</summary>
public sealed class WrongHandler
{
    public int Run(ExactTarget receiver, int value)
    {
        _ = receiver;
        return value;
    }
}

/// <summary>Throws to verify containment at the generated exact trampoline boundary.</summary>
public sealed class ThrowingHandler
{
    public int Run(ExactTarget receiver, int value, ref int observed)
    {
        _ = receiver;
        _ = value;
        _ = observed;
        throw new InvalidOperationException("contained patch failure");
    }
}
