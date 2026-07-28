namespace AlvorKit.Interception.Test;

/// <summary>Verifies the compatibility boundary for old-only backends.</summary>
[TestClass]
public sealed class InterceptionBackendCallShapeCompatibilityTest
{
    /// <summary>Ordinary shapes delegate while managed-reference shapes fail closed.</summary>
    [TestMethod]
    public void DefaultShapeOverloadNeverDropsReceiverOwnership()
    {
        var implementation = new ExactOldOnlyBackend();
        IInterceptionBackend backend = implementation;
        MethodInfo handler = typeof(ExactBackendShapeHandler)
            .GetMethod(nameof(ExactBackendShapeHandler.Invoke))!;

        using var ordinary = backend.CreateHandlerTrampoline(
            InterceptionCallShape.FromMethod(
                typeof(ExactBackendShapeTarget).GetMethod(
                    nameof(ExactBackendShapeTarget.Read))!),
            new ExactBackendShapeHandler(),
            handler,
            InterceptionHandlerExceptionPolicy.Propagate);
        Assert.AreEqual(1, implementation.LegacyCalls);

        Assert.ThrowsExactly<NotSupportedException>(() =>
            backend.CreateHandlerTrampoline(
                InterceptionCallShape.ForManagedReferenceReceiver(
                    typeof(ExactBackendShapeValue).GetMethod(
                        nameof(ExactBackendShapeValue.Read))!,
                    typeof(ExactBackendShapeValue)),
                new ExactBackendShapeHandler(),
                handler,
                InterceptionHandlerExceptionPolicy.Propagate));
        Assert.ThrowsExactly<NotSupportedException>(() =>
            backend.CreateHandlerTrampoline(
                InterceptionCallShape
                    .ForReadOnlyManagedReferenceReceiver(
                        typeof(ExactBackendShapeValue).GetMethod(
                            nameof(ExactBackendShapeValue.Read))!,
                        typeof(ExactBackendShapeValue)),
                new ExactBackendShapeHandler(),
                handler,
                InterceptionHandlerExceptionPolicy.Propagate));
        Assert.AreEqual(
            1,
            implementation.LegacyCalls,
            "Explicit receiver ownership must fail before the legacy overload.");
    }
}

public sealed class ExactBackendShapeTarget
{
    public int Read() => 1;
}

public struct ExactBackendShapeValue
{
    public readonly int Read() => 1;
}

public sealed class ExactBackendShapeHandler
{
    public int Invoke(ExactBackendShapeTarget _) => 1;
}

internal sealed class ExactOldOnlyBackend : IInterceptionBackend
{
    public InterceptionCapabilities Capabilities => default;
    public InterceptionCollisionRegistry CollisionRegistry { get; } = new();
    internal int LegacyCalls { get; private set; }

    public IInterceptionPatchHandle Install(InterceptionPlan plan) =>
        throw new NotSupportedException();

    public IInterceptionPatchHandle Install(InterceptionDispatchPlan plan) =>
        throw new NotSupportedException();

    public IInterceptionHandlerTrampoline CreateHandlerTrampoline(
        MethodInfo target,
        object? handlerInstance,
        MethodInfo handlerMethod,
        InterceptionHandlerExceptionPolicy exceptionPolicy)
    {
        LegacyCalls++;
        return new ExactBackendShapeTrampoline();
    }

    public InterceptionBackendState GetState() =>
        throw new NotSupportedException();

    public InterceptionCompletion GetCompletion(ulong requestId) =>
        throw new NotSupportedException();

    public InterceptionCompletion WaitFor(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null) =>
        throw new NotSupportedException();

    public ValueTask<InterceptionCompletion> WaitForAsync(
        ulong requestId,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class ExactBackendShapeTrampoline :
    IInterceptionHandlerTrampoline
{
    public Exception? Failure => null;
    public Exception? ConsumeFailure() => null;

    public bool TryAcquire(out nint entryPoint)
    {
        entryPoint = 1;
        return true;
    }

    public void Dispose()
    {
    }
}
