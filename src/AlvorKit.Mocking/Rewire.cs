namespace AlvorKit.Mocking;

/// <summary>Dispatches intercepted calls into capture, event, and return-value handling.</summary>
internal static class Rewire
{
    /// <summary>Runs the non-generic rewire path once arguments have been boxed into call order.</summary>
    internal static bool Method(MethodInfo method, object instance, Mocked mocked, object?[] args, out object? rval)
    {
        rval = null;

        var vargs = args;
        var outIndices = Indices.OutParameterIndices(mocked.Type, method);
        if (outIndices.Length > 0)
        {
            vargs = [.. args];
            for (var i = 0; i < outIndices.Length; i++)
                vargs[outIndices[i]] = null;
        }

        if (Capture.Context.IsActive)
        {
            Capture.Write(instance, method, vargs);
            rval = ReturnValues.GetDefault(mocked, method);
            return true;
        }

        var ev = Events.Get(mocked, method);
        if (ev != null && vargs[0] != null)
            Events.HandleAddAndRemove(mocked, method, ev, (Delegate)vargs[0]!);

        return ReturnValues.Get(mocked, method, vargs, args, out rval);
    }
}
