namespace AlvorKit;

/// <summary>Reports whether a rewritten caller entered its exact Mocking wrapper.</summary>
internal interface IProfiledReceiverFreeHandler
{
    /// <summary>Gets the number of exact handler entries.</summary>
    int InvocationCount { get; }
}

/// <summary>Counts exact handler entries shared by receiver-free signature shapes.</summary>
internal abstract class ProfiledReceiverFreeHandler :
    IProfiledReceiverFreeHandler
{
    private int invocationCount;

    /// <summary>Gets the number of exact handler entries.</summary>
    public int InvocationCount => Volatile.Read(ref invocationCount);

    /// <summary>Records one entry before invoking the real Mocking wrapper.</summary>
    protected void Record() =>
        Interlocked.Increment(ref invocationCount);
}

internal sealed class ProfiledReceiverFreeInt32UnaryHandler(
    ProfiledReceiverFreeInt32Unary wrapper) :
    ProfiledReceiverFreeHandler
{
    public int Invoke(int value)
    {
        Record();
        return wrapper(value);
    }
}

internal sealed class ProfiledReceiverFreeStringUnaryHandler(
    ProfiledReceiverFreeStringUnary wrapper) :
    ProfiledReceiverFreeHandler
{
    public string Invoke(string value)
    {
        Record();
        return wrapper(value);
    }
}

internal sealed class ProfiledReceiverFreeInt32WriteHandler(
    ProfiledReceiverFreeInt32Write wrapper) :
    ProfiledReceiverFreeHandler
{
    public void Invoke(int value)
    {
        Record();
        wrapper(value);
    }
}

internal sealed class ProfiledReceiverFreeInt32ReadHandler(
    ProfiledReceiverFreeInt32Read wrapper) :
    ProfiledReceiverFreeHandler
{
    public int Invoke()
    {
        Record();
        return wrapper();
    }
}

internal sealed class ProfiledReceiverFreeConstructionHandler(
    ProfiledReceiverFreeConstruction wrapper) :
    ProfiledReceiverFreeHandler
{
    public ProfiledReceiverFreeTarget Invoke(int value)
    {
        Record();
        return wrapper(value);
    }
}

internal sealed class ProfiledReceiverFreeInstanceInt32ReadHandler(
    ProfiledReceiverFreeInstanceInt32Read wrapper) :
    ProfiledReceiverFreeHandler
{
    public int Invoke(ProfiledReceiverFreeTarget target)
    {
        Record();
        return wrapper(target);
    }
}

internal sealed class ProfiledReceiverFreeInstanceInt32WriteHandler(
    ProfiledReceiverFreeInstanceInt32Write wrapper) :
    ProfiledReceiverFreeHandler
{
    public void Invoke(ProfiledReceiverFreeTarget target, int value)
    {
        Record();
        wrapper(target, value);
    }
}

internal sealed class ProfiledReceiverFreeInstanceStringReadHandler(
    ProfiledReceiverFreeInstanceStringRead wrapper) :
    ProfiledReceiverFreeHandler
{
    public string? Invoke(ProfiledReceiverFreeTarget target)
    {
        Record();
        return wrapper(target);
    }
}

internal sealed class ProfiledReceiverFreeInstanceStringWriteHandler(
    ProfiledReceiverFreeInstanceStringWrite wrapper) :
    ProfiledReceiverFreeHandler
{
    public void Invoke(
        ProfiledReceiverFreeTarget target,
        string? value)
    {
        Record();
        wrapper(target, value);
    }
}
