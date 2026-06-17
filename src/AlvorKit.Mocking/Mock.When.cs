namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Captures a mocked method or property getter and returns a clause used to configure its return value.</summary>
    public static MockWhenClause<T> When<T>(Func<T> func)
    {
        try
        {
            Capture.Start();
            func.Invoke();

            if (Capture.Context.Instance is null || Capture.Context.Method is null || Capture.Context.Args is null)
            {
                throw new MockException(
                    $"Failed to capture method invocation for {typeof(T).FullName}. Ensure it is a valid mockable method.");
            }

            var mocked = GetMocked(Capture.Context.Instance)!;

            if (Capture.FirstMatchers.Count > 0)
            {
                var prevMethod = Capture.Context.Method;
                var prevInstance = Capture.Context.Instance;
                Capture.Disambiguate();
                func.Invoke();
                ValidateDisambiguation(prevMethod, prevInstance);
                ProcessMatchers(mocked, Capture.Context.Method, Capture.Context.Args);
            }

            return new(mocked, Capture.Context.Method, Capture.Context.Args);
        }
        finally
        {
            Capture.End();
        }
    }

    /// <summary>Captures a mocked void method or property setter and returns a clause used to configure reference outputs.</summary>
    public static MockWhenVoidClause When(Action func)
    {
        try
        {
            Capture.Start();
            func.Invoke();

            if (Capture.Context.Instance is null || Capture.Context.Method is null || Capture.Context.Args is null)
                throw new MockException("Failed to capture method invocation in action. Ensure it is a valid mockable method.");

            var mocked = GetMocked(Capture.Context.Instance)!;

            if (Capture.FirstMatchers.Count > 0)
            {
                var prevMethod = Capture.Context.Method;
                var prevInstance = Capture.Context.Instance;
                Capture.Disambiguate();
                func.Invoke();
                ValidateDisambiguation(prevMethod, prevInstance);
                ProcessMatchers(mocked, Capture.Context.Method, Capture.Context.Args);
            }

            return new(mocked, Capture.Context.Method, Capture.Context.Args);
        }
        finally
        {
            Capture.End();
        }
    }

    /// <summary>Ensures matcher disambiguation replays the same mocked call on the same instance.</summary>
    private static void ValidateDisambiguation(MethodInfo method, object instance)
    {
        if (method != Capture.Context.Method || instance != Capture.Context.Instance)
        {
            throw new MockException(
                "Failed to disambiguate matchers because method and instance changed from " +
                $"{method.Name} {instance} to {Capture.Context.Method?.Name} {Capture.Context.Instance}.");
        }
    }

    /// <summary>Maps captured matcher sentinels back onto the invocation argument array.</summary>
    private static void ProcessMatchers(Mocked mocked, MethodInfo method, object?[] args)
    {
        if (Capture.FirstMatchers.Count != Capture.SecondMatchers.Count)
        {
            throw new MockException(
                $"Failed to capture consistent matchers. Created {Capture.FirstMatchers.Count} matchers " +
                $"initally but then created {Capture.SecondMatchers.Count} matchers.");
        }

        var first = Capture.FirstFingerprints;
        var second = Capture.SecondFingerprints;
        var indices = Indices.ParameterIndices(mocked.Type, method);
        Span<int> diff = stackalloc int[128];
        int diffCount = 0;

        for (int i = 0; i < indices.Length; i++)
        {
            var index = indices[i];
            if (first[index] != second[index])
                diff[diffCount++] = index;
        }

        if (diffCount != Capture.FirstMatchers.Count)
        {
            throw new MockException(
                $"Failed to capture consistent matchers. Detected {Capture.FirstMatchers.Count} " +
                $"matchers intially but confirmed {diffCount} matchers.");
        }

        for (int i = 0; i < Capture.FirstMatchers.Count; i++)
            args[diff[i]] = Capture.FirstMatchers[i];
    }
}
