namespace AlvorKit.Mocking;

/// <summary>Configures a captured read-only managed-reference return.</summary>
public sealed class MockRefReadonlySetupClause<T>
{
    private readonly Mocked mocked;
    private readonly MethodInfo method;
    private readonly object?[] args;

    /// <summary>Creates a read-only managed-reference setup clause.</summary>
    internal MockRefReadonlySetupClause(
        Mocked mocked,
        MethodInfo method,
        object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
    }

    /// <summary>Returns the exact stable read-only reference produced by the factory.</summary>
    public void ReturnRef(MockRefReadonlyCall<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        mocked.AddRefReadonlyReturnFactory(method, args, factory);
    }

    /// <summary>Returns a read-only reference to stable storage owned by this setup.</summary>
    public void ReturnRef(T value)
    {
        var storage = new MockRefStorage<T>(value);
        ReturnRef(storage.ReadOnly);
    }

    /// <summary>Configures the captured call to throw the supplied exception.</summary>
    public void Throw(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        mocked.AddThrow(method, args, exception);
    }
}
