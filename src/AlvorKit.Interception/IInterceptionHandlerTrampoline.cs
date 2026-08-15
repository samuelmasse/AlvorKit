namespace AlvorKit;

/// <summary>Exact managed handler entry with in-flight acquisition and retirement semantics.</summary>
public interface IInterceptionHandlerTrampoline : IDisposable
{
    /// <summary>Gets the first contained handler exception, if any.</summary>
    Exception? Failure { get; }

    /// <summary>Reserves one invocation while this handler remains active.</summary>
    bool TryAcquire(out nint entryPoint);

    /// <summary>Consumes the first contained handler exception, if any.</summary>
    Exception? ConsumeFailure();
}
