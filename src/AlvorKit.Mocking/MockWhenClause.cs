namespace AlvorKit.Mocking;

/// <summary>Configures the return behavior for a captured mocked call.</summary>
public class MockWhenClause<T>
{
    /// <summary>The mock state owning the configured return.</summary>
    private readonly Mocked mocked;

    /// <summary>The captured method or accessor.</summary>
    private readonly MethodInfo method;

    /// <summary>The captured argument signature.</summary>
    private readonly object?[] args;

    /// <summary>Creates a return configuration clause for one captured call.</summary>
    internal MockWhenClause(Mocked mocked, MethodInfo method, object?[] args)
    {
        this.mocked = mocked;
        this.method = method;
        this.args = args;
    }

    /// <summary>Configures the captured call to return a value.</summary>
    public void Return(T arg)
    {
        Return(arg, []);
    }

    /// <summary>Configures the captured call to return a value and optional ref or out parameter values.</summary>
    public void Return(T arg, object?[] refs)
    {
        ReturnValues.Add(mocked, method, arg, args, refs);
    }
}
